using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol {
    public static class AfpUams {
        public const string kNoUserAuthStr = "No User Authent";
        public const string kClearTextUAMStr = "Cleartxt Passwrd";
        public const string kRandNumUAMStr = "Randnum Exchange";
        public const string kTwoWayRandNumUAMStr = "2-Way Randnum";
        public const string kDHCAST128UAMStr = "DHCAST128";
        public const string kDHX2UAMStr = "DHX2";
        public const string kKerberosUAMStr = "Client Krb v2";
        public const string kReconnectUAMStr = "Recon1";
    }
}
