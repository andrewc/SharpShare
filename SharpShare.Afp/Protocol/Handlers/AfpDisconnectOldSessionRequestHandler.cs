using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpDisconnectOldSessionRequestHandler : IAfpRequestHandler{
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 65;  }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Padding
            requestStream.ReadInt16(); // Type (always zero)

            int tokenLength = requestStream.ReadInt32();
            byte[] tokenData = requestStream.ReadBytes((uint)tokenLength);

            Guid token = new Guid(tokenData);
            IAfpSession otherSession = session.TransferTo(token);

            if (otherSession == null) {
                return AfpResultCode.FPMiscErr;
            }

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
