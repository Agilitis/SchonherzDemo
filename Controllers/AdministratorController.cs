using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchonherzDemo.Controllers
{
	public class AdministratorController : Controller
	{
		// GET
		[Authorize(Roles = "Admins")]
		public IActionResult Index()
		{
			return
			View();
		}
	}
}