using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [Guid(IID.IID_IDirectSoundNotify8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundNotify8
    {
        [PreserveSig]
        uint SetNotificationPositions(int dwPositionNotifies, IntPtr pcPositionNotifies);
    }
}