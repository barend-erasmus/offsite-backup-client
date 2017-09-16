using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OffsiteBackupClient.Gateways
{
    public class DropboxGateway : IGateway
    {
        private string _accessToken;

        private Dictionary<string, string> _sessionIds = new Dictionary<string, string>();
        private Dictionary<string, int> _bytesUploaded = new Dictionary<string, int>();

        public DropboxGateway(string accessToken)
        {
            _accessToken = accessToken;
        }
        public void Upload(string filename, int fileSize, int offset, byte[] bytes)
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

            AppendSession(sessionId, filename, fileSize, bytes);

            if (!_bytesUploaded.ContainsKey(filename))
            {
                _bytesUploaded.Add(filename, bytes.Length);
            }
            else
            {
                _bytesUploaded[filename] += bytes.Length;
            }

            int bytesUploaded = _bytesUploaded[filename];

            if (bytesUploaded == fileSize)
            {
                EndSession(sessionId, filename, fileSize);
            }
        }

        internal void AppendSession(string sessionId, string filename, int fileSize, byte[] bytes)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/octet-stream");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_accessToken}");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Dropbox-API-Arg", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                cursor = new
                {
                    session_id = sessionId,
                    offset = fileSize
                },
                close = false
            }));

            HttpResponseMessage result = client.PostAsync("https://content.dropboxapi.com/2/files/upload_session/append_v2", new ByteArrayContent(bytes)).Result;

        }

        internal void EndSession(string sessionId, string filename, int fileSize)
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
                    path = $"{filename}",
                    mode = "add",
                    autorename = true,
                    mute = false
                }
            }));

            IRestResponse<dynamic> response = client.Execute<dynamic>(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception();
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

            return response.Data["session_id"];
        }
    }
}
