using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    [Flags]
    public enum AfpPathType : byte {
        kFPShortName = 1,
        kFPLongName = 2,
        kFPUTF8Name = 3
    }

    [Flags]
    public enum AfpFileDirectoryBitmap : ushort {
        kFPAttributeBit = 0x1,
        kFPParentDirIDBit = 0x2,
        kFPCreateDateBit = 0x4,
        kFPModDateBit = 0x8,
        kFPBackupDateBit = 0x10,
        kFPFinderInfoBit = 0x20,
        kFPLongNameBit = 0x40,
        kFPShortNameBit = 0x80,
        kFPNodeIDBit = 0x100,

        /* Bits that apply only to directories: */
        kFPOffspringCountBit = 0x0200,
        kFPOwnerIDBit = 0x0400,
        kFPGroupIDBit = 0x0800,
        kFPAccessRightsBit = 0x1000,

        /* Bits that apply only to files (same bits as previous group): */
        kFPDataForkLenBit = 0x0200,
        kFPRsrcForkLenBit = 0x0400,
        kFPExtDataForkLenBit = 0x0800, // In AFP version 3.0 and later
        kFPLaunchLimitBit = 0x1000,

        /* Bits that apply to everything except where noted: */
        //kFPProDOSInfoBit = 0x2000,      // Deprecated; AFP version 2.2 and earlier
        kFPUTF8NameBit = 0x2000,       // AFP version 3.0 and later
        kFPExtRsrcForkLenBit = 0x4000, // Files only; AFP version 3.0 and later
        kFPUnixPrivsBit = 0x8000,       // AFP version 3.0 and later
        //kFPUUID = 0x10000              // Directories only; AFP version 3.2 and later (with ACL support)

    }
}
