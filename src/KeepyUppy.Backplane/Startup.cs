using Owin;

namespace KeepyUppy.Backplane
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
#if DEBUG
            app.UseErrorPage();
#endif
            app.UseWelcomePage("/");
        }
    }
}