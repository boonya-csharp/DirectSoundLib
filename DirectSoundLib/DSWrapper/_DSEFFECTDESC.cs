using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DirectSoundLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _DSEFFECTDESC
    {
        public int dwSize;
        public int dwFlags;
        public _GUID guidDSFXClass;
        public int dwReserved1;
        public int dwReserved2;
    }
}