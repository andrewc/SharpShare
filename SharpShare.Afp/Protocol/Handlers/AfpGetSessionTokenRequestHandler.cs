using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpGetSessionTokenRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 64; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Pad

            AfpSessionTokenTypes type = requestStream.ReadEnum<AfpSessionTokenTypes>();
            int idLength = requestStream.ReadInt32();

            if (type == AfpSessionTokenTypes.kLoginWithTimeAndID ||
                type == AfpSessionTokenTypes.kReconnWithTimeAndID) {
                    int timestamp = requestStream.ReadInt32();
            }

            byte[] id = requestStream.ReadBytes((uint)idLength);

            byte[] token = session.AssignToken().ToByteArray();

            responseStream.WriteInt32(token.Length);
            responseStream.WriteBytes(token);

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
