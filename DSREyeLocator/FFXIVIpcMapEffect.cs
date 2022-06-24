using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSREyeLocator
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct FFXIVIpcMapEffect
    {
        [FieldOffset(0)]
        public uint InstanceContentID;

        [FieldOffset(4)]
        public uint unk_0x4;

        [FieldOffset(8)]
        public byte unk_0x8;
        
        [FieldOffset(10)]
        public byte unk_0x10;

        [FieldOffset(12)]
        public ushort unk_0x12;
    }
}
