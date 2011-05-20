using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpGetServerParamsRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 16; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            responseStream.WriteMacintoshDate(DateTime.Now);
            responseStream.WriteUInt8((byte)session.Server.Shares.Count); // Documentation says int16_t ?

            foreach (IStorageProvider share in session.Server.Shares) {
                responseStream.WriteUInt8(0); // Flags
                responseStream.WritePascalString(share.Name);
            }

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
