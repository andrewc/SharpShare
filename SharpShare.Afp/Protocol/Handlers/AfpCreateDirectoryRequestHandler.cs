using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpCreateDirectoryRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 6; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            requestStream.ReadUInt8(); // Padding

            ushort volumeId = requestStream.ReadUInt16();
            uint directoryId = requestStream.ReadUInt32();
            AfpPathType pathType = requestStream.ReadEnum<AfpPathType>();
            string pathName = null;

            switch (pathType) {
                case AfpPathType.kFPLongName:
                case AfpPathType.kFPShortName:
                    pathName = requestStream.ReadPascalString();
                    break;
                case AfpPathType.kFPUTF8Name:
                    pathName = requestStream.ReadUTF8StringWithHint();
                    break;
            }

            IAfpVolume volume = session.GetVolume(volumeId);

            if (volume == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IStorageContainer container = null;

            if (directoryId == 2) {
                container = volume.StorageProvider;
            } else {
                container = (volume.GetNode(directoryId) as IStorageContainer);
            }

            if (container == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IStorageContainer newContainer = container.CreateContainer(pathName);

            uint newContainerId = volume.GetNode(newContainer);

            responseStream.WriteUInt32(newContainerId);

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
