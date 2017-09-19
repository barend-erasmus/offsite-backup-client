using Microsoft.VisualStudio.TestTools.UnitTesting;
using OffsiteBackupClient.Gateways;
using System.Text;

namespace OffsiteBackupClient.Tests.Gateways
{
    [TestClass]
	public class SimpleCloudStorageGatewayTests
	{
        private const string _profileId = "irvuTESGyC";
        private const string _uri = "http://localhost:3000";

        [TestMethod, TestCategory("Integration")]
        public void Upload()
        {
            SimpleCloudStorageGateway gateway = new SimpleCloudStorageGateway(_profileId, _uri);

            byte[] bytes = Encoding.ASCII.GetBytes("Hello World");

            gateway.Upload("hello_world.txt", bytes.Length, bytes);
        }
    }
}
