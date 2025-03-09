using Hestia.UI.Core.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace Hestia.UI.Core.Controllers
{

    [Produces("text/html")]
    [ServiceFilter(typeof(ModelStateValidationFilter))]
    public class UIController : Controller
    {
    }
}
