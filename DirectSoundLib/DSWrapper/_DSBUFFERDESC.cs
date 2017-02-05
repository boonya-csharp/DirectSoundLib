using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DirectSoundLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _DSBUFFERDESC
    {
        public int dwSize;
        public int dwFlags;
        public int dwBufferBytes;
        public int dwReserved;
        public IntPtr lpwfxFormat;
        public _GUID guid3DAlgorithm;
    }
}