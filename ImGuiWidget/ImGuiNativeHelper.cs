using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;

namespace ImGuiNETWidget
{
    public static unsafe class ImGuiNativeHelper
    {

        public static void CopyData(void* from, void* to, uint length)
        {
            uint ptrSize = (uint)IntPtr.Size;
            uint lengthAlign = (length / ptrSize) * ptrSize;
            if (ptrSize == 4)
            {
                for (uint i = 0; i < lengthAlign; i += ptrSize)
                    *((uint*)to + i) = *((uint*)from + i);
            }
            else
            {
                for (ulong i = 0; i < lengthAlign; i += ptrSize)
                    *((ulong*)to + i) = *((ulong*)from + i);
            }
            for (uint i = lengthAlign; i < length; i++)
                *((byte*)to + i) = *((byte*)from + i);
        }

        public static void ClearData(void* to, uint length)
        {
            uint ptrSize = (uint)IntPtr.Size;
            uint lengthAlign = (length / ptrSize) * ptrSize;
            if (ptrSize == 4)
            {
                for (uint i = 0; i < lengthAlign; i += ptrSize)
                    *((uint*)((uint)to + i)) = 0;
            }
            else
            {
                for (ulong i = 0; i < lengthAlign; i += ptrSize)
                    *((ulong*)((ulong)to + i)) = 0;
            }
            for (uint i = lengthAlign; i < length; i++)
                *((byte*)to + i) = 0;
        }

    }
}
