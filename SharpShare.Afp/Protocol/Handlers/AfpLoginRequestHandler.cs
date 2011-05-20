using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpLoginRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 18; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            session.UsingAfpVersion = requestStream.ReadPascalString();
            session.UsingUam = requestStream.ReadPascalString();

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
