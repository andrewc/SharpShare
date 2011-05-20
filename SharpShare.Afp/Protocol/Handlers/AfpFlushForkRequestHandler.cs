using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpFlushForkRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 11; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
