using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Afp.Protocol.Security;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpLoginRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 18; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            string version = requestStream.ReadPascalString();
            string uam = requestStream.ReadPascalString();

            IAfpUserAuthenticationMethod method = session.Server.GetAuthenticationMethod(uam);

            if (method == null) {
                return AfpResultCode.FPBadUAM;
            }

            AfpUserAuthenticationResult result = method.Authenticate(session, version, null, null, requestStream);

            return result.Execute(session, responseStream);
        }

        #endregion
    }
}
