using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [ComImport]
    [Guid(IID.IID_IDirectSoundCaptureBuffer8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundCaptureBuffer8
    {
        [PreserveSig]
        uint GetCaps([In,Out]ref _DSCBCAPS pDSCBCaps);

        /// <summary>
        /// 获取当前捕获位置
        /// </summary>
        /// <param name="pdwCapturePosition">Capture指针在缓冲中的位置</param>
        /// <param name="pdwReadPosition">Read指针在缓冲中的位置</param>
        /// <remarks>
        /// Read指针是指名当前你可以读到哪个位置，而不是从哪个位置开始读，比如第一次取数据的时候，会取缓冲的最开始位置到Read指针所指的位置。
        /// 又由于是一个环形的缓冲，所以Capture指针的位置不一定总是大于Read指针的，可以理解成，Capture表明了在调用这个函数时正在录取的数据将要写到（为止）的位置，而Read是已经写好了的到这个位置为止的位置。
        /// 所以我们在读取数据的时候要保存一个偏移量，记录每次要读取的数据的起始位置
        /// </remarks>
        /// <returns></returns>
        [PreserveSig]
        uint GetCurrentPosition([Out]out int pdwCapturePosition, [Out]out int pdwReadPosition);

        [PreserveSig]
        uint GetFormat(out tWAVEFORMATEX pwfxFormat, int dwSizeAllocated, out int pdwSizeWritten);

        [PreserveSig]
        uint GetStatus(out int pdwStatus);

        [PreserveSig]
        uint Initialize(IntPtr pDirectSoundCapture, IntPtr pcDSCBufferDesc);

        /// <summary>
        /// The Lock method locks a portion of the buffer. Locking the buffer returns pointers into the buffer, allowing the application to read or write audio data into memory.
        /// </summary>
        /// <param name="dwOffset">Offset, in bytes, from the start of the buffer to the point where the lock begins. </param>
        /// <param name="dwBytes">Size, in bytes, of the portion of the buffer to lock. Because the buffer is conceptually circular, this number can exceed the number of bytes between dwOffset and the end of the buffer. </param>
        /// <param name="ppvAudioPtr1"></param>
        /// <param name="pdwAudioBytes1"></param>
        /// <param name="ppvAudioPtr2"></param>
        /// <param name="pdwAudioBytes2"></param>
        /// <param name="dwFlags">Flags modifying the lock event. This value can be zero or the following flag: DSCBLOCK_ENTIREBUFFER  Ignore dwBytes and lock the entire capture buffer.  </param>
        /// <returns></returns>
        [PreserveSig]
        uint Lock(uint dwOffset, uint dwBytes, out IntPtr ppvAudioPtr1, out int pdwAudioBytes1, out IntPtr ppvAudioPtr2, out int pdwAudioBytes2, int dwFlags);

        [PreserveSig]
        uint Start(int dwFlags);

        [PreserveSig]
        uint Stop();

        [PreserveSig]
        uint Unlock(IntPtr pvAudioPtr1, int dwAudioBytes1, IntPtr pvAudioPtr2, int dwAudioBytes2);

        [PreserveSig]
        uint GetObjectInPath(ref _GUID rguidObject, int dwIndex, ref _GUID rguidInterface, out IntPtr ppObject);

        [PreserveSig]
        uint GetFXStatus(int dwEffectsCount, out int pdwFXStatus);
    }
}