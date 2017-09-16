using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OffsiteBackupClient.Gateways;

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

            for (int i = 0; i < 15; i++)
            {
                gateway.Upload("hello_world.txt", 150, 0, new byte[] {
                    10, 20, 30, 40, 50, 60, 70, 80, 90, 100
                });
            }
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
