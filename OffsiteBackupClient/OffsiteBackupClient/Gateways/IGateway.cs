namespace OffsiteBackupClient.Gateways
{
    public interface IGateway
    {
        void Upload(string filename, long fileSize, byte[] bytes);
    }
}
