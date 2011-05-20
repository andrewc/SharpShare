using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    [Flags]
    public enum AfpAccessRightsBitmap : uint {
        All = kSPOwner | kRPOwner | kWROwner | kSPGroup | kRPGroup | kWRGroup | kSPOther | kRPOther | kWROther | kSPUser | kRPUser | kWRUser | kUserIsOwner ,
        kSPOwner = 0x1,
        kRPOwner = 0x2,
        kWROwner = 0x4,

        kSPGroup = 0x100,
        kRPGroup = 0x200,
        kWRGroup = 0x400,

        kSPOther = 0x10000,
        kRPOther = 0x20000,
        kWROther = 0x40000,

        kSPUser = 0x1000000,
        kRPUser = 0x2000000,
        kWRUser = 0x4000000,
        kBlankAcess = 0x10000000,
        kUserIsOwner = 0x80000000
    }
}
