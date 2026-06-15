using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AppointmentBooking.Infrastructure.Identity;
using AppointmentBooking.Application.ViewModels.Account;
using AppointmentBooking.Application.Shared;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using AppointmentBooking.Application.Settings;

namespace AppointmentBooking.Web.Controllers
{
    [EnableRateLimiting("auth")]
    public class AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<LocalizationSettings> options, ILogger<AccountController> logger, RoleManager<IdentityRole> roleManager, IValidator<LoginViewModel> loginValidator, IValidator<RegisterViewModel> registerValidator) : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IValidator<LoginViewModel> _loginValidator = loginValidator;
        private readonly IValidator<RegisterViewModel> _registerValidator = registerValidator;
        private readonly ILogger<AccountController> _logger = logger;
        private readonly LocalizationSettings _localizationSettings = options.Value;

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            return await ValidateAndExecuteAsync<LoginViewModel, IValidator<LoginViewModel>>(
                model, nameof(Login), async () =>
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                    {
                        ModelState.AddModelError(string.Empty, Localizer["Login_InvalidCredentials"]);
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(
                        model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        user.LastLoginAt = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);

                        return RedirectToAction("Index", "Dashboard");
                    }

                    if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, Localizer["Login_AccountLocked"]);
                        return View(model);
                    }

                    ModelState.AddModelError(string.Empty, Localizer["Login_ErrorOccurred"]);
                    return View(model);
                }
            );
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            return await ValidateAndExecuteAsync<RegisterViewModel, IValidator<RegisterViewModel>>(
                model, nameof(Register), async () =>
                {
                    var user = new ApplicationUser
                    {
                        FullName = model.Name,
                        UserName = model.Email,
                        Email = model.Email,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        if (!await _roleManager.RoleExistsAsync(Constants.UserRole))
                        {
                            await _roleManager.CreateAsync(new IdentityRole { Name = Constants.UserRole });
                        }

                        await _userManager.AddToRoleAsync(user, Constants.UserRole);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Dashboard");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(model);
                }
            );
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SetLanguage(string culture, string? returnUrl = null)
        {
            try
            {
                _logger.LogInformation("SetLanguage called: culture={Culture}, returnUrl={ReturnUrl}", culture, returnUrl);

                var cultureName = GetCultureNameFromConfig(culture);

                var profile = await GetCurrentProfileAsync();
                if (profile != null)
                {
                    profile.Localization.Language = culture;
                    profile.Localization.Direction = culture == GetDefaultLanguage() ? "rtl" : "ltr";
                    await ProfileService.UpdateAsync(profile);
                }

                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        HttpOnly = false,
                        SameSite = SameSiteMode.Lax,
                        Path = "/"
                    }
                );

                Response.Cookies.Append(
                    Constants.Keys.CookieUserLanguage,
                    culture,
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        HttpOnly = false,
                        Path = "/"
                    }
                );


                if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))

                    returnUrl = Request.Headers.Referer.ToString();


                if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))

                    returnUrl = "/";


                _logger.LogInformation("Redirecting to: {ReturnUrl}", returnUrl);
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change language");
                return Redirect("/");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Gets the full culture name (e.g., ar-EG, en-US) from the short culture code (ar, en)
        /// </summary>
        private string GetCultureNameFromConfig(string culture)
        {
            if (_localizationSettings.SupportedCultures != null &&
            _localizationSettings.SupportedCultures.TryGetValue(culture, out var cultureName))
            {
                return cultureName;
            }

            return _localizationSettings.DefaultCulture ?? Methods.CultureSwitch(culture);
        }

        /// <summary>
        /// Gets the default language (ar or en) from configuration
        /// </summary>
        private string GetDefaultLanguage()
        {
            return _localizationSettings.DefaultLanguage ?? Constants.DefaultLanguage;
        }

        #endregion
    }
}