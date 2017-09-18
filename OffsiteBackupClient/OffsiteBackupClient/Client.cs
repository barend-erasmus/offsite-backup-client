using OffsiteBackupClient.Gateways;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void UploadDirectory(string path)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                UploadFile(file);
            }

            foreach (string directory in Directory.GetDirectories(path))
            {
                foreach (string file in Directory.GetFiles(directory))
                {
                    UploadFile(file);
                }

                UploadDirectory(directory);
            }
        }

        internal void UploadFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);

            string fileName = fileInfo.Name;
            long fileSize = fileInfo.Length;

            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            UploadStream(stream, fileSize, fileName);
        }

        internal void UploadStream(Stream stream, long length, string fileName)
        {

            byte[] buffer = new byte[_bufferSize];
            int offset = 0;
            int bytesUploaded = 0;

            while (bytesUploaded < length)
            {

                int bytesRead = stream.Read(buffer, offset, _bufferSize);

                if (bytesRead < _bufferSize)
                {
                    buffer = buffer.Take(bytesRead).ToArray();
                }

                _gateway.Upload(fileName, length, buffer);

                bytesUploaded += bytesRead;

            }
        }
    }
}
