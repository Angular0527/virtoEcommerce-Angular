﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using VirtoCommerce.Client.Api;
using VirtoCommerce.Client.Model;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Controllers
{
    [RoutePrefix("account")]
    public class AccountController : Controller
    {
        private readonly WorkContext _workContext;
        private IStorefrontUrlBuilder _urlBuilder;
        private readonly ICommerceCoreModuleApi _commerceCoreApi;
        private readonly ICustomerManagementModuleApi _customerApi;

        public AccountController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder, ICommerceCoreModuleApi commerceCoreApi, ICustomerManagementModuleApi customerApi)
        {
            _workContext = workContext;
            _urlBuilder = urlBuilder;
            _commerceCoreApi = commerceCoreApi;
            _customerApi = customerApi;
        }

        private IAuthenticationManager _authenticationManager;
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return _authenticationManager ?? (_authenticationManager = HttpContext.GetOwinContext().Authentication);
            }
        }

        [HttpGet]
        [Route("")]
        public ActionResult Index(int page = 1)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                _workContext.CurrentPage = page;
                return View("customers/account", _workContext);
            }

            return Redirect("~/account/login");
        }

        [HttpGet]
        [Route("register")]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View("customers/register", _workContext);
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register(Register formModel)
        {
            var user = new VirtoCommercePlatformCoreSecurityApplicationUserExtended
            {
                Email = formModel.Email,
                Password = formModel.Password,
                UserName = formModel.Email,
            };

            var result = await _commerceCoreApi.StorefrontSecurityCreateAsync(user);

            if (result.Succeeded == true)
            {
                user = await _commerceCoreApi.StorefrontSecurityGetUserByNameAsync(user.UserName);

                var contact = new VirtoCommerceCustomerModuleWebModelContact
                {
                    Id = user.Id,
                    Emails = new List<string> { formModel.Email },
                    FullName = string.Join(" ", formModel.FirstName, formModel.LastName),
                };

                if (string.IsNullOrEmpty(contact.FullName))
                {
                    contact.FullName = formModel.Email;
                }

                contact = await _customerApi.CustomerModuleCreateContactAsync(contact);

                await _commerceCoreApi.StorefrontSecurityPasswordSignInAsync(formModel.Email, formModel.Password);

                var identity = CreateClaimsIdentity(formModel.Email);
                AuthenticationManager.SignIn(identity);
                return Redirect("~/account");
            }
            else
            {
                ModelState.AddModelError("form", result.Errors.First());
            }

            return View("customers/register", _workContext);
        }

        [HttpGet]
        [Route("login")]
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                AuthenticationManager.SignOut();
            }

            _workContext.Login = new Login();

            return View("customers/login", _workContext);
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(Login formModel, string returnUrl)
        {
            var loginResult = await _commerceCoreApi.StorefrontSecurityPasswordSignInAsync(formModel.Email, formModel.Password);

            switch (loginResult.Status)
            {
                case "success":
                    var identity = CreateClaimsIdentity(formModel.Email);
                    AuthenticationManager.SignIn(identity);
                    return Redirect(returnUrl);
                case "lockedOut":
                    return View("lockedout", _workContext);
                case "requiresVerification":
                    return Redirect("~/account/sendcode");
                case "failure":
                default:
                    ModelState.AddModelError("form", "Login attempt failed.");
                    return View("customers/login", _workContext);
            }
        }

        [HttpGet]
        [Route("logout")]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut();
            return Redirect("~/");
        }

        [HttpPost]
        [Route("forgotpassword")]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(ForgotPassword formModel)
        {
            var user = await _commerceCoreApi.StorefrontSecurityGetUserByNameAsync(formModel.Email);

            if (user != null)
            {
                string callbackUrl = Url.Action("ResetPassword", "Account",
                    new { UserId = user.Id, Code = "token" }, protocol: Request.Url.Scheme);

                await _commerceCoreApi.StorefrontSecurityGenerateResetPasswordTokenAsync(user.Id, _workContext.CurrentStore.Id, _workContext.CurrentLanguage.CultureName, callbackUrl);
            }
            else
            {
                ModelState.AddModelError("form", "User not found");
            }

            return Redirect("~/account/login#recover");
        }

        [HttpGet]
        [Route("resetpassword")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string code, string userId)
        {
            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(userId))
            {
                _workContext.ErrorMessage = "Error in URL format";

                return View("error", _workContext);
            }

            var user = await _commerceCoreApi.StorefrontSecurityGetUserByIdAsync(userId);
            if (user == null)
            {
                _workContext.ErrorMessage = "User was not found.";
                return View("error", _workContext);
            }

            var tokenCookie = new HttpCookie(StorefrontConstants.PasswordResetTokenCookie, code);
            tokenCookie.Expires = DateTime.UtcNow.AddDays(1);
            HttpContext.Response.Cookies.Add(tokenCookie);

            var customerIdCookie = new HttpCookie(StorefrontConstants.CustomerIdCookie, userId);
            customerIdCookie.Expires = DateTime.UtcNow.AddDays(1);
            HttpContext.Response.Cookies.Add(customerIdCookie);

            return View("customers/reset_password", _workContext);
        }

        [HttpPost]
        [Route("resetpassword")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPassword formModel)
        {
            var customerIdCookie = HttpContext.Request.Cookies[StorefrontConstants.CustomerIdCookie];
            string userId = customerIdCookie != null ? customerIdCookie.Value : null;

            var tokenCookie = HttpContext.Request.Cookies[StorefrontConstants.PasswordResetTokenCookie];
            string token = tokenCookie != null ? tokenCookie.Value : null;

            if (userId == null && token == null)
            {
                _workContext.ErrorMessage = "Not enough info for reseting password";
                return View("error", _workContext);
            }

            var result = await _commerceCoreApi.StorefrontSecurityResetPasswordAsync(userId, token, formModel.Password);

            if (result.Succeeded == true)
            {
                HttpContext.Response.Cookies.Add(new HttpCookie(StorefrontConstants.CustomerIdCookie) { Expires = DateTime.UtcNow.AddDays(-1) });
                HttpContext.Response.Cookies.Add(new HttpCookie(StorefrontConstants.PasswordResetTokenCookie) { Expires = DateTime.UtcNow.AddDays(-1) });

                return View("customers/reset_password_confirmation", _workContext);
            }
            else
            {
                ModelState.AddModelError("form", result.Errors.First());
            }

            return View("customers/reset_password", _workContext);
        }


        protected override RedirectResult Redirect(string url)
        {
            var newUrl = Url.IsLocalUrl(url) ? url : "~/";
            var appRelativeUrl = _urlBuilder.ToAppRelative(_workContext, newUrl, _workContext.CurrentStore, _workContext.CurrentLanguage);
            return base.Redirect(appRelativeUrl);
        }


        private ClaimsIdentity CreateClaimsIdentity(string userName)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, userName));

            var identity = new ClaimsIdentity(claims, Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie);

            return identity;
        }
    }
}
