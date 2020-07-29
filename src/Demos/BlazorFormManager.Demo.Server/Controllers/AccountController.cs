using BlazorFormManager.Demo.Client.Models;
using BlazorFormManager.Demo.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IOFile = System.IO.File;

namespace BlazorFormManager.Demo.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // 5MB max for photos
        private const long PHOTO_SIZE_LIMIT = 5242880L;
        private const long BIG_FILE_SIZE_LIMIT = Startup.BIG_FILE_SIZE_LIMIT;
        private const string UPDATE_ERROR = " An update error occurred in the database while creating the user.";
        private const string GENERIC_ERROR = " An unexpected error of type {0} occurred while {1}.";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetInfo()
        {
            var user = await _userManager.FindByNameAsync(GetUserName());
            if (user != null)
            {
                return Ok(new { user.FirstName, user.LastName, user.Email, user.PhoneNumber });
            }
            return NotFound();
        }

        /// <summary>
        /// The request size limit should be <see cref="PHOTO_SIZE_LIMIT"/>.
        /// Setting it to <see cref="BIG_FILE_SIZE_LIMIT"/> gives you
        /// enough time to see the upload progress and allows you to abort the request.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="signInManager"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Register")]
        [RequestSizeLimit(BIG_FILE_SIZE_LIMIT)]
        [RequestFormLimits(MultipartBodyLengthLimit = BIG_FILE_SIZE_LIMIT)]
        public async Task<IActionResult> Register
        (
            [FromForm] RegisterUserModel model, 
            [FromServices] SignInManager<ApplicationUser> signInManager
        )
        {
            string message = null;
            try
            {
                var emailConfirmed = !_userManager.Options.SignIn.RequireConfirmedAccount;
                var user = new ApplicationUser
                {
                    Email = model.Email,
                    EmailConfirmed = emailConfirmed,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,
                };

                bool success;
                (success, message) = await SetPhotoAsync(user);
                if (!success) return Ok(new { success, error = message });

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    var signedIn = false;

                    if (emailConfirmed)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        signedIn = true;

                        message += " You have been automatically signed in because " +
                            "account confirmation is disabled.";
                        
                        _logger.LogInformation(message);
                    }

                    return Ok(new { success = true, message, signedIn });
                }

                var sb = new System.Text.StringBuilder();

                foreach (var error in result.Errors)
                    sb.AppendLine(error.Description);

                message += sb.ToString();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, UPDATE_ERROR);
#if TRACE
                message += ex.ToString();
#else
                message += UPDATE_ERROR;
#endif
            }
            catch (Exception ex)
            {
                var genericMessage = GetGenericErrorMessage(ex, "creating the user");
                _logger.LogError(ex, genericMessage);
#if TRACE
                message += ex.ToString();
#else
                message += genericMessage;
#endif
            }
            return Ok(new { success = false, error = message });
        }

        [HttpPost("Update")]
        [RequestSizeLimit(PHOTO_SIZE_LIMIT)]
        [RequestFormLimits(MultipartBodyLengthLimit = PHOTO_SIZE_LIMIT)]
        public async Task<IActionResult> Update([FromForm] UpdateUserModel model)
        {
            string message = null;
            try
            {
                var user = await _userManager.FindByNameAsync(GetUserName());

                if (user != null)
                {
                    bool success;
                    (success, message) = await SetPhotoAsync(user);
                    if (!success) return Ok(new { success, error = message });

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;

                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User updated account information.");
                    return Ok(new { success = true, message });
                }
                else
                {
                    message = "User not found!";
                }
            }
            catch (Exception ex)
            {
                var genericMessage = GetGenericErrorMessage(ex, "updating the user");
                _logger.LogError(ex, genericMessage);
#if TRACE
                message += ex.ToString();
#else
                message += genericMessage;
#endif
            }

            return Ok(new { success = false, error = message });
        }

        [HttpGet("Photo")]
        public async Task<IActionResult> Photo()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name ?? GetUserName());
            if (user != null && user.Photo != null)
            {
                return File(user.Photo, "image/jpeg");
            }
            return NotFound();
        }

        [HttpPost("UploadBigFileTest")]
        [RequestSizeLimit(Startup.BIG_FILE_SIZE_LIMIT)]
        [RequestFormLimits(MultipartBodyLengthLimit = Startup.BIG_FILE_SIZE_LIMIT)]
        public async Task<IActionResult> UploadBigFileTest([FromServices] IWebHostEnvironment env)
        {
            var (filename, length) = await CopyFileToTempLocationAsync(env);
            var success = !string.IsNullOrEmpty(filename);

            if (success)
            {
                // don't keep the file
                await Task.Delay(1000);
                try
                {
                    IOFile.Delete(filename);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }

            return Ok(new
            { 
                success, 
                error = success ? null : "Could not save the file.",
                filename, 
                length 
            });
        }

        #region helpers

        private string GetUserName() => User.Identity.Name ?? User.FindFirstValue(ClaimTypes.Name);

        private async Task<(bool success, string message)> SetPhotoAsync(ApplicationUser user)
        {
            if (Request.Form.Files.Any())
            {
                var file = Request.Form.Files.First();

                if (string.Equals("image/jpeg", file.ContentType, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals("image/jpg", file.ContentType, StringComparison.OrdinalIgnoreCase))
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0L;
                    var content = ms.ToArray();
                    user.Photo = content;
                    return (true, $"Total size of uploaded file: {content.Length / 1024d:N2} kb.\n");
                }
                else
                {
                    return (false, "Only photos of type JPEG (with file extension .jpeg or .jpg) are supported.\n");
                }
            }
            return (true, null);
        }

        private async Task<(string filename, long length)> CopyFileToTempLocationAsync(IWebHostEnvironment env)
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file != null)
                {
                    var tmpFile = ReplaceInvalidFileNameChars
                    (
                        $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid():n}.tmp"
                    );

                    var destinationFile = Path.Combine
                    (
                        env.ContentRootPath,
                        "Uploads",
                        "Temp",
                        tmpFile
                    );

                    using var destinationStream = IOFile.OpenWrite(destinationFile);
                    await file.CopyToAsync(destinationStream);
                    return (destinationFile, file.Length);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            return (null, 0L);
        }

        private string ReplaceInvalidFileNameChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()))
                .Replace(" ", string.Empty);
        }

        private string GetGenericErrorMessage(Exception ex, string activity)
        {
            return string.Format(GENERIC_ERROR, ex.GetType().FullName, activity);
        }

        #endregion
    }
}
