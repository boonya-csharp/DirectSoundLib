using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace DirectSoundLib
{
    internal enum DSSCL
    {
        /// <summary>
        /// Sets the normal level. This level has the smoothest multitasking and resource-sharing behavior, but because it does not allow the primary buffer format to change, output is restricted to the default 8-bit format. 
        /// </summary>
        DSSCL_NORMAL = 0x00000001,
        /// <summary>
        /// Sets the priority level. Applications with this cooperative level can call the SetFormat and Compact methods. 
        /// </summary>
        DSSCL_PRIORITY = 0x00000002,
        /// <summary>
        /// For DirectX 8.0 and later, has the same effect as DSSCL_PRIORITY. For previous versions, sets the application to the exclusive level. This means that when it has the input focus, the application will be the only one audible; sounds from applications with the DSBCAPS_GLOBALFOCUS flag set will be muted. With this level, it also has all the privileges of the DSSCL_PRIORITY level. DirectSound will restore the hardware format, as specified by the most recent call to the SetFormat method, after the application gains the input focus. 
        /// </summary>
        DSSCL_EXCLUSIVE = 0x00000003,
        /// <summary>
        /// Sets the write-primary level. The application has write access to the primary buffer. No secondary buffers can be played. This level cannot be set if the DirectSound driver is being emulated for the device; that is, if the GetCaps method returns the DSCAPS_EMULDRIVER flag in the DSCAPS structure. 
        /// </summary>
        DSSCL_WRITEPRIMARY = 0x00000004,
    }

    internal enum DSBCAPS
    {
        DSBCAPS_PRIMARYBUFFER = 0x00000001,
        DSBCAPS_STATIC = 0x00000002,
        /// <summary>
        /// 缓冲区存储在声卡里, 混音是在声卡里做的
        /// </summary>
        DSBCAPS_LOCHARDWARE = 0x00000004,
        /// <summary>
        /// 缓冲区存储在内存里, 混音是CPU做的
        /// </summary>
        DSBCAPS_LOCSOFTWARE = 0x00000008,
        /// <summary>
        /// The sound source can be moved in 3D space. 
        /// </summary>
        DSBCAPS_CTRL3D = 0x00000010,
        /// <summary>
        /// 可以控制声音的频率
        /// </summary>
        DSBCAPS_CTRLFREQUENCY = 0x00000020,
        /// <summary>
        /// The sound source can be moved from left to right. 
        /// </summary>
        DSBCAPS_CTRLPAN = 0x00000040,
        /// <summary>
        /// The volume of the sound can be changed. 
        /// </summary>
        DSBCAPS_CTRLVOLUME = 0x00000080,
        /// <summary>
        /// 缓冲区通知功能
        /// </summary>
        DSBCAPS_CTRLPOSITIONNOTIFY = 0x00000100,
        /// <summary>
        /// Effects can be added to the buffer. 
        /// </summary>
        DSBCAPS_CTRLFX = 0x00000200,
        DSBCAPS_STICKYFOCUS = 0x00004000,
        /// <summary>
        /// 失去焦点继续播放功能
        /// </summary>
        DSBCAPS_GLOBALFOCUS = 0x00008000,
        DSBCAPS_GETCURRENTPOSITION2 = 0x00010000,
        DSBCAPS_MUTE3DATMAXDISTANCE = 0x00020000,
        DSBCAPS_LOCDEFER = 0x00040000,
        DSBCAPS_TRUEPLAYPOSITION = 0x00080000
    }

    internal enum DSBPLAY
    {
        /// <summary>
        /// 缓冲区播放完毕之后从缓冲区开始的位置继续播放, 当播放主缓冲区的时候必须设置DSBPLAY_LOOPING
        /// </summary>
        DSBPLAY_LOOPING = 0x00000001,
        DSBPLAY_LOCHARDWARE = 0x00000002,
        DSBPLAY_LOCSOFTWARE = 0x00000004,
        DSBPLAY_TERMINATEBY_TIME = 0x00000008,
        DSBPLAY_TERMINATEBY_DISTANCE = 0x000000010,
        DSBPLAY_TERMINATEBY_PRIORITY = 0x000000020
    }

    internal enum DSBSTATUS
    {
        DSBSTATUS_PLAYING = 0x00000001,
        DSBSTATUS_BUFFERLOST = 0x00000002,
        DSBSTATUS_LOOPING = 0x00000004,
        DSBSTATUS_LOCHARDWARE = 0x00000008,
        DSBSTATUS_LOCSOFTWARE = 0x00000010,
        DSBSTATUS_TERMINATED = 0x00000020
    }

    internal enum DSBLOCK
    {
        DSBLOCK_FROMWRITECURSOR = 0x00000001,
        DSBLOCK_ENTIREBUFFER = 0x00000002
    }

    internal enum DSBFREQUENCY
    {
        DSBFREQUENCY_ORIGINAL = 0,
        DSBFREQUENCY_MIN = 100,
#if DIRECTSOUND_VERSION_0x0900
        DSBFREQUENCY_MAX = 200000,
#else
        DSBFREQUENCY_MAX = 100000
#endif
    }

    internal enum DS3DALG
    {
        DS3DALG_DEFAULT = 0
    }

    internal class DSLibNatives
    {
        public const uint INFINITE = 0xFFFFFFFF;
        public const uint WAIT_ABANDONED_0 = 0x00000080;
        public const uint WAIT_OBJECT_0 = 0x00000000;
        /// <summary>
        /// 等待信号失败
        /// </summary>
        public const uint WAIT_FAILED = 0xFFFFFFFF;

        /// <summary>
        /// flags for wFormatTag field of WAVEFORMAT
        /// </summary>
        public const int WAVE_FORMAT_PCM = 1;


        public const uint DSBPN_OFFSETSTOP = 0xFFFFFFFF;
        public const int DSCBSTART_LOOPING = 0x00000001;

        #region DirectSound

        /// <summary>
        /// 创建一个DirectSoundCapture8接口
        /// </summary>
        /// <param name="pcGuidDevice"></param>
        /// <param name="ppDSC8"></param>
        /// <param name="pUnkOuter"></param>
        /// <returns></returns>
        [DllImport("dsound", CallingConvention = CallingConvention.StdCall)]
        public static extern uint DirectSoundCaptureCreate8(IntPtr pcGuidDevice, out IntPtr ppDSC8, IntPtr pUnkOuter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpcGuidDevice">
        /// Address of the GUID that identifies the sound device
        /// DSDEVID_DefaultPlayback : System-wide default audio playback device. Equivalent to NULL. 
        /// DSDEVID_DefaultVoicePlayback : Default voice playback device. 
        /// </param>
        /// <param name="ppDS8">Address of a variable to receive an IDirectSound8 interface pointer. </param>
        /// <param name="pUnkOuter"></param>
        /// <returns></returns>
        /// <remarks>
        /// 在创建IDirectSound8接口之后必须首先调用SetCooperativeLevel
        /// </remarks>
        [DllImport("dsound", CallingConvention = CallingConvention.StdCall)]
        public static extern uint DirectSoundCreate8(IntPtr lpcGuidDevice, out IntPtr ppDS8, IntPtr pUnkOuter);

        #endregion

        #region Kernel32

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset, [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string lpName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nCount">指定列表中的句柄数量  最大值为MAXIMUM_WAIT_OBJECTS（64）</param>
        /// <param name="lpHandles">柄数组的指针。lpHandles为指定对象句柄组合中的第一个元素 HANDLE类型可以为（Event，Mutex，Process，Thread，Semaphore）数组</param>
        /// <param name="bWaitAll">如果为TRUE，表示除非对象都发出信号，否则就一直等待下去；如果FALSE，表示任何对象发出信号即可</param>
        /// <param name="dwMilliseconds">指定要等候的毫秒数。如设为零，表示立即返回。如指定常数INFINITE，则可根据实际情况无限等待下去</param>
        /// <returns>
        /// WAIT_ABANDONED_0：所有对象都发出消息，而且其中有一个或多个属于互斥体（一旦拥有它们的进程中止，就会发出信号）
        /// WAIT_TIMEOUT：对象保持未发信号的状态，但规定的等待超时时间已经超过
        /// WAIT_OBJECT_0：所有对象都发出信号，WAIT_OBJECT_0是微软定义的一个宏，你就把它看成一个数字就可以了。例如，WAIT_OBJECT_0 + 5的返回结果意味着列表中的第5个对象发出了信号
        /// WAIT_IO_COMPLETION：（仅适用于WaitForMultipleObjectsEx）由于一个I/O完成操作已作好准备执行，所以造成了函数的返回
        /// 返回WAIT_FAILED则表示函数执行失败，会设置GetLastError
        /// </returns>
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        public static extern uint WaitForMultipleObjects(int nCount, IntPtr lpHandles, [MarshalAs(UnmanagedType.Bool)]bool bWaitAll, uint dwMilliseconds);

        /// <summary>
        /// 等待信号量
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="dwMilliseconds"></param>
        /// <returns></returns>
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        public static extern uint WaitForSingleObject(IntPtr evt, uint dwMilliseconds);

        /// <summary>
        /// 重置信号量为无信号状态
        /// </summary>
        /// <param name="hEvent"></param>
        /// <returns></returns>
        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ResetEvent(IntPtr hEvent);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("MSVCRT.DLL", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr memset(IntPtr _Dst, int _Val, uint _Size);

        #endregion
    }
}