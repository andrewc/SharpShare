using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpCreateFileRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 7; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            byte hardCreate = requestStream.ReadUInt8();

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
                throw new StorageItemNotFoundException();
            }

            IStorageContainer container = null;

            if (directoryId == 2) {
                container = volume.StorageProvider;
            } else {
                container = (volume.GetNode(directoryId) as IStorageContainer);
            }

            if (container == null) {
                throw new StorageItemNotFoundException();
            }

            IStorageFile existingFile = container.Content(pathName) as IStorageFile;

            if (existingFile != null && hardCreate == 0) {
                return AfpResultCode.FPObjectExists;
            }

            IStorageFile newFile = container.CreateFile(pathName);

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
