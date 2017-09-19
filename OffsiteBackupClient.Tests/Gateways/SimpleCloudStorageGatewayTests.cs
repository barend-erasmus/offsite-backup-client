using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OffsiteBackupClient.Gateways;
using System.Text;

namespace OffsiteBackupClient.Tests.Gateways
{
	[TestClass]
	public class SimpleCloudStorageGatewayTests
	{
        private const string _profileId = "irvuTESGyC";

        [TestMethod, TestCategory("Integration")]
        public void Upload()
        {
            SimpleCloudStorageGateway gateway = new SimpleCloudStorageGateway(_profileId);

            byte[] bytes = Encoding.ASCII.GetBytes("Hello World");

            gateway.Upload("hello_world.txt", bytes.Length, bytes);
        }
    }
}
