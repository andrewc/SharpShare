using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    class AfpSyncDirectoryRequestHandler : IAfpRequestHandler {
        public byte CommandCode {
            get { return 78; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            return AfpResultCode.FPNoErr;
        }
    }
    class AfpSyncForkRequestHandler : IAfpRequestHandler {
        public byte CommandCode {
            get { return 79; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            return AfpResultCode.FPNoErr;
        }
    }
}
