using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Afp.Protocol.Security;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpLoginExtRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 63; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Padding
            requestStream.ReadUInt16(); // Read unused flags

            string version = requestStream.ReadPascalString();
            string uam = requestStream.ReadPascalString();

            requestStream.ReadUInt8(); // User type always 3

            string userName = requestStream.ReadUTF8String();

            requestStream.ReadUInt8();

            string pathname = requestStream.ReadUTF8String();

            requestStream.ReadPadding();

            IAfpUserAuthenticationMethod method = session.Server.GetAuthenticationMethod(uam);

            if (method == null) {
                return AfpResultCode.FPBadUAM;
            }

            session.AuthenticationMethod = method;

            AfpUserAuthenticationResult result = method.Authenticate(session, version, pathname, userName, requestStream);

            return result.Execute(session, responseStream);
        }

        #endregion
    }
}
