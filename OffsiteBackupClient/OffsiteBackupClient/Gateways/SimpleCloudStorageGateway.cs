using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace OffsiteBackupClient.Gateways
{
    public class SimpleCloudStorageGateway: IGateway
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(SimpleCloudStorageGateway));

        private readonly string _profileId;
        private readonly string _uri;

        private Dictionary<string, string> _sessionIds = new Dictionary<string, string>();
        private Dictionary<string, long> _bytesUploaded = new Dictionary<string, long>();

        public SimpleCloudStorageGateway(string profileId, string uri)
        {
            _profileId = profileId;
            _uri = uri;
        }

        public void Upload(string fileName, long fileSize, byte[] bytes)
        {
            fileName = ToLinuxPath(fileName);

            _log.Info($"Upload(\"{fileName}\", {fileSize}, bytes)");

            string sessionId;

            if (!_sessionIds.ContainsKey(fileName))
            {
                sessionId = GetSessionId(fileName, fileSize);

                _sessionIds.Add(fileName, sessionId);
            }
            else
            {
                sessionId = _sessionIds[fileName];
            }

            if (!_bytesUploaded.ContainsKey(fileName))
            {
                _bytesUploaded.Add(fileName, 0);
            }

            long offset = _bytesUploaded[fileName];

            AppendSession(sessionId, fileName, offset, bytes);

            _bytesUploaded[fileName] += bytes.Length;

            long bytesUploaded = _bytesUploaded[fileName];

            if (bytesUploaded == fileSize)
            {
                EndSession(sessionId, fileName, fileSize);
            }
        }

        internal void AppendSession(string sessionId, string fileName, long offset, byte[] bytes)
        {
            _log.Info($"AppendSession(\"{sessionId}\", \"{fileName}\", {offset}, bytes)");

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", sessionId);

            HttpContent content = new ByteArrayContent(bytes);
            content.Headers.Add("Content-Type", "application/octet-stream");

            HttpResponseMessage result = client.PostAsync($"{_uri}/files/append", content).Result;

            _log.Info($"URI: POST {_uri}/files/append -> {result.StatusCode}");

            if (!result.IsSuccessStatusCode)
            {
                string errorMessage = result.Content.ReadAsStringAsync().Result;

                throw new Exception(errorMessage);
            }

        }

        internal void EndSession(string sessionId, string fileName, long fileSize)
        {
            _log.Info($"EndSession(\"{sessionId}\", \"{fileName}\", {fileSize})");

            RestClient client = new RestClient(_uri);

            RestRequest request = new RestRequest("files/finish", Method.POST);

            request.AddHeader("Content-Type", "application/octet-stream");
            request.AddHeader("Authorization", sessionId);

            IRestResponse<dynamic> response = client.Execute<dynamic>(request);

            _log.Info($"URI: POST {_uri}/files/finish -> {response.StatusCode}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                string errorMessage = response.Content;
                throw new Exception(errorMessage);
            }

        }

        internal string GetSessionId(string fileName, long fileSize)
        {
            _log.Info($"GetSessionId(\"{fileName}\", {fileSize})");

            RestClient client = new RestClient(_uri);

            RestRequest request = new RestRequest("files/start", Method.POST);

            request.AddHeader("Content-Type", "application/json");

            request.RequestFormat = DataFormat.Json;
            request.AddBody(new
            {
                fileName = fileName,
                fileSize = fileSize,
                profileId = _profileId,
            });

            IRestResponse<dynamic> response = client.Execute<dynamic>(request);

            _log.Info($"URI: POST {_uri}/files/start -> {response.StatusCode}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                string errorMessage = response.Content;
                throw new Exception(errorMessage);
            }

            return response.Data;
        }

        internal string ToLinuxPath(string path)
        {
            return path.Replace(@"\", "/");
        }
    }
}
