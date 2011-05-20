using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpLoginExtRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 63; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            /*
            requestStream.ReadUInt8(); // Padding
            requestStream.ReadUInt16(); // Read unused flags

            string version = requestStream.ReadPascalString();
            string uam = requestStream.ReadPascalString();

            requestStream.ReadUInt8(); // User type always 3

            string userName = requestStream.ReadUTF8StringWithHint();
            AfpPathType pathType = requestStream.ReadEnum<AfpPathType>();

            string pathName = requestStream.ReadPascalString();

            requestStream.ReadUInt8(); // Padding
            */

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
