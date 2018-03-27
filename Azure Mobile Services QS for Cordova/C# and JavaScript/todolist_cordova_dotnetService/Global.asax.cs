using System.Web.Http;
using System.Web.Routing;

namespace todolist_cordova_dotnetService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WebApiConfig.Register();
        }
    }
}