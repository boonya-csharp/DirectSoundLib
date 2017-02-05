using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _DSCBUFFERDESC
    {
        public int dwSize;

        /// <summary>
        /// 指定设备能力, 可以为0, 
        /// DSCBCAPS_CTRLFX:（支持音效的Buffer）
        /// 只支持从DirectSoundCaptureCreate8函数创建的设备对象, 需要WindowsXP版本（Capture effects require Microsoft Windows XP）
        /// DSCBCAPS_WAVEMAPPED（The Win32 wave mapper will be used for formats not supported by the device.）
        /// </summary>
        public int dwFlags;

        /// <summary>
        /// 捕获缓冲区大小, 字节为单位
        /// 缓冲区大小设置为传输速率, 那么每一个缓冲区就存储了一秒钟的声音数据
        /// </summary>
        public int dwBufferBytes;

        /// <summary>
        /// 保留字段, 供以后使用
        /// </summary>
        public int dwReserved;

        /// <summary>
        /// 要捕获的波形声音的格式信息
        /// tWAVEFORMATEX结构体指针
        /// </summary>
        public IntPtr lpwfxFormat;

        /// <summary>
        /// 一定为0, 除非dwFlag字段设置了DSCBCAPS_CTRLFX标志
        /// </summary>
        public int dwFXCount;
        public IntPtr lpDSCFXDesc;
    }
}