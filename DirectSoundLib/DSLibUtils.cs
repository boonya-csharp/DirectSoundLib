using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DirectSoundLib
{
    public class DSLibUtils
    {
        /// <summary>
        /// 结构体转byte数组, 不会释放内存
        /// </summary>
        /// <param name="structObj">要转换的结构体</param>
        /// <returns>转换后的byte数组</returns>
        public static IntPtr StructureToPtr(object structObj)
        {
            int size = Marshal.SizeOf(structObj);

            IntPtr structPtr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(structObj, structPtr, false);

            //Marshal.FreeHGlobal(structPtr);

            return structPtr;
        }

        /// <summary>
        /// 结构体转byte数组, 释放内存
        /// </summary>
        /// <returns></returns>
        public static IntPtr StructureToPtrFree(object structObj)
        {
            int size = Marshal.SizeOf(structObj);

            IntPtr structPtr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(structObj, structPtr, false);

            Marshal.FreeHGlobal(structPtr);

            return structPtr;
        }

        internal static void PrintLog(string log, params object[] param)
        {
#if DSDEBUG
            Console.WriteLine(log, param);
#endif
        }
    }
}