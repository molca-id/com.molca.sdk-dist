namespace MolcaSDK.Media
{
    public interface IMediaHandler
    {
        void Load(string url);
        void Load(MediaInfo info);
    }
}