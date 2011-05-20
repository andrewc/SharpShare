using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpGetVolumeParamsRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 17; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Pad

            ushort volumeId = requestStream.ReadUInt16();
            AfpVolumeBitmap bitmap = requestStream.ReadEnum<AfpVolumeBitmap>();

            IStorageProvider provider = session.GetVolume(volumeId);

            if (provider == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            responseStream.WriteEnum(bitmap);
            responseStream.WriteVolumeInfo(session, provider, bitmap);

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
