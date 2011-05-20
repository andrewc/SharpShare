using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    [Flags]
    public enum AfpVolumeAttributesBitmap : ushort {
        kReadOnly = 0x01,
        kHasVolumePassword = 0x02,
        kSupportsFileIDs = 0x04,
        kSupportsCatSearch = 0x08,
        kSupportsBlankAccessPrivs = 0x10,
        kSupportsUnixPrivs = 0x20,
        kSupportsUTF8Names = 0x40,
        kNoNetworkUserIDs = 0x80,
        kDefaultPrivsFromParent = 0x100,
        kNoExchangeFiles = 0x200,
        kSupportsExtAttrs = 0x400,
        kSupportsACLs = 0x800,
        kCaseSensitive = 0x1000,
        kSupportsTMLockSteal = 0x2000

    }
}
