using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Core.Controllers
{
    [Route("/")]
    public class DefaultPathController
    {
        public IResult Get()
        {
            return Results.Redirect("/ingredients");
            //return Results.Redirect("/dayplanner");
        }
    }
}
