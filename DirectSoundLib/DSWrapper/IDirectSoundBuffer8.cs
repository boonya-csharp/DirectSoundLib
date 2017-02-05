using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [ComImport]
    [Guid(IID.IID_IDirectSoundBuffer8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundBuffer8
    {
        [PreserveSig]
        uint GetCaps(out _DSCBCAPS pDSBufferCaps);

        [PreserveSig]
        uint GetCurrentPosition(out int pdwCurrentPlayCursor, out int pdwCurrentWriteCursor);

        [PreserveSig]
        uint GetFormat(out tWAVEFORMATEX pwfxFormat, int dwSizeAllocated, out int pdwSizeWritten);

        [PreserveSig]
        uint GetVolume(out int plVolume);

        [PreserveSig]
        uint GetPan(out int plPan);

        [PreserveSig]
        uint GetFrequency(out int pdwFrequency);

        [PreserveSig]
        uint GetStatus(out int pdwStatus);

        [PreserveSig]
        uint Initialize(IntPtr pDirectSound, [In]ref _DSBUFFERDESC pcDSBufferDesc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwOffset">Offset, in bytes, from the start of the buffer to the point where the lock begins. This parameter is ignored if DSBLOCK_FROMWRITECURSOR is specified in the dwFlags parameter. </param>
        /// <param name="dwBytes"></param>
        /// <param name="ppvAudioPtr1"></param>
        /// <param name="pdwAudioBytes1"></param>
        /// <param name="ppvAudioPtr2"></param>
        /// <param name="pdwAudioBytes2"></param>
        /// <param name="dwFlags">
        /// DSBLOCK_FROMWRITECURSOR : Start the lock at the write cursor. The dwOffset parameter is ignored.
        /// DSBLOCK_ENTIREBUFFER : Lock the entire buffer. The dwBytes parameter is ignored.
        /// </param>
        /// <returns></returns>
        [PreserveSig]
        uint Lock(int dwOffset, int dwBytes, out IntPtr ppvAudioPtr1, out uint pdwAudioBytes1, out IntPtr ppvAudioPtr2, out uint pdwAudioBytes2, int dwFlags);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dwReserved1">必须为0</param>
        /// <param name="dwPriority">声音优先级, 当分配硬件混合资源的时候用来管理声音, 最低级别为0, 最高级别0xFFFFFFFF, 如果缓冲区创建的时候没有设置DSBCAPS_LOCDEFER标志, 那么取值必须为0</param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [PreserveSig]
        uint Play(int dwReserved1, int dwPriority, int dwFlags);

        [PreserveSig]
        uint SetCurrentPosition(int dwNewPosition);

        [PreserveSig]
        uint SetFormat([In]ref tWAVEFORMATEX pcfxFormat);

        [PreserveSig]
        uint SetVolume(int lVolume);

        [PreserveSig]
        uint SetPan(int lPan);

        [PreserveSig]
        uint SetFrequency(int dwFrequency);

        [PreserveSig]
        uint Stop();

        [PreserveSig]
        uint Unlock(IntPtr pvAudioPtr1, int dwAudioBytes1, IntPtr pvAudioPtr2, int dwAudioBytes2);

        [PreserveSig]
        uint Restore();

        [PreserveSig]
        uint SetFX(int dwEffectsCount, _DSEFFECTDESC pDSFXDesc, out int pdwResultCodes);

        [PreserveSig]
        uint AcquireResources(int dwFlags, int dwEffectsCount, out int pdwResultCodes);

        [PreserveSig]
        uint GetObjectInPath(_GUID rguidObject, int dwIndex, _GUID rguidInterface, out IntPtr ppObject);
    }
}