using OffsiteBackupClient.Gateways;
using System.IO;
using System.Linq;

namespace OffsiteBackupClient
{
    public class Client
    {

        private readonly IGateway _gateway = null;
        private readonly int _bufferSize = 0;

        public Client(IGateway gateway, int bufferSize)
        {
            _gateway = gateway;
            _bufferSize = bufferSize;
        }

        public void UploadDirectory(string path, string basePath)
        {
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

            byte[] buffer = new byte[_bufferSize]; 
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (bytesRead < _bufferSize)
                {
                    buffer = buffer.Take(bytesRead).ToArray();
                }

                for (int i = 0; i < 3; i++)
                {

                    try
                    {
                        _gateway.Upload(fileName, length, buffer);

                        break;
                    }
                    catch
                    {

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
