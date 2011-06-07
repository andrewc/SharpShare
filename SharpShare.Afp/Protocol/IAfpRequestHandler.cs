using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Afp.Protocol;

namespace SharpShare.Afp.Protocol {
    public interface IAfpRequestHandler {
        byte CommandCode { get; }

        AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream);
    }
}
