using RopeyDVD.Models;
//using RopeyDVD.Models.Identity;
using RopeyDVD.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using RopeyDVD.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace RopeyDVD.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ApplicationDBContext _context;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly IConfiguration _configuration;

        public AuthenticationController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDBContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UserDetails()
        {
            ViewBag.User = TempData["user"];    //Passing data using ViewBag
            return View();
        }

        // Method to log user into the system
        public IActionResult Login()
        {
            return View();  //Returning user to Login View
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserLoginModel loginModel)
        {
            //Finds user using Username
            var user = await _userManager.FindByNameAsync(loginModel.Username);
            //Check if passwod if valid 
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var result = await _signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, false,false);   //Sign in using the SignInManager
                var userRoles = await _userManager.GetRolesAsync(user);
                return RedirectToAction("Index", "Home");   //Redirecting to home controller Index action

            }

            //Passing error messages to view
            ViewBag.ErrorMessage = "Invalid Email or password";     
            return View("Login");
            //return RedirectToAction("Index", "Home");
        }

        // Method to display Register assistant View
        public IActionResult Register()
        {
            return View();
        }


        // Method to Register assistants into the system
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register( UserRegisterModel model)
        {
            //Finding user using Username
            var userExists = await _userManager.FindByNameAsync(model.Username);

            //Checking if user exists and displaying message if user exists
            if (userExists != null)
            {
                ViewBag.ErrorMessage = "User already exists";
                return View("Register");
            }

            //Creating new user
            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            //Saving user to database
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            //Giving roles to user
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            return RedirectToAction("Index", "Home");
        }

        // Method to display register admin view
        public IActionResult RegisterAdmin()
        {
            return View();
        }


        // Method to register manager into the system
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdmin(UserRegisterModel model)
        {
            //Finding user using Username
            var userExists = await _userManager.FindByNameAsync(model.Username);

            //Checking if user Exists
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            //New user object
            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            //Saving user to database
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            //Giving user roles
            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
            return RedirectToAction("Index", "Home");

        }

        //Method to display Unauthorized access view
        public IActionResult UnauthorizedAccess()
        {
            return View();
        }

        //Method to log user out of the system
        public async Task<IActionResult> Logout()
        {
            //signing out user using SignInManager
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //Method to display edit user details into the edit user view
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
            {
            //Finding user using id
            var user = await _userManager.FindByIdAsync(id);

            //Checking if user is found
            if (user == null)
            {
                return View("NotFound");
            }

            //New EditUserViewModel
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
            };

            return View("EditUser",model);
        }

        //Method to update user details 
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            //Finding user using id
            var user = await _userManager.FindByIdAsync(model.Id);

            //Checking if user exists
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                //Updating user deatils
                user.Email = model.Email;
                user.UserName = model.UserName;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        //Method to delete user
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            //Finding user using id
            var user = await _userManager.FindByIdAsync(id);

            //checking if user is null
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User Id = {id} cannot be found";
                return View("NotFound");
            }
            else
            {
                //Deleting user
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers","Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return RedirectToAction("ListUsers", "Home");
            }
        }

        //Method to return List user page after cancel button is clicked
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelEditUser()
        {
            return RedirectToAction("ListUsers", "Home");
        }

        //Method to return change password view
        [Authorize(Roles = "User")]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //Method to change password
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // ChangePasswordAsync changes the user password
                var result = await _userManager.ChangePasswordAsync(user,
                    model.CurrentPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                // after successfully changing the password refresh sign-in cookie
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

    }
}
