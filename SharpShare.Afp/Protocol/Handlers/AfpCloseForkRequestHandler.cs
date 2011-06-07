using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpCloseForkRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 4; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8();
            short forkId = requestStream.ReadInt16();

            IAfpFork fork = session.GetFork(forkId);

            if (fork == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            fork.Close();

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
