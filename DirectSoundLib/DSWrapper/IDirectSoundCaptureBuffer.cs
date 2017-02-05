using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [ComImport]
    [Guid(IID.IID_IDirectSoundCaptureBuffer)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundCaptureBuffer
    {
        int GetCaps([In, Out]ref _DSCBCAPS pDSCBCaps);

        int GetCurrentPosition(out int pdwCapturePosition, out int pdwReadPosition);

        int GetFormat(out tWAVEFORMATEX pwfxFormat, int dwSizeAllocated, out int pdwSizeWritten);

        int GetStatus(out int pdwStatus);

        int Initialize(IntPtr pDirectSoundCapture, IntPtr pcDSCBufferDesc);

        int Lock(int dwOffset, int dwBytes, out IntPtr ppvAudioPtr1, out int pdwAudioBytes1, out IntPtr ppvAudioPtr2, out int pdwAudioBytes2, int dwFlags);

        int Start(int dwFlags);

        int Stop();

        int Unlock(IntPtr pvAudioPtr1, int dwAudioBytes1, IntPtr pvAudioPtr2, int dwAudioBytes2);
    }
}