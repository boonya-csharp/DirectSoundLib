using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _DSCEFFECTDESC
    {
        public int dwSize;
        public int dwFlags;
        public _GUID guidDSCFXClass;
        public _GUID guidDSCFXInstance;
        public int dwReserved1;
        public int dwReserved2;
    }
}