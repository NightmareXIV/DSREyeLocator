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
        public uint unk_4;

        /// <summary>
        /// We want this for eye
        /// </summary>
        [FieldOffset(8)]
        public byte unk_8;
        
        /// <summary>
        /// Possibly junk data
        /// </summary>
        [FieldOffset(10)]
        public byte unk_10;

        [FieldOffset(12)]
        public ushort unk_12;
    }
}
