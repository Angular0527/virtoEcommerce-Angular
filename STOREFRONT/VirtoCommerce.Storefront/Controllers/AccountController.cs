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
    [Authorize]
    public class AccountController : StorefrontControllerBase
    {
        private readonly ICommerceCoreModuleApi _commerceCoreApi;
        private readonly ICustomerManagementModuleApi _customerApi;
        private readonly IAuthenticationManager _authenticationManager;
        private readonly IVirtoCommercePlatformApi _platformApi;

        public AccountController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder, ICommerceCoreModuleApi commerceCoreApi, ICustomerManagementModuleApi customerApi, IAuthenticationManager authenticationManager, IVirtoCommercePlatformApi platformApi)
            : base(workContext, urlBuilder)
        {
            _commerceCoreApi = commerceCoreApi;
            _customerApi = customerApi;
            _authenticationManager = authenticationManager;
            _platformApi = platformApi;
        }

        [HttpGet]
        [Route("")]
        public ActionResult Index(int page = 1)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                base.WorkContext.CurrentPage = page;
                return View("customers/account", base.WorkContext);
            }

            return StoreFrontRedirect("~/account/login");
        }

        [HttpGet]
        [Route("register")]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View("customers/register", base.WorkContext);
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
                _authenticationManager.SignIn(identity);
                return StoreFrontRedirect("~/account");
            }
            else
            {
                ModelState.AddModelError("form", result.Errors.First());
            }

            return View("customers/register", base.WorkContext);
        }

        [HttpGet]
        [Route("login")]
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                _authenticationManager.SignOut();
            }

            base.WorkContext.Login = new Login();

            return View("customers/login", base.WorkContext);
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
                    _authenticationManager.SignIn(identity);
                    return StoreFrontRedirect(returnUrl);
                case "lockedOut":
                    return View("lockedout", base.WorkContext);
                case "requiresVerification":
                    return StoreFrontRedirect("~/account/sendcode");
                case "failure":
                default:
                    ModelState.AddModelError("form", "Login attempt failed.");
                    return View("customers/login", base.WorkContext);
            }
        }

        [HttpGet]
        [Route("logout")]
        public ActionResult Logout()
        {
            _authenticationManager.SignOut();
            return StoreFrontRedirect("~/");
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

                await _commerceCoreApi.StorefrontSecurityGenerateResetPasswordTokenAsync(user.Id, base.WorkContext.CurrentStore.Id, base.WorkContext.CurrentLanguage.CultureName, callbackUrl);
            }
            else
            {
                ModelState.AddModelError("form", "User not found");
            }

            return StoreFrontRedirect("~/account/login#recover");
        }

        [HttpGet]
        [Route("resetpassword")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string code, string userId)
        {
            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(userId))
            {
                base.WorkContext.ErrorMessage = "Error in URL format";

                return View("error", base.WorkContext);
            }

            var user = await _commerceCoreApi.StorefrontSecurityGetUserByIdAsync(userId);
            if (user == null)
            {
                base.WorkContext.ErrorMessage = "User was not found.";
                return View("error", base.WorkContext);
            }

            var tokenCookie = new HttpCookie(StorefrontConstants.PasswordResetTokenCookie, code);
            tokenCookie.Expires = DateTime.UtcNow.AddDays(1);
            HttpContext.Response.Cookies.Add(tokenCookie);

            var customerIdCookie = new HttpCookie(StorefrontConstants.CustomerIdCookie, userId);
            customerIdCookie.Expires = DateTime.UtcNow.AddDays(1);
            HttpContext.Response.Cookies.Add(customerIdCookie);

            return View("customers/reset_password", base.WorkContext);
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
                base.WorkContext.ErrorMessage = "Not enough info for reseting password";
                return View("error", base.WorkContext);
            }

            var result = await _commerceCoreApi.StorefrontSecurityResetPasswordAsync(userId, token, formModel.Password);

            if (result.Succeeded == true)
            {
                HttpContext.Response.Cookies.Add(new HttpCookie(StorefrontConstants.CustomerIdCookie) { Expires = DateTime.UtcNow.AddDays(-1) });
                HttpContext.Response.Cookies.Add(new HttpCookie(StorefrontConstants.PasswordResetTokenCookie) { Expires = DateTime.UtcNow.AddDays(-1) });

                return View("customers/reset_password_confirmation", base.WorkContext);
            }
            else
            {
                ModelState.AddModelError("form", result.Errors.First());
            }

            return View("customers/reset_password", base.WorkContext);
        }

        [HttpPost]
        [Route("profile")]
        public async Task<ActionResult> UpdateProfile(Profile formModel)
        {
            var contact = new VirtoCommerceCustomerModuleWebModelContact
            {
                Id = base.WorkContext.CurrentCustomer.Id
            };

            var fullName = string.Join(" ", formModel.FirstName, formModel.LastName).Trim();

            if (string.IsNullOrEmpty(fullName))
            {
                fullName = formModel.Email;
            }

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                contact.FullName = fullName;
            }

            if (!string.IsNullOrWhiteSpace(formModel.Email))
            {
                contact.Emails = new List<string> { formModel.Email };
            }

            await _customerApi.CustomerModuleUpdateContactAsync(contact);

            return View("customers/account", base.WorkContext);
        }

        [HttpPost]
        [Route("password")]
        public async Task<ActionResult> ChangePassword(ChangePassword formModel)
        {
            var changePassword = new VirtoCommercePlatformWebModelSecurityChangePasswordInfo
            {
                OldPassword = formModel.OldPassword,
                NewPassword = formModel.NewPassword,
            };

            var result = await _platformApi.SecurityChangePasswordAsync(base.WorkContext.CurrentCustomer.UserName, changePassword);

            if (result.Succeeded == true)
            {
                return StoreFrontRedirect("~/account");
            }
            else
            {
                ModelState.AddModelError("form", result.Errors.First());
                return View("customers/account", base.WorkContext);
            }
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
