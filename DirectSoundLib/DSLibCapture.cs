using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace DirectSoundLib
{
    /// <summary>
    /// 使用DirectSound接口实现的录音器
    /// </summary>
    public class DSLibCapture
    {
        #region 事件

        /// <summary>
        /// 录音数据到达事件
        /// 数据类型：PCM
        /// </summary>
        public event Action<byte[]> AudioDataCaptured;

        /// <summary>
        /// 录音器发生错误时发生
        /// 参数为GetLastError的返回值
        /// </summary>
        public event Action<int> OnError;

        #endregion

        #region 实例变量

        // DirectSound对象指针
        /// <summary>
        /// IDirectSoundCapture8
        /// </summary>
        private IDirectSoundCapture8 dsc8;
        private IntPtr pdsc8;

        /// <summary>
        /// IDirectSoundCaptureBuffer8
        /// </summary>
        private IDirectSoundCaptureBuffer8 dscb8;
        private IntPtr pdscb8;

        // 通知对象句柄
        private IntPtr[] notifyHwnd_close;

        // 音频结构
        private tWAVEFORMATEX wfx;
        private IntPtr pwfx_free;
        private _DSCBUFFERDESC bufferDesc;
        private IntPtr pBufferDesc_free;

        // 当前是否正在录音
        private bool isRunning = false;

        #endregion

        #region 属性

        /// <summary>
        /// 获取录制的波形声音的格式信息
        /// </summary>
        public tWAVEFORMATEX WaveFormat
        {
            get
            {
                return this.wfx;
            }
        }

        /// <summary>
        /// 获取当前是否正在录音
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        #endregion

        #region 公开接口

        public int Initialize()
        {
            uint dsErr = DSERR.DS_OK;

            if ((this.CreateIDirectSoundCapture8(out dsErr) &&
                this.CreateCaptureBuffer(out dsErr) &&
                this.CreateBufferNotifications(out dsErr)))
            {
            }

            return (int)dsErr;
        }

        public int Start()
        {
            uint dsErr = this.dscb8.Start(DSLibNatives.DSCBSTART_LOOPING);
            if (dsErr != DSERR.DS_OK)
            {
                DSLibUtils.PrintLog("开始录音失败, DSERROR = {0}", dsErr);
                return (int)dsErr;
            }

            this.isRunning = true;

            Task.Factory.StartNew((state) =>
            {
                while (this.isRunning)
                {
                    // 这里需要实时获取通知对象的指针, 因为这个指针的值每隔一段时间会改变。。。
                    IntPtr lpHandles = Marshal.UnsafeAddrOfPinnedArrayElement(this.notifyHwnd_close, 0);

                    // DSLibNatives.WaitForSingleObject(this.close_notifyHwnd[0], DSLibNatives.INFINITE);
                    switch (DSLibNatives.WaitForMultipleObjects(DSLibConsts.NotifyEvents, lpHandles, false, DSLibNatives.INFINITE))
                    {
                        case DSLibNatives.WAIT_OBJECT_0:
                            {
                                (state as SynchronizationContext).Send((o) =>
                                {
                                    try
                                    {
                                        byte[] audioData = null;
                                        if (this.RecordCapturedData(0, (uint)this.wfx.nAvgBytesPerSec, out audioData) == DSERR.DS_OK)
                                        {
                                            if (this.AudioDataCaptured != null)
                                            {
                                                this.AudioDataCaptured(audioData);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        DSLibUtils.PrintLog("保存音频流异常, Exception = {0}", ex);
                                    }
                                }, null);

                                DSLibNatives.ResetEvent(this.notifyHwnd_close[0]);
                            }
                            break;

                        case DSLibNatives.WAIT_OBJECT_0 + 1:
                            {
                                // 录音结束
                                DSLibNatives.ResetEvent(this.notifyHwnd_close[1]);

                                this.isRunning = false;
                            }
                            break;

                        case DSLibNatives.WAIT_FAILED:
                            {
                                int error = Marshal.GetLastWin32Error();

                                // 失败, 句柄已经被销毁
                                DSLibUtils.PrintLog("WAIT_FAILED, LastWin32Error = {0}", error);

                                this.isRunning = false;

                                this.Stop();

                                if (this.OnError != null)
                                {
                                    this.OnError(error);
                                }
                            }
                            break;
                    }
                }

            }, SynchronizationContext.Current);

            return (int)dsErr;
        }

        public int Stop()
        {
            uint dsErr = this.dscb8.Stop();
            if (dsErr != DSERR.DS_OK)
            {
                DSLibUtils.PrintLog("停止录音失败, DSERR = {0}", dsErr);
            }

            return (int)dsErr;
        }

        public int Release()
        {
            Marshal.FreeHGlobal(this.pBufferDesc_free);
            Marshal.FreeHGlobal(this.pwfx_free);
            Marshal.Release(this.pdscb8);
            Marshal.Release(this.pdsc8);
            //Marshal.ReleaseComObject(this.dscb8);
            //Marshal.ReleaseComObject(this.dsc8);
            Marshal.FinalReleaseComObject(this.dscb8);
            Marshal.FinalReleaseComObject(this.dsc8);

            this.pBufferDesc_free = IntPtr.Zero;
            this.pwfx_free = IntPtr.Zero;

            this.pdscb8 = IntPtr.Zero;
            this.pdsc8 = IntPtr.Zero;

            this.dscb8 = null;
            this.dsc8 = null;

            foreach (var hwnd in this.notifyHwnd_close)
            {
                DSLibNatives.CloseHandle(hwnd);
            }
            this.notifyHwnd_close = null;

            return (int)DSERR.DS_OK;
        }

        #endregion

        #region 实例方法

        private bool CreateIDirectSoundCapture8(out uint dsErr)
        {
            dsErr = DSERR.DS_OK;

            dsErr = DSLibNatives.DirectSoundCaptureCreate8(IntPtr.Zero, out this.pdsc8, IntPtr.Zero);
            if (dsErr != DSERR.DS_OK)
            {
                DSLibUtils.PrintLog("DirectSoundCaptureCreate8失败, DSERROR = {0}", dsErr);
                return false;
            }

            this.dsc8 = Marshal.GetObjectForIUnknown(this.pdsc8) as IDirectSoundCapture8;

            return true;
        }

        private bool CreateCaptureBuffer(out uint dsErr)
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

            this.bufferDesc = new _DSCBUFFERDESC()
            {
                dwFlags = 0,
                dwSize = Marshal.SizeOf(typeof(_DSCBUFFERDESC)),
                dwReserved = 0,
                dwFXCount = 0,
                dwBufferBytes = DSLibConsts.BufferSize,
                lpwfxFormat = this.pwfx_free,
                lpDSCFXDesc = IntPtr.Zero
            };

            this.pBufferDesc_free = DSLibUtils.StructureToPtr(this.bufferDesc);

            #endregion

            IntPtr pdscb;
            Guid iid_dscb8;
            dsErr = this.dsc8.CreateCaptureBuffer(this.pBufferDesc_free, out pdscb, IntPtr.Zero); //TestInvoke2(this.free_bufferDesc, out ppDSCBuff); 
            if (dsErr == DSERR.DS_OK)
            {
                // 获取IDirectSoundCaptureBuffer8接口实例
                iid_dscb8 = new Guid(IID.IID_IDirectSoundCaptureBuffer8);
                Marshal.QueryInterface(pdscb, ref iid_dscb8, out this.pdscb8);
                Marshal.Release(pdscb);
                this.dscb8 = Marshal.GetObjectForIUnknown(this.pdscb8) as IDirectSoundCaptureBuffer8;
            }
            else
            {
                DSLibUtils.PrintLog("CreateCaptureBuffer失败, DSERROR = {0}", dsErr);
                return false;
            }

            return true;
        }

        private bool CreateBufferNotifications(out uint dsErr)
        {
            dsErr = DSERR.DS_OK;

            // 获取IDirectSoundNotify8接口
            Guid iid_dsNotify8 = new Guid(IID.IID_IDirectSoundNotify8);
            IntPtr pdsNotify8;
            IDirectSoundNotify8 dsNotify8;
            Marshal.QueryInterface(this.pdscb8, ref iid_dsNotify8, out pdsNotify8);
            dsNotify8 = Marshal.GetObjectForIUnknown(pdsNotify8) as IDirectSoundNotify8;

            try
            {
                tWAVEFORMATEX wfx;
                int pdwSizeWritten;
                dsErr = this.dscb8.GetFormat(out wfx, Marshal.SizeOf(typeof(tWAVEFORMATEX)), out pdwSizeWritten);
                if (dsErr != DSERR.DS_OK)
                {
                    DSLibUtils.PrintLog("GetFormat失败, DSERROR = {0}", dsErr);
                    return false;
                }

                _DSBPOSITIONNOTIFY[] rgdsbpn = new _DSBPOSITIONNOTIFY[DSLibConsts.NotifyEvents];
                this.notifyHwnd_close = new IntPtr[DSLibConsts.NotifyEvents];
                for (int i = 0; i < DSLibConsts.NotifyEvents; i++)
                {
                    this.notifyHwnd_close[i] = DSLibNatives.CreateEvent(IntPtr.Zero, true, false, null);
                }

                rgdsbpn[0].dwOffset = (uint)(wfx.nAvgBytesPerSec - 1);
                rgdsbpn[0].hEventNotify = this.notifyHwnd_close[0];

                rgdsbpn[1].dwOffset = DSLibNatives.DSBPN_OFFSETSTOP;
                rgdsbpn[1].hEventNotify = this.notifyHwnd_close[1];

                dsErr = dsNotify8.SetNotificationPositions(DSLibConsts.NotifyEvents, Marshal.UnsafeAddrOfPinnedArrayElement(rgdsbpn, 0));
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

        private int RecordCapturedData(uint offset, uint dataSize, out byte[] audioData)
        {
            audioData = null;
            IntPtr pbCaptureData;
            int dwCaptureLength;
            IntPtr pbCaptureData2;
            int dwCaptureLength2;
            uint dsErr = DSERR.DS_OK;

            dsErr = this.dscb8.Lock(offset, dataSize, out pbCaptureData, out dwCaptureLength, out pbCaptureData2, out dwCaptureLength2, 0);
            if (dsErr != DSERR.DS_OK)
            {
                DSLibUtils.PrintLog("Lock失败, DSERROR = {0}", dsErr);
                return (int)dsErr;
            }

            // Unlock the capture buffer.
            this.dscb8.Unlock(pbCaptureData, dwCaptureLength, pbCaptureData2, dwCaptureLength2);

            // 拷贝音频数据
            int audioLength = dwCaptureLength + dwCaptureLength2;
            audioData = new byte[audioLength];
            Marshal.Copy(pbCaptureData, audioData, 0, dwCaptureLength);
            if (pbCaptureData2 != IntPtr.Zero)
            {
                Marshal.Copy(pbCaptureData2, audioData, dwCaptureLength, dwCaptureLength2);
            }

            return (int)dsErr;
        }

        #endregion

        [DllImport("DirectSound")]
        public static extern int TestInvoke2(IntPtr pdsc8, IntPtr p, out IntPtr ppDSCBuffer, IntPtr lpUn);

        [DllImport("DirectSound")]
        public static extern int TestInvoke3(IntPtr p);
    }
}