using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpShare.Afp {
    public interface IAfpServer : IShareServer {
        string[] Versions { get; }
        Guid Signature { get; }
    }
}
