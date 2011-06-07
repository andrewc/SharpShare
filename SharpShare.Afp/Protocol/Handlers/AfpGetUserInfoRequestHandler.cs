using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp.Protocol.Handlers {
    public class AfpGetUserInfoRequestHandler : IAfpRequestHandler {
        private enum UserFlags : byte {
            ThisUser = 1
        }
        private enum UserBitmap : short {
            UserId = 0x1,
            PrimaryGroupId = 0x2,
            Uuid = 0x4
        }
        #region IAfpRequestHandler Members

        public byte CommandCode {
            get { return 37; }
        }

        public AfpResultCode Process(IAfpSession session, DsiHeader dsiHeader, AfpStream requestStream, AfpStream responseStream) {
            UserFlags flags = requestStream.ReadEnum<UserFlags>();
            int? userId = null;

            //if (flags != UserFlags.ThisUser) {
                userId = requestStream.ReadInt32();
           // }

            UserBitmap bitmap = requestStream.ReadEnum<UserBitmap>();

            responseStream.WriteEnum(bitmap);

            foreach (UserBitmap value in bitmap.EnumerateFlags()) {
                switch (value) {
                    case UserBitmap.UserId:
                        responseStream.WriteInt32(0);
                        break;
                    case UserBitmap.Uuid:
                        responseStream.WriteBytes(new byte[16]);
                        break;
                    case UserBitmap.PrimaryGroupId:
                        responseStream.WriteInt32(0);
                        break;
                }
            }

            return AfpResultCode.FPNoErr;
        }

        #endregion
    }
}
