using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    public enum AfpFileDirectoryAttributes : ushort {
        kFPInvisibleBit = 0x01,

        kFPMultiUserBit = 0x02,    // for files
        kAttrIsExpFolder = 0x02,   // for directories

        kFPSystemBit = 0x04,

        kFPDAlreadyOpenBit = 0x08, // for files
        kAttrMounted = 0x08,       // for directories

        kFPRAlreadyOpenBit = 0x10, // for files
        kAttrInExpFolder = 0x10,   // for directories

        kFPWriteInhibitBit = 0x20,
        kFPBackUpNeededBit = 0x40,
        kFPRenameInhibitBit = 0x80,
        kFPDeleteInhibitBit = 0x100,
        kFPCopyProtectBit = 0x400,
        kFPSetClearBit = 0x8000
    }
}
