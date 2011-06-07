using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    [Flags]
    public enum AfpAccessModes : short {
        Read = 1,
        Write = 2,
        DenyRead = 16,
        DenyWrite = 32
    }
}
