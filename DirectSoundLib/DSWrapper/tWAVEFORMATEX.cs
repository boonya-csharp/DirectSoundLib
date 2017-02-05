using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct tWAVEFORMATEX
    {
        public short wFormatTag;         /* format type */

        /// <summary>
        /// 通道数量
        /// </summary>
        public short nChannels;          /* number of channels (i.e. mono, stereo...) */

        /// <summary>
        /// 采样率, 每秒采样次数
        /// </summary>
        public int nSamplesPerSec;     /* sample rate */

        /// <summary>
        /// 设置声音数据的传输速率, 每秒平均传输的字节数, 单位byte/s, 如果wFormatTag = WAVE_FORMAT_PCM, nAvgBytesPerSec为nBlockAlign * nSamplesPerSec, 对于非PCM格式请根据厂商的说明计算
        /// </summary>
        public int nAvgBytesPerSec;    /* for buffer estimation */

        /// <summary>
        /// 以字节为单位设置块对齐。块对齐是指最小数据的原子大小，如果wFormatTag = WAVE_FORMAT_PCM, nBlockAlign为(nChannels * wBitsPerSample) / 8, 对于非PCM格式请根据厂商的说明计算
        /// </summary>
        public short nBlockAlign;        /* block size of data */

        /// <summary>
        /// 采样位数, 每个采样的位数
        /// 如果wFormatTag是WAVE_FORMAT_PCM, 必须设置为8或者16, 其他的不支持
        /// 如果wFormatTag是WAVE_FORMAT_EXTENSIBLE, 必须设置为8的倍数, 一些压缩方法不定义此值, 所以此值可以为0
        /// </summary>
        public short wBitsPerSample;     /* number of bits per sample of mono data */

        /// <summary>
        /// 额外信息的大小，以字节为单位，额外信息添加在WAVEFORMATEX结构的结尾。这个信息可以作为非PCM格式的wFormatTag额外属性，如果wFormatTag不需要额外的信息，此值必需为0，对于PCM格式此值被忽略。
        /// </summary>
        public short cbSize;             /* the count in bytes of the size of */
    }
}