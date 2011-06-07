using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpOpenVolumeRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 24; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Pad

            AfpVolumeBitmap volumeBitmap = requestStream.ReadEnum<AfpVolumeBitmap>();
            string volumeName = requestStream.ReadPascalString();

            IStorageProvider provider = session.Server.Shares
                .Where(s => s.Name == volumeName)
                .FirstOrDefault();

            if (provider == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IAfpVolume volume = session.OpenVolume(provider);

            if (volume == null) {
                return AfpResultCode.FPAccessDenied;
            }

            responseStream.WriteEnum(volumeBitmap);
            responseStream.WriteVolumeInfo(volume, volumeBitmap);

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
