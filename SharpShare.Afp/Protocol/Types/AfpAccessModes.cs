using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    [Flags]
    public enum AfpAccessModes : short {
        Read = 0,
        Write = 1,
        DenyRead = 4,
        DenyWrite = 5
    }
}
