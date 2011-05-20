using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    [Flags]
    public enum AfpSessionTokenTypes : short {
        kLoginWithoutID = 0,
        kLoginWithID = 1,
        kReconnWithID = 2,
        kLoginWithTimeAndID = 3,
        kReconnWithTimeAndID = 4,
        kRecon1Login = 5,
        kRecon1ReconnectLogin = 6,
        kRecon1RefreshToken = 7,
        kGetKerberosSessionKey = 8
    }
}
