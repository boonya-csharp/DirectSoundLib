using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [ComImport]
    [Guid(IID.IID_IDirectSoundCapture8)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDirectSoundCapture8
    {
        [PreserveSig]
        uint CreateCaptureBuffer([In]IntPtr pcDSCBufferDesc, [Out]out IntPtr ppDSCBuffer, [In]IntPtr pUnkOuter);

        /// <summary>
        /// 获取捕获音频设备的信息
        /// </summary>
        /// <param name="pDSCCaps">DSCCAPS结构体指针, 必须指定dwSize字段</param>
        /// <returns></returns>
        [PreserveSig]
        uint GetCaps([In, Out]IntPtr pDSCCaps);

        [PreserveSig]
        uint Initialize([In]IntPtr pcGuidDevice);
    }

    [ComImport]
    [Guid(CLSID.CLSID_DirectSoundCapture8)]
    public class DirectSoundCapture8
    {

    }
}