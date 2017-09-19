using log4net;
using OffsiteBackupClient.Gateways;
using System;
using System.IO;
using System.Linq;

namespace OffsiteBackupClient
{
    public class Client
    {

        private readonly IGateway[] _gateways = null;
        private readonly int _bufferSize = 0;
        private readonly ILog _log = LogManager.GetLogger(typeof(Client));

        public Client(IGateway gateway, int bufferSize)
        {
            _gateways = new IGateway[] {
                gateway
             };

            _bufferSize = bufferSize;
        }

        public Client(IGateway[] gateways, int bufferSize)
        {
            _gateways = gateways;
            _bufferSize = bufferSize;
        }

        public void UploadDirectory(string path, string basePath)
        {
            _log.Info($"UploadDirectory(\"{path}\", \"{basePath}\")");

            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = path;
            }

            foreach (string file in Directory.GetFiles(path))
            {
                UploadFile(file, basePath);
            }

            foreach (string directory in Directory.GetDirectories(path))
            {
                UploadDirectory(directory, basePath);
            }
        }

        internal void UploadFile(string path, string basePath)
        {
            _log.Info($"UploadFile(\"{path}\", \"{basePath}\")");

            FileInfo fileInfo = new FileInfo(path);

            string fileName = ToRelativePath(path, basePath);
            long fileSize = fileInfo.Length;

            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            UploadStream(stream, fileSize, fileName);

            stream.Close();
            stream.Dispose();

            File.Delete(path);
        }

        internal void UploadStream(Stream stream, long length, string fileName)
        {

            _log.Info($"UploadStream(stream, {length}, \"{fileName}\")");

            byte[] bytes = new byte[_bufferSize];
            int bytesRead;

            while ((bytesRead = stream.Read(bytes, 0, bytes.Length)) > 0)
            {
                if (bytesRead < _bufferSize)
                {
                    bytes = bytes.Take(bytesRead).ToArray();
                }

                UploadBytes(fileName, length, bytes);
            }

        }

        internal void UploadBytes(string fileName, long length, byte[] bytes)
        {
            foreach (IGateway gateway in _gateways)
            {
                for (int i = 0; i < 3; i++)
                {

                    try
                    {
                        gateway.Upload(fileName, length, bytes);

                        break;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                    }

                }
            }
        }

        internal string ToRelativePath(string path, string basePath)
        {
            return path.Substring(basePath.Length + 1);
        }
    }
}
