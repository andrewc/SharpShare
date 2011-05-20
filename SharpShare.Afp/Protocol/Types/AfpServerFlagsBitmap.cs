using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    [Flags]
    public enum AfpServerFlagsBitmap : ushort {
        kSupportsCopyfile = 0x01,
        kSupportsChgPwd = 0x02,
        kDontAllowSavePwd = 0x04,
        kSupportsSrvrMsg = 0x08,
        kSupportsSrvrSig = 0x10,
        kSupportsTCP = 0x20,
        kSupportsSrvrNotify = 0x40,
        kSupportsReconnect = 0x80,
        kSupportsDirServices = 0x100,
        kSupportsUTF8SrvrName = 0x200,
        kSupportsUUIDs = 0x400,
        kSupportsExtSleep = 0x800,
        kSupportsSuperClient = 0x8000
    }
}
