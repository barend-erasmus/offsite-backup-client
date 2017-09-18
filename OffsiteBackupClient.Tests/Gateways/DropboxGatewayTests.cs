using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OffsiteBackupClient.Gateways;
using System.Text;

namespace OffsiteBackupClient.Tests.Gateways
{
    [TestClass]
    public class DropboxGatewayTests
    {
        private const string _accessToken = "FZCdx-RSopAAAAAAAAABEW0D_UpucNysSeKCgSjZwXS-wIQiNByl-U-qjYRBkTeL";

        [TestMethod, TestCategory("Integration")]
        public void Upload()
        {
            DropboxGateway gateway = new DropboxGateway(_accessToken);

            byte[] bytes = Encoding.ASCII.GetBytes("Hello World");

            gateway.Upload("hello_world.txt", bytes.Length, bytes);
        }

        [TestMethod, TestCategory("Integration")]
        public void GetSessionId_ShouldReturnSessionId_GivenValidAccessToken()
        {
            DropboxGateway gateway = new DropboxGateway(_accessToken);

            string sessionId = gateway.GetSessionId();

            Assert.IsNotNull(sessionId);
        }
    }
}
