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

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Pad

            AfpSessionTokenTypes type = requestStream.ReadEnum<AfpSessionTokenTypes>();
            int idLength = requestStream.ReadInt32();
            int? timestamp = null;

            if (type == AfpSessionTokenTypes.kLoginWithTimeAndID ||
                type == AfpSessionTokenTypes.kReconnWithTimeAndID) {
                timestamp = requestStream.ReadInt32();
            }

            Guid clientToken = new Guid(requestStream.ReadBytes((uint)idLength));

            switch (type) {
                case AfpSessionTokenTypes.kLoginWithID: {
                        // Find existing session and disconnect it.
                        IAfpSession existingSession = session.Server.FindSession(clientToken, AfpSessionSearchType.ClientIssued);

                        if (existingSession != null) {
                            existingSession.Close();
                        }

                        break;
                    }
                case AfpSessionTokenTypes.kLoginWithTimeAndID: {
                        if (!timestamp.HasValue) {
                            return AfpResultCode.FPParamErr;
                        }

                        // Find an existing session.
                        IAfpSession existingSession = session.Server.FindSession(clientToken, AfpSessionSearchType.ClientIssued);

                        if (existingSession != null && existingSession != session) {
                            // Existing session found, transfer resources if timestamp matches.
                            if (!existingSession.Timestamp.HasValue || existingSession.Timestamp.Value != timestamp.Value) {
                                // Timestamp is different, close old session.
                                existingSession.Close();
                            }
                        }

                        break;
                    }
            }

            session.Timestamp = timestamp;
            session.ClientToken = clientToken;
            session.ServerToken = Guid.NewGuid();

            byte[] token = session.ServerToken.Value.ToByteArray();

            responseStream.WriteInt32(token.Length);
            responseStream.WriteBytes(token);

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
