using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [ComImport]
    [Guid(IID.IID_IDirectSound8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSound8
    {
        [PreserveSig]
        uint CreateSoundBuffer([In]ref _DSBUFFERDESC pcDSBufferDesc, out IntPtr ppDSBuffer, IntPtr pUnkOuter);

        [PreserveSig]
        uint GetCaps(out _DSCAPS pDSCaps);

        [PreserveSig]
        uint DuplicateSoundBuffer(IntPtr pDSBufferOriginal, out IntPtr ppDSBufferDuplicate);

        [PreserveSig]
        uint SetCooperativeLevel(IntPtr hwnd, int dwLevel);

        [PreserveSig]
        uint Compact();

        [PreserveSig]
        uint GetSpeakerConfig(out int pdwSpeakerConfig);

        [PreserveSig]
        uint SetSpeakerConfig(int dwSpeakerConfig);

        [PreserveSig]
        uint Initialize(IntPtr pcGuidDevice);

        [PreserveSig]
        uint VerifyCertification(out int pdwCertified);
    }
}