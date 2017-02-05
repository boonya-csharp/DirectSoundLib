using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _DSCBCAPS
    {
        public int dwSize;
        public int dwFlags;
        public int dwBufferBytes;
        public int dwReserved;
    }
}