using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BotDetect.Web;
using BotDetect.Web.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SchonherzDemo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        public UserManager<IdentityUser> UserManager { get;}
        
        public IServiceProvider ServiceProvider { get; }

        
        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, UserManager<IdentityUser> userManager,  IServiceProvider serviceProvider)
        {
            _signInManager = signInManager;
            _logger = logger;
            UserManager = userManager;
            ServiceProvider = serviceProvider;

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public Captcha CaptchaCode { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            await CreateRoles();
            IdentityUser[] users = {
                new IdentityUser { UserName = "Admin", Email = "Admin@admin.com"},
                new IdentityUser { UserName = "User1" ,Email = "user1@user1.com"},
                new IdentityUser { UserName = "User2",Email = "user2@user2.com"},
                new IdentityUser { UserName = "User3" , Email = "user3@user3.com"}
            };
            foreach (var user in users)
            {
                await UserManager.CreateAsync(user, "VerySecure!Password123");
                
            }

            var result1 = await UserManager.AddToRoleAsync(users[0], "Admins");
            await UserManager.AddToRoleAsync(users[1], "ContentDevelopers");
            await UserManager.AddToRoleAsync(users[2], "ContentDevelopers");
            await UserManager.AddToRoleAsync(users[3], "Users");
            
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }
        
        public async Task CreateRoles()
        {
            var RoleManager = ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = {"Admins", "ContentDevelopers", "Users"};

            foreach (var roleName in roleNames)
            {
                var result = await RoleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine(result);
            }

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
