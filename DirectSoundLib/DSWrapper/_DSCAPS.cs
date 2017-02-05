using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _DSCAPS
    {
        public int dwSize;
        public int dwFlags;
        public int dwMinSecondarySampleRate;
        public int dwMaxSecondarySampleRate;
        public int dwPrimaryBuffers;
        public int dwMaxHwMixingAllBuffers;
        public int dwMaxHwMixingStaticBuffers;
        public int dwMaxHwMixingStreamingBuffers;
        public int dwFreeHwMixingAllBuffers;
        public int dwFreeHwMixingStaticBuffers;
        public int dwFreeHwMixingStreamingBuffers;
        public int dwMaxHw3DAllBuffers;
        public int dwMaxHw3DStaticBuffers;
        public int dwMaxHw3DStreamingBuffers;
        public int dwFreeHw3DAllBuffers;
        public int dwFreeHw3DStaticBuffers;
        public int dwFreeHw3DStreamingBuffers;
        public int dwTotalHwMemBytes;
        public int dwFreeHwMemBytes;
        public int dwMaxContigFreeHwMemBytes;
        public int dwUnlockTransferRateHwBuffers;
        public int dwPlayCpuOverheadSwBuffers;
        public int dwReserved1;
        public int dwReserved2;
    }
}