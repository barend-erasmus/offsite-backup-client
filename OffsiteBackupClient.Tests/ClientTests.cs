using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OffsiteBackupClient.Gateways;
using Moq;
using System.Text;
using System.IO;

namespace OffsiteBackupClient.Tests
{
    [TestClass]
    public class ClientTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Mock<IGateway> mockGateway = new Mock<IGateway>();

            IGateway gateway = mockGateway.Object;
            int bufferSize = 240;

            Client client = new Client(gateway, bufferSize);

            string content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis porta lacus ante, vitae rutrum leo ornare molestie. Nam porta commodo ultrices. Aenean in felis non urna maximus efficitur a eget leo. Sed sit amet nisi dictum, blandit ante nec, ultricies leo. Integer dictum non diam aliquet commodo. Nullam convallis euismod luctus. Interdum et malesuada fames ac ante ipsum primis in faucibus. Proin efficitur augue ac nisl ultrices, in aliquam nisi pharetra. Ut gravida augue id sapien gravida, ut bibendum urna accumsan. Donec non turpis convallis, luctus erat ac, placerat orci. Maecenas ut varius arcu. Quisque non rutrum mauris. Nunc vulputate porttitor nisl, ultrices iaculis urna mattis eu. Vestibulum eu massa facilisis, ultrices orci quis, posuere nisi.";

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            client.UploadStream(stream, content.Length, "hello_world.txt");

            mockGateway.Verify((x) => x.Upload(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<byte[]>()), Times.Exactly(4));
        }

    }
}
