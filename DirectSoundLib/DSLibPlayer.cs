using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirectSoundLib
{
    /// <summary>
    /// 使用DirectSound接口实现的音频播放器
    /// </summary>
    public class DSLibPlayer
    {
        #region 事件

        /// <summary>
        /// 播放状态改变发生
        /// 第一个参数：状态
        /// 第二个参数：如果状态是Error, 那么是LastWin32Error, 其他状态都是0
        /// </summary>
        public event Action<DSLibPlayerStatus, int> StatusChanged;

        #endregion

        #region 类变量

        private static byte[] EmptyBuffer = new byte[DSLibConsts.BufferSize];

        #endregion

        #region 实例变量

        private IntPtr pds8;
        private IDirectSound8 ds8;

        private IntPtr pdsb8;
        private IDirectSoundBuffer8 dsb8;

        private tWAVEFORMATEX wfx;
        private IntPtr pwfx_free;

        private _DSBUFFERDESC dsbd;

        // 通知句柄
        private IntPtr[] notifyHwnd_close;

        // 是否正在播放
        private bool isPlaying = false;

        private DSLibPlayerStatus status;

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置音频源
        /// </summary>
        public Uri UriSource { get; set; }

        public Stream StreamSource { get; set; }

        public bool IsStreamingBuffer { get; set; }

        public IntPtr WindowHandle { get; set; }

        public DSLibPlayerStatus Status { get { return this.status; } }

        #endregion

        #region 公开接口

        public int Initialize()
        {
            uint dsErr = DSERR.DS_OK;

            if ((this.CreateIDirectSound8(this.WindowHandle, out dsErr) &&
                this.CreateSecondaryBuffer(out dsErr) &&
                this.CreateBufferNotifications(out dsErr)))
            {

            }

            return (int)dsErr;
        }

        public int Release()
        {
            Marshal.FreeHGlobal(this.pwfx_free);
            Marshal.Release(this.pdsb8);
            Marshal.Release(this.pds8);
            //Marshal.ReleaseComObject(this.dsb8);
            //Marshal.ReleaseComObject(this.ds8);
            Marshal.FinalReleaseComObject(this.dsb8);
            Marshal.FinalReleaseComObject(this.ds8);

            this.pwfx_free = IntPtr.Zero;

            this.pdsb8 = IntPtr.Zero;
            this.pds8 = IntPtr.Zero;

            this.dsb8 = null;
            this.ds8 = null;

            foreach (var item in notifyHwnd_close)
            {
                DSLibNatives.CloseHandle(item);
            }
            this.notifyHwnd_close = null;

            return (int)DSERR.DS_OK;
        }

        public int Play()
        {
            uint dsErr = this.dsb8.Play(0, 0, (int)DSBPLAY.DSBPLAY_LOOPING);
            if (dsErr != DSERR.DS_OK)
            {
                DSLibUtils.PrintLog("Play失败, DSERR = {0}", dsErr);
                return (int)dsErr;
            }

            this.isPlaying = true;

            this.NotifyStatusChanged(DSLibPlayerStatus.Playing, 0);

            Task.Factory.StartNew((state) =>
            {
                while (this.isPlaying)
                {
                    //IntPtr lpHandles = Marshal.UnsafeAddrOfPinnedArrayElement(this.notifyHwnd_close, 0);

                    //switch (DSLibNatives.WaitForMultipleObjects(DSLibConsts.NotifyEvents, lpHandles, false, DSLibNatives.INFINITE))
                    switch (DSLibNatives.WaitForSingleObject(this.notifyHwnd_close[0], DSLibNatives.INFINITE))
                    {
                        case DSLibNatives.WAIT_OBJECT_0:
                            {
                                if (this.StreamSource == null)
                                {
                                    // 空数据
                                    DSLibNatives.ResetEvent(this.notifyHwnd_close[0]);
                                    DSLibUtils.PrintLog("StreamSource为空, 继续等待通知");
                                    break;
                                }

                                if (!this.isPlaying)
                                {
                                    // 通知是异步的,在调用了Stop之后, 如果收到通知的速度比音频流重置的速度慢（也就是说先重置了音频流，然后又收到了一次通知）, 有可能会再次读取一次数据
                                    break;
                                }

                                byte[] buffer = new byte[this.dsbd.dwBufferBytes];
                                if (this.StreamSource.Read(buffer, 0, buffer.Length) == 0)
                                {
                                    // 没有数据
                                    if (this.IsStreamingBuffer)
                                    {
                                        DSLibUtils.PrintLog("缓冲区中没有数据, 继续等待通知");
                                        // 清空播放缓冲区的音频数据, 不然会一直播放最后一个Buffer里的数据
                                        (state as SynchronizationContext).Send((o) =>
                                        {
                                            uint e;
                                            this.WriteDataToBuffer(EmptyBuffer, out e);
                                        }, null);
                                    }
                                    else
                                    {
                                        DSLibUtils.PrintLog("缓冲区播放完毕, 停止播放");
                                        this.Stop();
                                        this.NotifyStatusChanged(DSLibPlayerStatus.Stopped, 0);
                                    }

                                    DSLibNatives.ResetEvent(this.notifyHwnd_close[0]);

                                    break;
                                }

                                    // 缓冲区通知
                                    (state as SynchronizationContext).Send((o) =>
                                    {
                                        DSLibUtils.PrintLog("播放缓冲区数据, 大小:{0}字节", buffer.Length);

                                        try
                                        {
                                            uint error;
                                            if (this.WriteDataToBuffer(buffer, out error))
                                            {
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            DSLibUtils.PrintLog("处理缓冲区回调异常, Exception = {0}", ex);
                                        }
                                    }, null);

                                DSLibNatives.ResetEvent(this.notifyHwnd_close[0]);
                            }
                            break;

                        case DSLibNatives.WAIT_OBJECT_0 + 1:
                            {
                            }
                            break;

                        case DSLibNatives.WAIT_FAILED:
                            {
                                int winErr = Marshal.GetLastWin32Error();

                                DSLibUtils.PrintLog("等待信号失败, LastWin32Error = {0}", winErr);
                                this.Stop();
                                this.NotifyStatusChanged(DSLibPlayerStatus.Error, winErr);
                            }
                            break;
                    }
                }

                Console.WriteLine("跳出循环");

            }, SynchronizationContext.Current);

            return (int)dsErr;
        }

        public int Stop()
        {
            this.isPlaying = false;

            uint dsErr = this.dsb8.Stop();
            if (dsErr != DSERR.DS_OK)
            {
                DSLibUtils.PrintLog("Stop失败, DSERR = {0}", dsErr);
            }

            DSLibNatives.SetEvent(this.notifyHwnd_close[0]);

            return (int)dsErr;
        }

        //public int Pause()
        //{
        //    return DSERR.DS_OK;
        //}

        //public int Restore()
        //{
        //    return DSERR.DS_OK;
        //}

        #endregion

        #region 实例方法

        private bool CreateIDirectSound8(IntPtr hwnd, out uint dsErr)
        {
            dsErr = DSLibNatives.DirectSoundCreate8(IntPtr.Zero, out this.pds8, IntPtr.Zero);
            if (dsErr != DSERR.DS_OK)
            {
                DSLibUtils.PrintLog("DirectSoundCreate8失败, DSERR = {0}", dsErr);
                return false;
            }

            this.ds8 = Marshal.GetObjectForIUnknown(this.pds8) as IDirectSound8;

            dsErr = this.ds8.SetCooperativeLevel(hwnd, (int)DSSCL.DSSCL_NORMAL);
            if (dsErr != DSERR.DS_OK)
            {
                DSLibUtils.PrintLog("SetCooperativeLevel失败, DSERR = {0}", dsErr);
                return false;
            }

            return true;
        }

        private bool CreateSecondaryBuffer(out uint dsErr)
        {
            dsErr = DSERR.DS_OK;

            #region 创建默认音频流格式

            this.wfx = new tWAVEFORMATEX()
            {
                nChannels = DSLibConsts.Channels,
                nSamplesPerSec = DSLibConsts.SamplesPerSec,
                wBitsPerSample = DSLibConsts.BitsPerSample,
                nBlockAlign = DSLibConsts.BlockAlign,
                nAvgBytesPerSec = DSLibConsts.Bps,
                cbSize = 0,
                wFormatTag = DSLibNatives.WAVE_FORMAT_PCM
            };

            this.pwfx_free = DSLibUtils.StructureToPtr(this.wfx);

            this.dsbd = new _DSBUFFERDESC()
            {
                dwSize = Marshal.SizeOf(typeof(_DSBUFFERDESC)),
                dwFlags = (int)DSBCAPS.DSBCAPS_CTRLPOSITIONNOTIFY | (int)DSBCAPS.DSBCAPS_GETCURRENTPOSITION2 | (int)DSBCAPS.DSBCAPS_GLOBALFOCUS,
                lpwfxFormat = this.pwfx_free,
                guid3DAlgorithm = new _GUID(),
                dwBufferBytes = DSLibConsts.BufferSize,
                dwReserved = 0
            };

            #endregion

            IntPtr pdsb;
            dsErr = this.ds8.CreateSoundBuffer(ref this.dsbd, out pdsb, IntPtr.Zero);
            if (dsErr != DSERR.DS_OK)
            {
                DSLibUtils.PrintLog("CreateSoundBuffer失败, DSERR = {0}", dsErr);
                return false;
            }

            Guid iid_dsb8 = new Guid(IID.IID_IDirectSoundBuffer8);
            Marshal.QueryInterface(pdsb, ref iid_dsb8, out this.pdsb8);
            Marshal.Release(pdsb);
            this.dsb8 = Marshal.GetObjectForIUnknown(this.pdsb8) as IDirectSoundBuffer8;

            return true;
        }

        private bool CreateBufferNotifications(out uint dsErr)
        {
            dsErr = DSERR.DS_OK;

            // 获取IDirectSoundNotify8接口
            Guid iid_dsNotify8 = new Guid(IID.IID_IDirectSoundNotify8);
            IntPtr pdsNotify8;
            IDirectSoundNotify8 dsNotify8;
            Marshal.QueryInterface(this.pdsb8, ref iid_dsNotify8, out pdsNotify8);
            dsNotify8 = Marshal.GetObjectForIUnknown(pdsNotify8) as IDirectSoundNotify8;

            try
            {
                int written;
                tWAVEFORMATEX wfx;
                dsErr = this.dsb8.GetFormat(out wfx, Marshal.SizeOf(typeof(tWAVEFORMATEX)), out written);
                if (dsErr != DSERR.DS_OK)
                {
                    DSLibUtils.PrintLog("GetFormat失败, DSERR = {0}", dsErr);
                    return false;
                }

                _DSBPOSITIONNOTIFY[] rgdsbpn = new _DSBPOSITIONNOTIFY[1];
                this.notifyHwnd_close = new IntPtr[1];
                this.notifyHwnd_close[0] = DSLibNatives.CreateEvent(IntPtr.Zero, true, false, null);

                rgdsbpn[0].dwOffset = (uint)(wfx.nAvgBytesPerSec - 1);
                rgdsbpn[0].hEventNotify = this.notifyHwnd_close[0];

                //rgdsbpn[1].dwOffset = DSLibNatives.DSBPN_OFFSETSTOP;
                //rgdsbpn[1].hEventNotify = this.notifyHwnd_close[1];

                dsErr = dsNotify8.SetNotificationPositions(1, Marshal.UnsafeAddrOfPinnedArrayElement(rgdsbpn, 0));
                if (dsErr != DSERR.DS_OK)
                {
                    DSLibUtils.PrintLog("SetNotificationPositions失败, DSERROR = {0}", dsErr);
                    return false;
                }
            }
            finally
            {
                Marshal.Release(pdsNotify8);
            }

            return true;
        }

        private bool WriteDataToBuffer(byte[] data, out uint dsErr)
        {
            IntPtr audioPtr1, audioPtr2;
            uint audioBytes1, audioBytes2;

            dsErr = this.dsb8.Lock(0, data.Length, out audioPtr1, out audioBytes1, out audioPtr2, out audioBytes2, 0);
            if (dsErr == DSERR.DSERR_BUFFERLOST)
            {
                this.dsb8.Restore();
                dsErr = this.dsb8.Lock(0, data.Length, out audioPtr1, out audioBytes1, out audioPtr2, out audioBytes2, 0);
                if (dsErr != DSERR.DS_OK)
                {
                    DSLibUtils.PrintLog("Lock失败, DSERR = {0}", dsErr);
                    return false;
                }
            }

            if (data != null && data.Length > 0)
            {
                Marshal.Copy(data, 0, audioPtr1, (int)audioBytes1);
                if (audioPtr2 != IntPtr.Zero)
                {
                    Marshal.Copy(data, (int)audioBytes1, audioPtr2, (int)audioBytes2);
                }
            }
            else
            {
                // 填充空数据
                //DSLibNatives.memset(audioPtr1, 0, audioBytes1);
                //if (audioPtr2 != IntPtr.Zero)
                //{
                //    DSLibNatives.memset(audioPtr2, 0, audioBytes2);
                //}
            }

            dsErr = this.dsb8.Unlock(audioPtr1, (int)audioBytes1, audioPtr2, (int)audioBytes2);
            if (dsErr != DSERR.DS_OK)
            {
                DSLibUtils.PrintLog("Unlock失败, DSERR = {0}", dsErr);
                return false;
            }

            return true;
        }

        private void NotifyStatusChanged(DSLibPlayerStatus status, int errCode)
        {
            this.status = status;

            if (this.StatusChanged != null)
            {
                this.StatusChanged(status, errCode);
            }
        }

        #endregion
    }

    public enum DSLibPlayerStatus
    {
        /// <summary>
        /// 空闲状态, 未播放
        /// </summary>
        Stopped,

        /// <summary>
        /// 正在播放
        /// </summary>
        Playing,

        /// <summary>
        /// 播放过程中发生错误
        /// </summary>
        Error
    }
}