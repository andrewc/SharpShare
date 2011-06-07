using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpLogoutRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 20; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            session.Logout();

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
