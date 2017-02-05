using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DirectSoundLib
{
    /// <summary>
    /// DirectSound return values
    /// </summary>
    public class DSERR
    {
        public const uint DS_OK = 0x00000000;

        public const uint DSERR_BUFFERLOST = 0x88780096;

        public const uint DSERR_INVALIDPARAM = 0x80070057;
    }
}