using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CPL20Archive.Areas.Identity.Pages.Account
{
	[AllowAnonymous]
	public class RegisterModel : PageModel
	{
		public IActionResult OnGet()
		{
			return RedirectToPage("Login");
		}

		public IActionResult OnPost()
		{
			return RedirectToPage("Login");
		}
	}
}