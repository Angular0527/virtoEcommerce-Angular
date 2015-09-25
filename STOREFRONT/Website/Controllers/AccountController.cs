﻿using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.ApiClient.DataContracts.Security;
using VirtoCommerce.Web.Models;
using VirtoCommerce.Web.Convertors;
using VirtoCommerce.Web.Models.FormModels;
using VirtoCommerce.Web.Models.Forms;

namespace VirtoCommerce.Web.Controllers
{
    [RoutePrefix("account")]
    [Authorize]
    public class AccountController : StoreControllerBase
    {
        private const string ResetCustomerPasswordTokenCookie = "Vcf.ResetCustomerPasswordToken";
        private const string CustomerIdCookie = "Vcf.CustomerId";

        private IAuthenticationManager _authenticationManager;
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return _authenticationManager ?? (_authenticationManager = HttpContext.GetOwinContext().Authentication);
            }
        }

        //
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        //[RequireHttps]
        public async Task<ActionResult> Login(string returnUrl, string uid)
        {
            if (User.Identity.IsAuthenticated)
            {
                AuthenticationManager.SignOut();
            }

            if (!String.IsNullOrEmpty(uid))
            {
                var impersonatedUser = await SecurityService.GetUserByIdAsync(uid);
                if (impersonatedUser != null && !String.IsNullOrEmpty(impersonatedUser.Email))
                {
                    var impersonatedCustomer = await CustomerService.GetCustomerAsync(impersonatedUser.Email, Context.StoreId);
                    if (impersonatedCustomer != null)
                    {
                        Context.Set("impersonated_user_name", impersonatedCustomer.Name);
                        Context.Set("impersonated_user_id", uid);
                    }
                }
            }

            return View("customers/login");
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        //[RequireHttps]
        public async Task<ActionResult> Login(LoginFormModel formModel, string returnUrl)
        {
            var form = Service.GetForm(SiteContext.Current, formModel.Id);

            if (form != null)
            {
                var formErrors = GetFormErrors(ModelState);

                if (formErrors == null)
                {
                    form.PostedSuccessfully = true;

                    if (!String.IsNullOrEmpty(formModel.ImpersonatedUserId))
                    {
                        var csrUser = await SecurityService.GetUserByNameAsync(formModel.Email);
                        if (csrUser == null)
                        {
                            Context.ErrorMessage = "CSR user was not found.";
                            return View("error");
                        }

                        //if (!csrUser.Permissions.Contains("customer:loginOnBehalf", StringComparer.OrdinalIgnoreCase))
                        //{
                        //    return View("error");
                        //}

                        //var csrCustomer = await CustomerService.GetCustomerAsync(formModel.Email, Context.StoreId);
                        //if (csrCustomer == null)
                        //{
                        //    return View("error");
                        //}

                        var user = await SecurityService.GetUserByIdAsync(formModel.ImpersonatedUserId);
                        if (user == null)
                        {
                            Context.ErrorMessage = "User was not found.";
                            return View("error");
                        }

                        var customer = await CustomerService.GetCustomerAsync(user.Email, Context.StoreId);
                        if (customer == null)
                        {
                            Context.ErrorMessage = "User has no account.";
                            return View("error");
                        }

                        var customerIdentity = SecurityService.CreateClaimsIdentity(user.Email);
                        AuthenticationManager.SignIn(SecurityService.CreateClaimsIdentity(user.Email));

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        var loginResult = await SecurityService.PasswordSingInAsync(
                            formModel.Email, formModel.Password, false);

                        switch (loginResult)
                        {
                            case SignInStatus.Success:
                                var identity = SecurityService.CreateClaimsIdentity(formModel.Email);
                                AuthenticationManager.SignIn(identity);
                                return RedirectToLocal(returnUrl);
                            case SignInStatus.LockedOut:
                                return View("lockedout");
                            case SignInStatus.RequiresVerification:
                                return RedirectToAction("SendCode", "Account");
                            case SignInStatus.Failure:
                            default:
                                form.Errors = new SubmitFormErrors("form", "Login attempt fails.");
                                form.PostedSuccessfully = false;
                                return View("customers/login");
                        }
                    }
                }
                else
                {
                    form.Errors = formErrors;
                    form.PostedSuccessfully = false;

                    return View("customers/login");
                }
            }

            Context.ErrorMessage = "Liquid error: Form context was not found.";

            return View("error");
        }

        //
        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        //[Route("register")]
        public ActionResult Register()
        {
            return View("customers/register");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterFormModel formModel)
        {
            var form = Service.GetForm(SiteContext.Current, formModel.Id);

            if (form != null)
            {
                var formErrors = GetFormErrors(ModelState);

                if (formErrors == null)
                {
                    form.PostedSuccessfully = true;

                    var user = new ApplicationUser
                    {
                        Email = formModel.Email,
                        Password = formModel.Password,
                        UserName = formModel.Email
                    };

                    var result = await SecurityService.CreateUserAsync(user);

                    if (result.Succeeded)
                    {
                        user = await SecurityService.GetUserByNameAsync(user.UserName);

                        Context.Customer = await this.CustomerService.CreateCustomerAsync(
                            formModel.Email, formModel.FirstName, formModel.LastName, user.Id, null);

                        await SecurityService.PasswordSingInAsync(formModel.Email, formModel.Password, false);

                        var identity = SecurityService.CreateClaimsIdentity(formModel.Email);
                        AuthenticationManager.SignIn(identity);

                        return RedirectToAction("Index", "Account");
                    }
                    else
                    {
                        form.Errors = new SubmitFormErrors("form", result.Errors.First());
                        form.PostedSuccessfully = false;
                    }
                }
                else
                {
                    form.Errors = formErrors;
                    form.PostedSuccessfully = false;
                }
            }
            else
            {
                return View("error");
            }

            return View("customers/register");
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordFormModel formModel)
        {
            var form = Service.GetForm(SiteContext.Current, formModel.Id);

            if (form != null)
            {
                var formErrors = GetFormErrors(ModelState);

                if (formErrors == null)
                {
                    form.PostedSuccessfully = true;

                    var user = await SecurityService.GetUserByNameAsync(formModel.Email);

                    if (user != null)
                    {
                        string callbackUrl = Url.Action("ResetPassword", "Account",
                            new { UserId = user.Id, Code = "token" }, protocol: Request.Url.Scheme);

                        await SecurityService.GenerateResetPasswordTokenAsync(
                            user.Id, Context.Shop.Name, callbackUrl);
                    }
                    else
                    {
                        form.Errors = new SubmitFormErrors("form", "User not found");
                        form.PostedSuccessfully = false;
                    }
                }
                else
                {
                    form.Errors = formErrors;
                    form.PostedSuccessfully = false;
                }
            }
            else
            {
                Context.ErrorMessage = "Liquid error: Form context was not found.";

                return View("error");
            }

            return new RedirectResult(Url.Action("Login", "Account") + "#recover");
        }

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string code, string userId)
        {
            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(userId))
            {
                Context.ErrorMessage = "Error in URL format";

                return View("error");
            }

            var user = await SecurityService.GetUserByIdAsync(userId);
            if (user == null)
            {
                Context.ErrorMessage = "User was not found.";

                return View("error");
            }

            var tokenCookie = new HttpCookie(ResetCustomerPasswordTokenCookie, code);
            tokenCookie.Expires = DateTime.UtcNow.AddDays(1);
            HttpContext.Response.Cookies.Add(tokenCookie);

            var customerIdCookie = new HttpCookie(CustomerIdCookie, userId);
            customerIdCookie.Expires = DateTime.UtcNow.AddDays(1);
            HttpContext.Response.Cookies.Add(customerIdCookie);

            return View("customers/reset_password");
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordFormModel formModel)
        {
            var form = Service.GetForm(SiteContext.Current, formModel.form_type);

            if (form != null)
            {
                var formErrors = GetFormErrors(ModelState);

                string userId = HttpContext.Request.Cookies[CustomerIdCookie] != null ?
                    HttpContext.Request.Cookies[CustomerIdCookie].Value : null;
                string token = HttpContext.Request.Cookies[ResetCustomerPasswordTokenCookie] != null ?
                    HttpContext.Request.Cookies[ResetCustomerPasswordTokenCookie].Value : null;

                if (userId == null && token == null)
                {
                    Context.ErrorMessage = "Not enough info for reseting password";

                    return View("error");
                }

                if (formErrors == null)
                {
                    var result = await SecurityService.ResetPasswordAsync(userId, token, formModel.Password);

                    if (result.Succeeded)
                    {
                        HttpContext.Response.Cookies.Remove(CustomerIdCookie);
                        HttpContext.Response.Cookies.Remove(ResetCustomerPasswordTokenCookie);

                        return View("password_reseted");
                    }
                    else
                    {
                        form.Errors = new SubmitFormErrors("form", result.Errors.First());
                        form.PostedSuccessfully = false;
                    }
                }
                else
                {
                    form.Errors = formErrors;
                    form.PostedSuccessfully = false;
                }
            }
            else
            {
                Context.ErrorMessage = "Liquid error: Form context was not found.";

                return View("error");
            }

            return View("customers/reset_password");
        }

        //
        // GET: /Account/LogOFf
        [HttpGet]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();

            return Redirect("~");
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLogin(ExternalLoginFormModel formModel, string returnUrl)
        {
            var form = GetForm(formModel.form_type);

            if (form != null)
            {
                return new ChallengeResult(
                    formModel.AuthenticationType,
                    Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
            }

            Context.ErrorMessage = "Liquid error: Form context was not found.";

            return View("error");
        }

        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
            {
                Context.ErrorMessage = "External login info was not found.";

                return View("error");
            }

            var user = await SecurityService.GetUserByLoginAsync(new UserLoginInfo
            {
                LoginProvider = loginInfo.Login.LoginProvider,
                ProviderKey = loginInfo.Login.ProviderKey
            });

            if (user == null)
            {
                return RedirectToAction("ExternalLoginConfirmation", "Account",
                    new { ReturnUrl = returnUrl, LoginProvider = loginInfo.Login.LoginProvider });
            }
            else
            {
                var identity = SecurityService.CreateClaimsIdentity(user.UserName);

                AuthenticationManager.SignIn(identity);

                return RedirectToLocal(returnUrl);
            }
        }

        //
        // GET: /Account/ExternalLoginConfirmation
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ExternalLoginConfirmation(string returnUrl, string loginProvider)
        {
            if (string.IsNullOrEmpty(loginProvider))
            {
                Context.ErrorMessage = "URL format error.";

                return View("error");
            }

            return View("external_login_confirmation");
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationFormModel formModel)
        {
            var form = GetForm(formModel.Id);

            if (form != null)
            {
                var formErrors = GetFormErrors(ModelState);

                if (formErrors == null)
                {
                    form.PostedSuccessfully = true;

                    var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();

                    if (loginInfo == null)
                    {
                        Context.ErrorMessage = "External login info was not found";

                        return View("error");
                    }

                    var user = new ApplicationUser { UserName = formModel.Email, Email = formModel.Email };
                    user.Logins = new List<UserLoginInfo>
                    {
                        new UserLoginInfo
                        {
                            LoginProvider = loginInfo.Login.LoginProvider,
                            ProviderKey = loginInfo.Login.ProviderKey
                        }
                    };

                    var result = await SecurityService.CreateUserAsync(user);

                    if (result.Succeeded)
                    {
                        form.PostedSuccessfully = true;

                        user = await SecurityService.GetUserByNameAsync(formModel.Email);

                        Context.Customer = await this.CustomerService.CreateCustomerAsync(
                            formModel.Email, formModel.Email, null, user.Id, null);

                        var identity = SecurityService.CreateClaimsIdentity(user.UserName);
                        AuthenticationManager.SignIn(identity);

                        return RedirectToLocal(formModel.ReturnUrl);
                    }
                    else
                    {
                        form.Errors = new SubmitFormErrors("form", result.Errors.First());
                        form.PostedSuccessfully = false;

                        return View("external_login_confirmation");
                    }
                }
                else
                {
                    form.Errors = formErrors;
                    form.PostedSuccessfully = false;

                    return View("external_login_confirmation");
                }
            }

            Context.ErrorMessage = "Liquid error: Form context was not found.";

            return View("error");
        }

        //
        // GET: /Account
        [HttpGet]
        public async Task<ActionResult> Index(int page = 1)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var customer = await CustomerService.GetCustomerAsync(
                    HttpContext.User.Identity.Name, Context.StoreId);

                Context.Set("Customer", customer);
                Context.Set("current_page", page);

                return View("customers/account");
            }

            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/Addresses
        [HttpGet]
        public ActionResult Addresses()
        {
            foreach (var address in this.Context.Customer.Addresses)
            {
                var addressForm = new AddressForm();
                addressForm.FormContext = address;
                addressForm.FormType = "customer_address";
                addressForm.Id = address.Id;

                Context.Forms.Add(addressForm);
            }

            return View("customers/addresses");
        }

        //
        // POST: /Account/EditAddress
        [HttpPost]
        public async Task<ActionResult> EditAddress(CustomerAddressFormModel formModel, string id)
        {
            var form = GetForm(formModel.form_type);

            var customer = this.Context.Customer;
            var customerAddress = customer.Addresses.FirstOrDefault(a => a.Id == id);

            if (customerAddress != null)
            {
                customer.Addresses.Remove(customerAddress);
                customer.Addresses.Add(formModel.AsWebModel());
            }
            else
            {
                customer.Addresses.Add(formModel.AsWebModel());
            }

            await this.CustomerService.UpdateCustomerAsync(customer);

            return RedirectToAction("Addresses", "Account");
        }

        //
        // POST: /Account/Addresses
        [HttpPost]
        public async Task<ActionResult> Addresses(string id)
        {
            var address = Context.Customer.Addresses.FirstOrDefault(a => a.Id == id);

            if (address != null)
            {
                Context.Customer.Addresses.Remove(address);
                await CustomerService.UpdateCustomerAsync(Context.Customer);

                return View("customers/addresses");
            }

            return View("error");
        }

        [HttpGet]
        [Route("order/{id}")]
        public async Task<ActionResult> Order(string id)
        {
            var order = await CustomerService.GetOrderAsync(Context.Shop.StoreId, Context.Customer.Email, id);

            if (order != null)
            {
                var orderModel = order.AsWebModel();

                if (orderModel.FinancialStatus == "Pending")
                {
                    orderModel.PaymentMethods = Context.Shop.PaymentMethods;
                }

                this.Context.Order = orderModel;
            }

            return this.View("customers/order");
        }

        //
        // POST: /account/payorder
        [HttpPost]
        public async Task<ActionResult> PayOrder(PayOrderFormModel formModel)
        {
            var form = GetForm(formModel.form_type);

            if (form != null)
            {
                var paymentMethods = Context.Shop.PaymentMethods;

                if (paymentMethods != null)
                {
                    var paymentMethod = paymentMethods.FirstOrDefault(pm => pm.Code == formModel.PaymentMethodId);

                    if (paymentMethod != null)
                    {
                        var order = await CustomerService.GetOrderAsync(Context.StoreId, Context.CustomerId, formModel.OrderId);

                        if (order != null)
                        {
                            if (order.InPayments == null)
                            {
                                order.InPayments = new List<ApiClient.DataContracts.Orders.PaymentIn>();
                            }
                            // TODO: Remake for partial payments
                            order.InPayments.Add(new ApiClient.DataContracts.Orders.PaymentIn
                            {
                                Currency = order.Currency,
                                CustomerId = Context.CustomerId,
                                GatewayCode = paymentMethod.Code,
                                Sum = order.Sum
                            });

                            await CustomerService.UpdateOrderAsync(order);

                            order = await CustomerService.GetOrderAsync(Context.StoreId, Context.CustomerId, formModel.OrderId);

                            if (order != null)
                            {
                                var inPayment = order.InPayments.Where(p => p.GatewayCode == formModel.PaymentMethodId)
                                    .OrderByDescending(p => p.CreatedDate).FirstOrDefault(); // For test

                                if (inPayment != null)
                                {
                                    //var paymentResult = await Service.ProcessPaymentAsync(order.Id, inPayment.Id);

                                    //if (paymentResult != null)
                                    //{
                                    //    if (paymentResult.IsSuccess)
                                    //    {
                                    //        if (paymentResult.PaymentMethodType == ApiClient.DataContracts.PaymentMethodType.Redirection)
                                    //        {
                                    //            if (!string.IsNullOrEmpty(paymentResult.RedirectUrl))
                                    //            {
                                    //                return Redirect(paymentResult.RedirectUrl);
                                    //            }
                                    //        }
                                    //        if (paymentResult.PaymentMethodType == ApiClient.DataContracts.PaymentMethodType.PreparedForm)
                                    //        {
                                    //            if (!string.IsNullOrEmpty(paymentResult.HtmlForm))
                                    //            {
                                    //                SiteContext.Current.Set("payment_html_form", paymentResult.HtmlForm);
                                    //                return View("payment");
                                    //            }
                                    //        }
                                    //    }
                                    //    else
                                    //    {
                                    //        Context.ErrorMessage = paymentResult.Error;

                                    //        return View("error");
                                    //    }
                                    //}
                                }
                            }
                        }
                    }
                }
            }

            return View("error");
        }

        [HttpGet]
        public async Task<ActionResult> Quotes(int? p)
        {
            var searchCriteria = new QuoteRequestSearchCriteria
            {
                CustomerId = Context.CustomerId,
                Skip = ((p ?? 1) - 1) * 20,
                StoreId = Context.StoreId,
                Tag = null,
                Take = 20
            };

            Context.Customer.Quotes = await QuoteService.SearchAsync(searchCriteria);

            return View("customers/quotes");
        }

        [HttpGet]
        [Route("quote/{number}")]
        public async Task<ActionResult> Quote(string number)
        {
            Context.QuoteRequest = await QuoteService.GetByNumberAsync(Context.StoreId, Context.CustomerId, number);

            return View("customers/quote");
        }

        [HttpGet]
        [Route("quote/edit/{number}")]
        public async Task<ActionResult> EditQuote(string number)
        {
            var quoteRequest = await QuoteService.GetByNumberAsync(Context.StoreId, Context.CustomerId, number);

            Context.ActualQuoteRequest = quoteRequest;
            Context.ActualQuoteRequest.Tag = "actual";

            await QuoteService.UpdateQuoteRequestAsync(Context.ActualQuoteRequest);

            return RedirectToAction("Index", "Quote");
        }

        [HttpGet]
        [Route("quote/reject/{number}")]
        public async Task<ActionResult> RejectQuote(string number)
        {
            var quoteRequest = await QuoteService.GetByNumberAsync(Context.StoreId, Context.CustomerId, number);
            quoteRequest.Status = "Rejected";

            await QuoteService.UpdateQuoteRequestAsync(quoteRequest);

            return RedirectToAction("Quotes");
        }

        [HttpPost]
        [Route("quote/checkout")]
        public async Task<ActionResult> ConfirmQuote(QuoteRequest model)
        {
            Context.QuoteRequest = await QuoteService.GetByNumberAsync(Context.StoreId, Context.CustomerId, model.Number);

            foreach (var modelQuoteItem in model.Items)
            {
                var quoteItem = Context.QuoteRequest.Items.FirstOrDefault(i => i.Id == modelQuoteItem.Id);
                quoteItem.SelectedTierPrice = modelQuoteItem.SelectedTierPrice;
            }

            var newQuoteRequest = await QuoteService.RecalculateAsync(Context.QuoteRequest);
            Context.QuoteRequest = newQuoteRequest;

            Context.Cart.Items.Clear();

            foreach (var quoteItem in Context.QuoteRequest.Items)
            {
                var lineItemModel = quoteItem.AsLineItemModel();
                lineItemModel.Sku = "1"; // TODO: Sku should be added to DB table
                Context.Cart.Items.Add(lineItemModel);
            }

            if (Context.Cart.IsTransient)
            {
                await Service.CreateCartAsync(Context.Cart);
            }
            else
            {
                await Service.SaveChangesAsync(Context.Cart);
            }

            return Json(new { redirectUrl = VirtualPathUtility.ToAbsolute("~/checkout") });
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public string LoginProvider { get; set; }

            public string RedirectUri { get; set; }

            public string UserId { get; set; }

            public ChallengeResult(string loginProvider, string redirectUri)
                : this(loginProvider, redirectUri, null)
            {
            }

            public ChallengeResult(string loginProvider, string redirectUri, string userId)
            {
                LoginProvider = loginProvider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };

                if (this.UserId != null)
                {
                    properties.Dictionary["XsrfId"] = this.UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~");
        }
    }
}