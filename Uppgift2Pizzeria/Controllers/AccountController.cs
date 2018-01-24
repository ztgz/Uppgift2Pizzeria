using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite.Internal.ApacheModRewrite;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Uppgift2Pizzeria.Data;
using Uppgift2Pizzeria.Models;
using Uppgift2Pizzeria.ViewModels;

namespace Uppgift2Pizzeria.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly TomasosContext _context;

        public AccountController(UserManager<ApplicationUser> usrMgr, 
            SignInManager<ApplicationUser> signInMgr, TomasosContext context)
        {
            _userManager = usrMgr;
            _signInManager = signInMgr;
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Kund user)
        {
            if (ModelState.IsValid)
            {
                //Try to log off
                await _signInManager.SignOutAsync();

                ApplicationUser newUser = new ApplicationUser
                {
                    UserName = user.AnvandarNamn
                };

                //Try to create a new Identity user
                var result = await _userManager.CreateAsync(newUser, user.Losenord);

                //if account creation was succesfull
                if (result.Succeeded)
                {
                    //hide password in old user table
                    user.Losenord = "***";
                    
                    //add and save user details
                    _context.Kund.Add(user);
                    _context.SaveChanges();

                    //...add 'RegularUser' to AspNetUserRole
                    var currentUser = await _userManager.FindByNameAsync(newUser.UserName);
                    await _userManager.AddToRoleAsync(currentUser, "RegularUser");

                    return RedirectToAction("Login");
                }
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(UserModel userModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(userModel.AnvandarNamn, userModel.Losenord, false, false);

                //If login succeded...
                if (result.Succeeded)
                {
                    //... redirect to new page
                    return Redirect(returnUrl ?? "MyAccount");
                }
            }

            return View();
        }
        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Resturant");
        }

        public async Task<IActionResult> MyAccount()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var model = _context.Kund.FirstOrDefault(k => k.AnvandarNamn == currentUser.UserName);

            if (model != null)
                return View(model);

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MyAccount(Kund userModel)
        {
            if (ModelState.IsValid)
            {
                var kund = _context.Kund.SingleOrDefault(k => k.AnvandarNamn == userModel.AnvandarNamn);

                if (kund != null)
                {
                    kund.Namn = userModel.Namn;
                    kund.Gatuadress = userModel.Gatuadress;
                    kund.Postnr = userModel.Postnr;
                    kund.Postort = userModel.Postort;
                    kund.Email = userModel.Email;
                    kund.Telefon = userModel.Telefon;

                    _context.SaveChanges();
                }

                return View("Updated");

            }

            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserInfo(string username)
        {
            UserRoleViewModel vm = new UserRoleViewModel();
            vm.User = _context.Kund.FirstOrDefault(k => k.AnvandarNamn == username);

            vm.Role = await GetRoleOfUser(username);

            return PartialView("_UserInfo" ,vm);
        }
       
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeUserRole(string userName)
        {
            //...get the user
            ApplicationUser user = await GetApplicationUser(userName);

            //Role of the user
            string role = await GetRoleOfUser(userName);

            //If user is a regular user...
            if (role == "RegularUser")
            {
                //...remove regular user role
                await _userManager.RemoveFromRoleAsync(user, "RegularUser");

                //...and add premium user role
                await _userManager.AddToRoleAsync(user, "PremiumUser");
            }
            else if(role == "PremiumUser")
            {
                //...remove premium user role
                await _userManager.RemoveFromRoleAsync(user, "PremiumUser");

                //...and add regular user role
                await _userManager.AddToRoleAsync(user, "RegularUser");
            }

            return RedirectToAction("Users", "Admin");
        }

        private async Task<string> GetRoleOfUser(string userName)
        {
            ApplicationUser user = await GetApplicationUser(userName);

            var roles = _userManager.GetRolesAsync(user);

            //A user only have one role in this application
            return roles.Result[0];
        }

        private async Task<ApplicationUser> GetApplicationUser(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }
    }
}