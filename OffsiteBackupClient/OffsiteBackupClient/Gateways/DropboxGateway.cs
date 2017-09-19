using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace OffsiteBackupClient.Gateways
{
    public class DropboxGateway : IGateway
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(DropboxGateway));

        private readonly string _accessToken;

        private Dictionary<string, string> _sessionIds = new Dictionary<string, string>();
        private Dictionary<string, long> _bytesUploaded = new Dictionary<string, long>();

        public DropboxGateway(string accessToken)
        {
            _accessToken = accessToken;
        }

        public void Upload(string fileName, long fileSize, byte[] bytes)
        {
            fileName = ToLinuxPath(fileName);

            _log.Info($"Upload(\"{fileName}\", {fileSize}, bytes)");

            string sessionId;

            if (!_sessionIds.ContainsKey(fileName))
            {
                sessionId = GetSessionId();

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

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
            client.DefaultRequestHeaders.Add("Dropbox-API-Arg", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                cursor = new
                {
                    session_id = sessionId,
                    offset = offset
                },
                close = false
            }));

            HttpContent content = new ByteArrayContent(bytes);
            content.Headers.Add("Content-Type", "application/octet-stream");

            HttpResponseMessage result = client.PostAsync("https://content.dropboxapi.com/2/files/upload_session/append_v2", content).Result;

            _log.Info($"URI: POST https://content.dropboxapi.com/2/files/upload_session/append_v2 -> {result.StatusCode}");

            if (!result.IsSuccessStatusCode)
            {
                string errorMessage = result.Content.ReadAsStringAsync().Result;

                throw new Exception(errorMessage);
            }

        }

        internal void EndSession(string sessionId, string fileName, long fileSize)
        {
            _log.Info($"EndSession(\"{sessionId}\", \"{fileName}\", {fileSize})");

            RestClient client = new RestClient("https://content.dropboxapi.com");

            RestRequest request = new RestRequest("2/files/upload_session/finish", Method.POST);

            request.AddHeader("Content-Type", "application/octet-stream");
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            request.AddHeader("Dropbox-API-Arg", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                cursor = new
                {
                    session_id = sessionId,
                    offset = fileSize
                },
                commit = new
                {
                    path = $"/{fileName}",
                    mode = "add",
                    autorename = true,
                    mute = false
                }
            }));

            IRestResponse<dynamic> response = client.Execute<dynamic>(request);

            _log.Info($"URI: POST https://content.dropboxapi.com/2/files/upload_session/finish -> {response.StatusCode}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                string errorMessage = response.Content;
                throw new Exception(errorMessage);
            }

        }

        internal string GetSessionId()
        {
            _log.Info($"GetSessionId()");

            RestClient client = new RestClient("https://content.dropboxapi.com");

            RestRequest request = new RestRequest("2/files/upload_session/start", Method.POST);

            request.AddHeader("Content-Type", "application/octet-stream");
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            request.AddHeader("Dropbox-API-Arg", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                close = false
            }));

            IRestResponse<dynamic> response = client.Execute<dynamic>(request);

            _log.Info($"URI: POST https://content.dropboxapi.com/2/files/upload_session/start -> {response.StatusCode}");

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                string errorMessage = response.Content;
                throw new Exception(errorMessage);
            }

            return response.Data["session_id"];
        }

        internal string ToLinuxPath(string path)
        {
            return path.Replace(@"\", "/");
        }
    }
}
