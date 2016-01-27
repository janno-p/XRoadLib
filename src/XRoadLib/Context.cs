namespace XRoadLib
{
    public class Context
    {
        public uint? Version { get; }

        public Context(uint? version)
        {
            Version = version;
        }
    }
}