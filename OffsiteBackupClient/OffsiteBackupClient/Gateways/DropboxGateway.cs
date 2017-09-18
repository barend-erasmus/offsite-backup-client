using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace OffsiteBackupClient.Gateways
{
    public class DropboxGateway : IGateway
    {
        private string _accessToken;

        private Dictionary<string, string> _sessionIds = new Dictionary<string, string>();
        private Dictionary<string, long> _bytesUploaded = new Dictionary<string, long>();

        public DropboxGateway(string accessToken)
        {
            _accessToken = accessToken;
        }
        public void Upload(string filename, long fileSize, byte[] bytes)
        {
            string sessionId;

            if (!_sessionIds.ContainsKey(filename))
            {
                sessionId = GetSessionId();

                _sessionIds.Add(filename, sessionId);
            }
            else
            {
                sessionId = _sessionIds[filename];
            }

 
            if (!_bytesUploaded.ContainsKey(filename))
            {
                _bytesUploaded.Add(filename, 0);
            }


            long offset = _bytesUploaded[filename];

            AppendSession(sessionId, filename, offset, bytes);

            _bytesUploaded[filename] += bytes.Length;

            long bytesUploaded = _bytesUploaded[filename];

            if (bytesUploaded == fileSize)
            {
                EndSession(sessionId, filename, fileSize);
            }
        }

        internal void AppendSession(string sessionId, string filename, long offset, byte[] bytes)
        {
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

            if (!result.IsSuccessStatusCode)
            {
                string errorMessage = result.Content.ReadAsStringAsync().Result;

                throw new Exception(errorMessage);
            }

        }

        internal void EndSession(string sessionId, string filename, long fileSize)
        {
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
                    path = $"/{filename}",
                    mode = "add",
                    autorename = true,
                    mute = false
                }
            }));

            IRestResponse<dynamic> response = client.Execute<dynamic>(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                string errorMessage = response.Content;
                throw new Exception(errorMessage);
            }

        }

        internal string GetSessionId()
        {
            RestClient client = new RestClient("https://content.dropboxapi.com");

            RestRequest request = new RestRequest("2/files/upload_session/start", Method.POST);

            request.AddHeader("Content-Type", "application/octet-stream");
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
            request.AddHeader("Dropbox-API-Arg", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                close = false
            }));

            IRestResponse<dynamic> response = client.Execute<dynamic>(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                string errorMessage = response.Content;
                throw new Exception(errorMessage);
            }

            return response.Data["session_id"];
        }
    }
}
