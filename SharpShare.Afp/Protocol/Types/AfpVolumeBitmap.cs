using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    public enum AfpVolumeBitmap : ushort {
        kFPVolAttributeBit = 0x1,
        kFPVolSignatureBit = 0x2,
        kFPVolCreateDateBit = 0x4,
        kFPVolModDateBit = 0x8,
        kFPVolBackupDateBit = 0x10,
        kFPVolIDBit = 0x20,
        kFPVolBytesFreeBit = 0x40,
        kFPVolBytesTotalBit = 0x80,
        kFPVolNameBit = 0x100,
        kFPVolExtBytesFreeBit = 0x200,
        kFPVolExtBytesTotalBit = 0x400,
        kFPVolBlockSizeBit = 0x800
    }
}
