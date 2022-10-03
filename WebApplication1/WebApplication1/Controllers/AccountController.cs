using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApplication1.Entities;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _configuration;
        public AccountController(DatabaseContext databaseContext, IConfiguration configuration)
        {
            _databaseContext = databaseContext;
            _configuration = configuration;
        }

        private string doHashedPassword(string password)
        {
            string md5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
            string salted = password + md5Salt;
            string hashed = salted.MD5();
            return hashed;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            string hashedPassword = doHashedPassword(model.Password);

            User checkUser = _databaseContext.Users.Where(u => u.UserName.ToLower() == model.UserName.ToLower() && u.Password == hashedPassword).FirstOrDefault();

            if (ModelState.IsValid)
            {
                if(checkUser != null)
                {
                    if (checkUser.Locked)
                    {
                        ModelState.AddModelError(nameof(model.UserName), "User is locked");
                        return View(model);
                    }
                    List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, checkUser.Id.ToString()));
                    claims.Add(new Claim(ClaimTypes.Name, checkUser.FullName ?? String.Empty));
                    claims.Add(new Claim(ClaimTypes.Role, checkUser.Role));
                    claims.Add(new Claim("Username", checkUser.UserName));

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);

                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    ModelState.AddModelError("", "Username or password is incorrect ");
                }
                // is valid
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {

                bool checkUser = _databaseContext.Users.Any(u => u.UserName.ToLower() == model.UserName.ToLower());

                if (checkUser)
                {
                    ModelState.AddModelError(nameof(model.UserName), " Username  is already exist");
                    return View(model);
                }

                string hashedPassword = doHashedPassword(model.Password);

                User user = new()
                {
                    UserName = model.UserName,
                    Password = hashedPassword,
                };

                _databaseContext.Users.Add(user);
                int affectedRowCount = _databaseContext.SaveChanges();

                if (affectedRowCount == 0)
                {
                    ModelState.AddModelError("", "User can not be added.");
                }
                else
                {
                    return RedirectToAction("Login");
                }
                // is valid 
            }

            return View(model);
        }


        [Authorize]
        public IActionResult Profile()
		{
			ProfileInfoLoading();

			return View();
		}

		private void ProfileInfoLoading()
		{
			Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
			User user = _databaseContext.Users.FirstOrDefault(u => u.Id == userid);

			ViewData["fullname"] = user.FullName;
		}

		[HttpPost]
        public IActionResult ProfileUpdateFullName([Required][StringLength(30)]string fullname)
        {
			if (ModelState.IsValid) 
            {
                Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = _databaseContext.Users.FirstOrDefault(u => u.Id == userid);
             
                user.FullName = fullname;
                _databaseContext.SaveChanges();
                ViewData["result"] = "UpdateFullName";             
            }

            ProfileInfoLoading();
            return View("Profile"); // For see the validation error
        }

        [HttpPost]
        public IActionResult ProfileUpdatePassword([Required][MinLength(8)][MaxLength(16)] string password)
        {
            if (ModelState.IsValid)
            {
                Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = _databaseContext.Users.FirstOrDefault(u => u.Id == userid);
                
                string hashedNewPassword = doHashedPassword(password);

                if(hashedNewPassword != user.Password)
				{
                    user.Password = hashedNewPassword;
                    _databaseContext.SaveChanges();
                    ViewData["result"] = "UpdatePassword";
                }
				else
				{
                    ViewData["result"] = "SamePassword";
				}
            }

            ProfileInfoLoading();
            return View("Profile"); // For see the validation error
        }

        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

    }
}
