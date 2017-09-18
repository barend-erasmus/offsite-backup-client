using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OffsiteBackupClient.Gateways
{
    public interface IGateway
    {
        void Upload(string filename, long fileSize, byte[] bytes);
    }
}
