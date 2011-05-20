using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;

namespace SharpShare.Afp.Protocol {
    public class AfpRenameRequestHandler : IAfpRequestHandler {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 28; }
        }

        public AfpResultCode Process(AfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
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

            AfpPathType newNameType = requestStream.ReadEnum<AfpPathType>();
            string newName = null;

            switch (newNameType) {
                case AfpPathType.kFPLongName:
                case AfpPathType.kFPShortName:
                    newName = requestStream.ReadPascalString();
                    break;
                case AfpPathType.kFPUTF8Name:
                    newName = requestStream.ReadUTF8StringWithHint();
                    break;
            }
            
            IStorageProvider provider = session.GetVolume(volumeId);

            if (provider == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IStorageContainer container = null;

            if (directoryId == 2) {
                container = provider;
            } else {
                container = (IStorageContainer)session.GetNode(directoryId);
            }

            if (container == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            IStorageItem renameItem = container.Content(pathName);

            if (renameItem == null) {
                return AfpResultCode.FPObjectNotFound;
            }

            renameItem.Rename(newName);

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
