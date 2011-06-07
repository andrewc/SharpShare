using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpShare.Afp.Protocol.Handlers;

namespace SharpShare.Afp.Protocol {
    public static class AfpRequestHandler {
        private static Dictionary<byte, IAfpRequestHandler> _handlers = new Dictionary<byte, IAfpRequestHandler>();

        static AfpRequestHandler() {
            Register(new AfpCatSearchExRequestHandler());
            Register(new AfpLoginRequestHandler());
            Register(new AfpGetServerParamsRequestHandler());
            Register(new AfpGetUserInfoRequestHandler());
            Register(new AfpOpenVolumeRequestHandler());
            Register(new AfpGetFileDirectoryParamsRequestHandler());
            Register(new AfpGetVolumeParamsRequestHandler());
            Register(new AfpGetSessionTokenRequestHandler());
            Register(new AfpEnumerateExt2RequestHandler());
            Register(new AfpOpenForkRequestHandler());
            Register(new AfpReadRequestHandler());
            Register(new AfpCloseForkRequestHandler());
            Register(new AfpLoginExtRequestHandler());
            Register(new AfpDisconnectOldSessionRequestHandler());
            Register(new AfpCloseVolumeRequestHandler());
            Register(new AfpWriteExtRequestHandler());
            Register(new AfpCreateDirectoryRequestHandler());
            Register(new AfpCreateFileRequestHandler());
            Register(new AfpSetDirectoryParamsRequestHandler());
            Register(new AfpSetFileParamsRequestHandler());
            Register(new AfpDeleteRequestHandler());
            Register(new AfpRenameRequestHandler());
            Register(new AfpFlushForkRequestHandler());
            Register(new AfpSyncForkRequestHandler());
            Register(new AfpMoveRenameRequestHandler());
            Register(new AfpLogoutRequestHandler());
            Register(new AfpResolveIDRequestHandler());
            Register(new AfpSyncDirectoryRequestHandler());
            Register(new AfpSyncForkRequestHandler());
            Register(new AfpLoginContinueRequestHandler());
        }

        public static void Register(IAfpRequestHandler handler) {
            _handlers[handler.CommandCode] = handler;
        }
        public static IAfpRequestHandler Find(byte commandCode) {
            if (_handlers.ContainsKey(commandCode)) {
                return _handlers[commandCode];
            }

            return null;
        }
    }
}
