using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpCloseVolumeRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 2; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Padding
            ushort volumeId = requestStream.ReadUInt16();

            IAfpVolume volume = session.GetVolume(volumeId);

            if (volume == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            volume.Close();

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
