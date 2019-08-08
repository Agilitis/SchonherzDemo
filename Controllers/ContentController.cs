using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchonherzDemo.Controllers
{
	[Authorize(Roles = "Admins,ContentDevelopers")]
	public class ContentController : Controller
	{
		// GET
		public IActionResult Index()
		{
			return
			View();
		}
	}
}