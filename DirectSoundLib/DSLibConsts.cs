using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DirectSoundLib
{
    /// <summary>
    /// DirectSoundLib使用的常量定义
    /// </summary>
    public class DSLibConsts
    {
        /// <summary>
        /// 通知对象个数
        /// </summary>
        public static readonly int NotifyEvents = 2;

        /// <summary>
        /// 通道数量
        /// </summary>
        public static readonly short Channels = 2;

        /// <summary>
        /// 采样率
        /// </summary>
        public static readonly int SamplesPerSec = 44100;

        /// <summary>
        /// 采样位数
        /// </summary>
        public static readonly short BitsPerSample = 16;

        /// <summary>
        /// 块对齐, 每个采样的字节数
        /// </summary>
        public static readonly short BlockAlign = (short)(Channels * BitsPerSample / 8);

        /// <summary>
        /// 捕获缓冲区大小和播放缓冲区大小
        /// </summary>
        public static readonly int BufferSize = BlockAlign * SamplesPerSec;

        /// <summary>
        /// 音频每秒传输速率
        /// </summary>
        public static readonly int Bps = DSLibConsts.BlockAlign * DSLibConsts.SamplesPerSec;
    }
}