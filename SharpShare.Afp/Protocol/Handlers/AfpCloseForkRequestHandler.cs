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

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8();
            short forkId = requestStream.ReadInt16();

            AfpOpenFileInfo info = session.FindFork(forkId);

            if (info == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            session.CloseFork(info);

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
