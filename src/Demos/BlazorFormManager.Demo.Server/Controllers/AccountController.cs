using BlazorFormManager.Demo.Client.Models;
using BlazorFormManager.Demo.Server.Models;
using Carfamsoft.Model2View.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IOFile = System.IO.File;

namespace BlazorFormManager.Demo.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        #region fields

        // 5MB max for photos
        private const long PHOTO_SIZE_LIMIT = 5242880L;
        private const long BIG_FILE_SIZE_LIMIT = Startup.BIG_FILE_SIZE_LIMIT;
        private const string UPDATE_ERROR = " An update error occurred in the database while creating the user.";
        private const string GENERIC_ERROR = " An unexpected error of type {0} occurred while {1}.";
        private const string AUTO_SIGNED_IN = "You have been automatically signed in because account confirmation is disabled. ";
        private const string EMAIL_CONFIRMATION_REQUIRED = "You must confirm your email address before you can log in. ";
        private const string ACCOUNT_CREATED = "Your account has been successfully created. ";
        private const string ACCOUNT_CREATED_WITH_PASSWORD = "User created a new account with password.";
        private const string ACCOUNT_UPDATED = "User updated account information.";
        private const string JPEG_ONLY_SUPPORTED = "Only photos of type JPEG (with file extension .jpeg or .jpg) are supported.\n";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        // These options could be retrieved from a database or another server-side store;
        // otherwise, there would be no point in making an HTTP request just to retrieve
        // static / hard-coded values. But hey, this is a demo project!
        private static readonly SelectOption[] AgeRangeOptions = new[]
        {
            new SelectOption(id: 0, value: "[Your most appropriate age]", isPrompt: true),
            new SelectOption(1, "Minor (< 18)"),
            new SelectOption(2, "Below or 25"),
            new SelectOption(3, "Below or 30"),
            new SelectOption(4, "Below or 40"),
            new SelectOption(5, "Below 50"),
            new SelectOption(6, "Between 50 and 54"),
            new SelectOption(7, "Between 55 and 60"),
            new SelectOption(8, "Above 60"),
            new SelectOption(9, "Above 70"),
            new SelectOption(10, "Above 80"),
        };

    #endregion

    public AccountController(UserManager<ApplicationUser> userManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("info/{id?}")]
        public async Task<IActionResult> GetInfo(string id)
        {
            var user = string.IsNullOrWhiteSpace(id) 
                ? await _userManager.FindByNameAsync(User.Identity.Name)
                : await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                return Ok(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.AgeRange,
                    user.FavouriteColor,
                    user.FavouriteWorkingDay,
                    user.TwoFactorEnabled,
                });
            }

            return NotFound();
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetUsers([FromQuery] PaginationRequestModel model)
        {
            var userQuery = _userManager.Users;

            if (model.Sort.HasValue && !string.IsNullOrWhiteSpace(model.Column))
                userQuery = SortUsers(userQuery, model.Column, ascending: model.Sort.Value);

            var items = await model.GetPageAsync(userQuery);
            
            return Ok(new
            {
                model.TotalItemCount,
                Items = items.Select(user => new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.AgeRange,
                    user.FavouriteColor,
                    user.FavouriteWorkingDay,
                    user.TwoFactorEnabled,
                })
            });
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

                var (success, photoMessage) = await SetPhotoAsync(user);
                if (!success) return Ok(new { success, error = photoMessage });

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    message = ACCOUNT_CREATED;
                    _logger.LogInformation(ACCOUNT_CREATED_WITH_PASSWORD);
                    var signedIn = false;

                    if (emailConfirmed)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        signedIn = true;
                        message += AUTO_SIGNED_IN;
                        _logger.LogInformation(message);
                    }
                    else
                    {
                        message += EMAIL_CONFIRMATION_REQUIRED;
                    }

                    message += photoMessage;
                    return Ok(new { success = true, message, signedIn });
                }

                var sb = new System.Text.StringBuilder();

                foreach (var error in result.Errors)
                    sb.AppendLine(error.Description);

                message = sb.ToString();
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
        public async Task<IActionResult> Update([FromForm] AutoUpdateUserModel model)
        {
            string message;
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);

                if (user != null)
                {
                    var (success, photoMessage) = await SetPhotoAsync(user);
                    if (!success) return Ok(new { success, error = photoMessage });

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.AgeRange = model.AgeRange;
                    user.FavouriteColor = model.FavouriteColor;
                    user.FavouriteWorkingDay = model.FavouriteWorkingDay;
                    user.TwoFactorEnabled = model.TwoFactorEnabled;

                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation(ACCOUNT_UPDATED);

                    message = $"Your account information has been updated. {photoMessage}";

                    if (!string.IsNullOrWhiteSpace(model.FavouriteColor))
                        message += $" Your favourite color is {model.FavouriteColor}.";

                    if (!string.IsNullOrWhiteSpace(model.FavouriteWorkingDay))
                        message += $" Your favourite working day is {model.FavouriteWorkingDay}.";

                    if (model.AgeRange > 0)
                        message += $" Your age range id is {model.AgeRange}.";

                    message += $"Two factor authentication is {(model.TwoFactorEnabled ? "enabled" : "disabled")}.";

                    return Ok(new { success = true, message });
                }
                else
                {
                    message = "User account not found!";
                }
            }
            catch (Exception ex)
            {
                var genericMessage = GetGenericErrorMessage(ex, "updating the user");
                _logger.LogError(ex, genericMessage);
#if TRACE
                message = ex.ToString();
#else
                message = genericMessage;
#endif
            }

            return Ok(new { success = false, error = message });
        }

        [HttpGet("Photo/{id?}")]
        public async Task<FileContentResult> Photo(string id)
        {
            ApplicationUser user;

            if (!string.IsNullOrWhiteSpace(id))
                user = await _userManager.FindByIdAsync(id);
            else
                user = await _userManager.FindByNameAsync(User.Identity.Name);
            
            return File(user?.Photo ?? new byte[0], "image/jpeg");
        }

        [HttpPost("UploadBigFileTest")]
        [RequestSizeLimit(Startup.BIG_FILE_SIZE_LIMIT)]
        [RequestFormLimits(MultipartBodyLengthLimit = Startup.BIG_FILE_SIZE_LIMIT)]
        public async Task<IActionResult> UploadBigFileTest([FromServices] IWebHostEnvironment env)
        {
            EnsureUploadsTempDirectoryCreated(env);
            var (filename, size) = await CopyFileToTempLocationAsync(env, Request.Form.Files.FirstOrDefault());
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

            var result = success
                ? new
                {
                    success,
                    message = "File successfully saved.",
                    filename,
                    size,
                }
                : (object)new
                {
                    success,
                    error = "Could not save the file.",
                };

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("upload-files-anonymous")]
        [RequestSizeLimit(Startup.BIG_FILE_SIZE_LIMIT)]
        [RequestFormLimits(MultipartBodyLengthLimit = Startup.BIG_FILE_SIZE_LIMIT)]
        public async Task<IActionResult> UploadFiles([FromForm] IFormFileCollection files, [FromServices] IWebHostEnvironment env)
        {
            bool success;

            if (files.Count > 0)
            {
                EnsureUploadsTempDirectoryCreated(env);

                var fileInfo = new List<object>();
                var totalSize = 0D;
                var totalUploadedSize = 0D;

                foreach (var file in files)
                {
                    totalUploadedSize += file.Length;

                    var (filename, size) = await CopyFileToTempLocationAsync(env, file);
                    success = !string.IsNullOrEmpty(filename);

                    if (success)
                    {
                        totalSize += size;
                        // don't keep the file
                        try
                        {
                            IOFile.Delete(filename);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                        }

                        fileInfo.Add(new { name = Path.GetFileName(filename), size });
                    }
                }

                var savedFiles = fileInfo.Take(20).ToArray();
                var diff = files.Count - fileInfo.Count;
                var truncated = files.Count - savedFiles.Length;
                var truncatedMsg = truncated > 0 ? $" {truncated} file(s) were left off in the 'savedFiles' property." : null;

                success = diff == 0;

                if (success)
                {
                    var totalSizeString = FileSizeToString(totalSize);
                    var totalUploadedSizeString = FileSizeToString(totalUploadedSize);

                    return Ok(new
                    {
                        success,
                        savedFiles,
                        totalSize,
                        message = $"All {files.Count} file(s) saved successfully " +
                                $"to temporary uploads directory and deleted immediately." +
                                $"{truncatedMsg} Total saved size: {totalSizeString} of {totalUploadedSizeString}.",
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success,
                        savedFiles,
                        error = $"Could not save {diff} of {files.Count} file(s).",
                    });
                }
            }

            return Ok(new { success = false, error = "No files uploaded." });
        }

        [HttpPost("array-test")]
        public IActionResult ArrayTest([FromForm] ArrayModel model)
        {
            var success = model?.Items?.Length == ArrayModel.SampleItems.Length;

            if (success) return Ok(new { success, message = "Array model lengths match." });

            return BadRequest(new { success, error = "Array model lengths do not match." });
        }

        [HttpGet("options")]
        public IEnumerable<SelectOptionList> GetOptions()
        {
            var colorOptions = new[]
            {
                new SelectOption("red", "Red"),
                new SelectOption("green", "Green"),
                new SelectOption("blue", "Blue"),
            };

            return new[]
            {
                new SelectOptionList(nameof(AutoUpdateUserModel.AgeRange), AgeRangeOptions),
                new SelectOptionList(nameof(AutoUpdateUserModel.FavouriteColor), colorOptions),
            };
        }

        #region helpers

        private async Task<(bool success, string message)> SetPhotoAsync(ApplicationUser user)
        {
            bool success = true;
            string message = null;
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files[0];

                if (string.Equals("image/jpeg", file.ContentType, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals("image/jpg", file.ContentType, StringComparison.OrdinalIgnoreCase))
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    ms.Position = 0L;
                    var content = ms.ToArray();
                    user.Photo = content;
                    message = $"Total size of uploaded file: {content.Length / 1024d:N2} kb.\n";
                }
                else
                {
                    (success, message) = (false, JPEG_ONLY_SUPPORTED);
                }
            }
            return (success, message);
        }

        private async Task<(string filename, long size)> CopyFileToTempLocationAsync(IWebHostEnvironment env, IFormFile file)
        {
            try
            {
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

        private void EnsureUploadsTempDirectoryCreated(IWebHostEnvironment env)
        {

            var destinationFile = Path.Combine
            (
                env.ContentRootPath,
                "Uploads",
                "Temp"
            );

            var directory = Path.GetDirectoryName(destinationFile);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        }

        private IQueryable<ApplicationUser> SortUsers(IQueryable<ApplicationUser> q, string column, bool ascending)
        {
            return column switch
            {
                nameof(ApplicationUser.FirstName)        => ascending ? q.OrderBy(u => u.FirstName)        : q.OrderByDescending(u => u.FirstName),
                nameof(ApplicationUser.LastName)         => ascending ? q.OrderBy(u => u.LastName)         : q.OrderByDescending(u => u.LastName),
                nameof(ApplicationUser.Email)            => ascending ? q.OrderBy(u => u.Email)            : q.OrderByDescending(u => u.Email),
                nameof(ApplicationUser.PhoneNumber)      => ascending ? q.OrderBy(u => u.PhoneNumber)      : q.OrderByDescending(u => u.PhoneNumber),
                nameof(ApplicationUser.FavouriteColor)   => ascending ? q.OrderBy(u => u.FavouriteColor)   : q.OrderByDescending(u => u.FavouriteColor),
                nameof(ApplicationUser.TwoFactorEnabled) => ascending ? q.OrderBy(u => u.TwoFactorEnabled) : q.OrderByDescending(u => u.TwoFactorEnabled),
                _ => q,
            };
        }

        /// <summary>
        /// Converts the specified file size to a human-readable string representation.
        /// </summary>
        /// <param name="size">The size of a file.</param>
        /// <returns></returns>
        public static string FileSizeToString(double size)
        {
            if (size == 0) return string.Empty;
            if (size < 1024) return $"{size} B";
            if (size < 1024 * 1024) return $"{size / (double)1024:N2} KB";
            if (size < 1024 * 1024 * 1024) return $"{size / (double)(1024 * 1024):N2} MB";
            return $"{size / (1024 * 1024 * 1024):N2} GB";
        }
        #endregion
    }
}
