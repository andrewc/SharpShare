using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpSyncDirectoryRequestHandler : IAfpRequestHandler {
        public byte CommandCode {
            get { return 78; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            return AfpResultCode.FPNoErr;
        }
    }
}
