namespace KeepyUppy.Interop
{
    public class TestUrlProvider : IUrlProvider
    {
        public string BackplaneUrl { get { return "http://localhost:8888"; } }
        public string ServiceUrl { get { return "http://localhost:8889"; } }
    }
}