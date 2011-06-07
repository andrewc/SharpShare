using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Storage;
using SharpShare.Diagnostics;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpOpenForkRequestHandler : IAfpRequestHandler, ILogProvider {
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 26; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            byte flag = requestStream.ReadUInt8();
            ushort volumeId = requestStream.ReadUInt16();
            uint directoryId = requestStream.ReadUInt32();

            bool resourceFork = (flag == 0x80);

            if (resourceFork) {
                Log.Add(this, EntryType.Error, "Session '{0}' attempted to open resource fork, failing...", session);
                return AfpResultCode.FPMiscErr;
            }

            AfpFileDirectoryBitmap bitmap = requestStream.ReadEnum<AfpFileDirectoryBitmap>();
            AfpAccessModes accessMode = requestStream.ReadEnum<AfpAccessModes>();
            AfpPathType pathType = requestStream.ReadEnum<AfpPathType>();
            string path = null;

            switch (pathType) {
                case AfpPathType.kFPLongName:
                case AfpPathType.kFPShortName:
                    path = requestStream.ReadPascalString();
                    break;
                case AfpPathType.kFPUTF8Name:
                    path = requestStream.ReadUTF8StringWithHint();
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

            IStorageFile file = (container.Content(path) as IStorageFile);

            if (file == null) {
                throw new StorageItemNotFoundException();
            }

            IAfpFork info = session.OpenFork(file, accessMode);

            if (info == null) {
                throw new StorageItemNotFoundException();
            }

            responseStream.WriteEnum<AfpFileDirectoryBitmap>(bitmap);
            responseStream.WriteInt16(info.Identifier);
            responseStream.WriteStorageFileInfo(volume, file, bitmap);

            return AfpResultCode.FPNoErr;

        }

        #endregion

        string ILogProvider.Name {
            get { return "AFP Open Fork"; }
        }
    }
}
