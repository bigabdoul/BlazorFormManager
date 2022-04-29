# Walkthrough: Integrating BlazorFormManager

## Introduction

This walkthrough requires some familarity with the latest version of `Visual Studio 2019`
or higher. You can also use `Visual Studio Code` or any other code editor that supports
the `dotnet CLI` but they don't apply to this document.

You can download and install for free the latest version of `Visual Studio 2019 Community`
here: https://visualstudio.microsoft.com/downloads

To integrate BlazorFormManager into your own project, follow the steps below. Feel free
to skip Step 1 if you already have an existing project whose layout corresponds to the
`Blazor WebAssembly App` project template created with the latest version of Visual
Studio 2019 or higher.

The instructions guide you through building step by step a set of projects similar to the
demo projects found in this repository. If you aren't fimiliar with Blazor, you can learn
more about it here: https://blazor.net.

Note: Developement was done on Windows so instructions like keyboard combinations are
specific to that environment.

_Warning: This document is a work in progress! Expect it to be frequently updated._

## What you will learn in this walkthrough:

- How to create a (client-side) Blazor WebAssembly Application using Visual Studio
- How to update all the packages in the new Solution
- How to build the Solution
- How to reference the BlazorFormManager Razor Class Library package
- How to integrate the BlazorFormManager package into your own project
- How to create a user registration component with BlazorFormManager
- How to create domain models for the user registration component
- How to create modular, reusable Razor Components
- How to create a database with EntityFramework Core migrations
- How to customize the `ApplicationUser class` with personal information
- How to update the database's structure with the customization
- How to create ASP.NET Core Web API endpoints for user management
- How to customize application settings when configuring services

## Step 1: Creating a new Solution

Create an `ASP.NET Core hosted Blazor WebAssembly App` with authentication using
individual user accounts named `{APP NAMESPACE}`. {APP NAMESPACE} is a placeholder
that you should replace with the name (without whitespaces) of your project
(e.g. _BlazorFormManagerWalkthrough_).

The directory structure should be similar to this:

## Step 2: Updating the Solution

As a best-practice, in Visual Studio 2019 Community (or higher), update the Solution
using the `Manage NuGet Packages for Solution...` context menu option: right-click on
the Solution file item. In the `NuGet - Solution` window, click the `Updates` tab,
wait until Visual Studio is done refreshing all projects to be updated. Check the
`Select all packages` option and click the `Update` button on the same line. If
prompted, read and accept the `License Acceptance` in the dialog. To install and manage
packages using the dotnet CLI (outside Visual Studio), please visit
https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-dotnet-cli

## Step 3: Building the Solution

After all packages have been restored to their respective latest version, make sure
that the Solution builds without errors: Ctrl+Shift+B, or `dotnet build` from the
Solution's root directory using the `dotnet CLI`.

## Step 4: Referencing the BlazorFormManager Razor Class Library (RCL)

Add a reference to the BlazorFormManager RCL project. Alternatively, you can install
the binaries through the NuGet Gallery at https://www.nuget.org/packages/BlazorFormManager.

In Visual Studio's Package Manager Console, type: `Install-Package BlazorFormManager`

or:

In your project's root directory, from the dotnet CLI:
`dotnet add package BlazorFormManager`

## Step 5: Integrating the BlazorFormManager RCL into your client project

In your `{APP NAMESPACE}.Client` project, open the `index.html` file located under the
**wwwroot** folder and add the following lines:

1.  At the top, right before the closing `</head>` tag:
    - `<link href="_content/BlazorFormManager/css/styles.css" rel="stylesheet" />`,
    - `<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.0/dist/css/bootstrap.min.css" rel="stylesheet" />`,
      and optionally
    - `<link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.14.0/css/all.min.css" />`
2.  At the bottom, right before the closing `</body>` tag:
    - `<script src="_content/BlazorFormManager/js/BlazorFormManager.js"></script>`, 
    - `<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.0/dist/js/bootstrap.bundle.min.js"></script>`, and optionally
    - `<script src="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.14.0/js/all.min.js"></script>`

## Step 6: Creating a user registration component

In your `{APP NAMESPACE}.Client` project, under the `Pages` folder, create a Razor
Component named `Register.razor`, for instance. Replace the content of the created
.razor file with the following C# code:

_File: {APP NAMESPACE}.Client/Pages/Register.razor_

```C#
@page "/account/register"
@using System.Text.Json;
```

followed by this HTML-like markup:

```HTML
<FormManager @ref="manager" Model="Model" FormAction="api/account/register" OnSubmitDone="HandleSubmitDone" EnableProgressBar>
    <DemoHeader Title="Form Manager Demo: Composition" SubTitle="Create a new account"
                LogLevel="LogLevel" OnLogLevelChanged="level => manager.SetLogLevelAsync(level)" />
    <div class="row">
        <div class="col-md-8"><UserModelInputs Model="Model" /></div>
    </div>
    <SubmitButton Manager="manager" Text="Register me" />
    <DataAnnotationsValidator />
</FormManager>
```

The `@page` C# directive configures routing for the component: it allows navigation to
the component using the relative `/account/register` URL.

`<FormManager />` is a Razor component defined in the `BlazorFormManager` package that we
referenced earlier. We are using composition instead of inheritance to build this
registration component by embedding `<FormManager />` into another component. The form
will be submitted to the `api/account/register` API endpoint as indicated by the
`FormAction` attribute.

The code seems pretty simple and self-explanatory somehow due the expressive nature of
Razor components.

The `<DataAnnotationsValidator />` component ensures client-side validation rules are
enforced thanks to the data annotation attributes `[Required], [StringLength]` and so
on. We'll see them soon when defining the domain models.

The `EnableProgressBar` attribute allows a progress bar to show up during form submissions
when there is at least one file to upload.

The `@ref` attribute acquires a reference to the rendered DOM element and stores it
into the `manager` member variable below.

The other components such as `<DemoHeader/>`, `<UserModelInputs/>`, and `<SubmitButton/>`
will be explained in more detail a bit later when we create them.

Usually, when the code starts to get too long it's a good idea to separate it from the UI
elements by placing it in a "code-behind" class file. But for the sake of simplicity,
copy and paste the following C# code block below the above markup:

_File: {APP NAMESPACE}.Client/Pages/Register.razor_

```C#
@code {
  private static readonly JsonSerializerOptions CaseInsensitiveJson =
   new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
  private FormManager<RegisterUserModel> manager;
  private ConsoleLogLevel LogLevel => manager?.LogLevel ?? ConsoleLogLevel.None;
  private RegisterUserModel Model = new RegisterUserModel();

  [Inject] NavigationManager NavigationManager { get; set; }

  private void HandleSubmitDone(FormManagerSubmitResult result)
  {
      // Succeeded means the server responded with a success status code.
      // But we still have to find out how the action really came out.
      if (result.Succeeded && result.XHR.IsJsonResponse)
      {
          try
          {
              // Since the response is in JSON format, let's parse it and investigate.
              var response = JsonSerializer.Deserialize<RegisterUserModelResult>(
                  result.XHR.ResponseText, CaseInsensitiveJson);

              if (!response.Success)
              {
                  manager.SubmitResult = FormManagerSubmitResult.Failed(result, response.Error);
              }
              else if (!string.IsNullOrEmpty(response.Message))
              {
                  manager.SubmitResult = FormManagerSubmitResult.Success(result, response.Message);
                  if (response.SignedIn)
                  {
                      NavigationManager.NavigateTo("/account/update", true);
                  }
                  else
                  {
                      // invalidate the form by setting a new model
                      Model = new RegisterUserModel();
                      StateHasChanged();
                  }
              }
          }
          catch (Exception ex)
          {
              System.Diagnostics.Trace.WriteLine(ex);
          }
      }
  }
}
```

The `HandleSubmitDone` routine is called an event callback handler. Whenever an
instance of the `FormManagerBase` class finishes submitting the form, and regardless
of the outcome (success or failure), the `OnSubmitDone` event callback is triggered,
hence executing the aforementioned routine.

Remember to replace **{APP NAMESPACE}** with the namespace of your project (e.g.
BlazorFormManagerWalkthrough). A couple of steps are ahead before getting a functional
application using the above code.

## Step 7: Creating the domain models

In the _**{APP NAMESPACE}.Client**_ root directory (in the same directory as the
**Pages** folder), create the **Models** folder then create a C# class file named
`UserModels.cs`. Copy and paste the following code into this file (and replace
{APP NAMESPACE} appropriately):

_File: {APP NAMESPACE}.Client/Models/UpdateUserModel.cs_

```C#
 using System.ComponentModel.DataAnnotations;
 namespace {APP NAMESPACE}.Client.Models
 {
     public class UpdateUserModel
     {
         [Required]
         [StringLength(100)]
         public string FirstName { get; set; }

         [Required]
         [StringLength(100)]
         public string LastName { get; set; }

         [Required]
         [StringLength(255)]
         [EmailAddress]
         public string Email { get; set; }

         [StringLength(30)]
         public string PhoneNumber { get; set; }
     }

     public class RegisterUserModel : UpdateUserModel
     {
         [Required]
         [DataType(DataType.Password)]
         public string Password { get; set; }

         [Required]
         [DataType(DataType.Password)]
         [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
         public string ConfirmPassword { get; set; }
     }

     public class RegisterUserModelResult
     {
         public bool Success { get; set; }
         public string Error { get; set; }
         public string Message { get; set; }
         public bool SignedIn { get; set; }
     }
 }
```

These class models will be used to submit a user's registration data and update their
information. The properties of the first class, `UpdateUserModel`, are common to both
registering and updating a user's personal information. It serves as the base class for
`RegisterUserModel`.

At this point the Solution no longer builds. Don't worry, we're going to fix everything
shortly.

## Step 8: Creating more components

In the **Shared** folder (which is in the same as the **Models** folder), create the
missing components, namely _DemoHeader_, _UserModelInputs_, and _SubmitButton_. These
components may contain others that we will also define later.

### DemoHeader.razor component:

This component contains the page's title and subtitle headings. It also contains a
dropdown allowing the user to control the reporting level in the browser's console
when debugging. This is useful when you as developer need to figure out problems
that might occur during development. Of course, this shouldn't be shipped into
production.

_File: {APP NAMESPACE}.Client/Pages/Shared/DemoHeader.razor_

```HTML
<div class="row my-3">
  <div class="col">
      <h3>
        <span class="float-left">@Title</span>
        <span class="float-md-right my-2">
          <span class="input-group">
            <span class="input-group-prepend">
              <span class="input-group-text">
                  <i class="fa fa-terminal"></i>&nbsp;console log:
              </span>
            </span>
            <select class="form-select" value="@LogLevel"
              @onchange="HandleLogLevelChanged">
              @foreach (var name in Enum.GetNames(typeof(ConsoleLogLevel)))
              {
                  <option value="@name">@name</option>
              }
            </select>
          </span>
        </span>
      </h3>
  </div>
</div>
<h4 class="mb-3 pt-3 border-top">@SubTitle</h4>
```

```C#
@code {
    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public string Title { get; set; }
    [Parameter] public string SubTitle { get; set; }
    [Parameter] public ConsoleLogLevel LogLevel { get; set; }
    [Parameter] public EventCallback<ConsoleLogLevel> OnLogLevelChanged { get; set; }

    private async Task HandleLogLevelChanged(ChangeEventArgs e)
    {
        if (Enum.TryParse<ConsoleLogLevel>(e.Value.ToString(), out var result))
        {
            LogLevel = result;
            if (OnLogLevelChanged.HasDelegate)
                await OnLogLevelChanged.InvokeAsync(LogLevel);
        }
    }
}
```

### UserModelInputs.razor component:

This component will be used both for user registration and editing their personal
information. It contains the following fields:

- FirstName (required)
- LastName (required)
- Email (required)
- PhoneNumber (optional)
- Password (required, registration only)
- ConfirmPassword (required, registration only)

A file selector is included at the end to allow users to attach their photo.

_File: {APP NAMESPACE}.Client/Pages/Shared/UserModelInputs.razor_

```HTML
<div class="row">
    <div class="@ColWidth">
        <div class="form-group">
            <label class="sr-only" for="@nameof(Model.FirstName)">@FirstNameText</label>
            <InputText @bind-Value="Model.FirstName" class="form-control" id="@nameof(Model.FirstName)" title="@FirstNameText" placeholder="@FirstNameText" />
            <ValidationMessage For="() => Model.FirstName" />
        </div>
        <div class="form-group">
            <label class="sr-only" for="@nameof(Model.LastName)">@LastNameText</label>
            <InputText @bind-Value="Model.LastName" class="form-control" id="@nameof(Model.LastName)" title="@LastNameText" placeholder="@LastNameText" />
            <ValidationMessage For="() => Model.LastName" />
        </div>
        <div class="form-group">
            <label class="sr-only" for="@nameof(Model.Email)">@EmailText</label>
            <InputText @bind-Value="Model.Email" type="email" class="form-control" id="@nameof(Model.Email)" title="@EmailText" placeholder="@EmailText" disabled="@(!IsRegistration)" />
            <ValidationMessage For="() => Model.Email" />
        </div>
        <div class="form-group">
            <label class="sr-only" for="@nameof(Model.PhoneNumber)">@PhoneNumberText</label>
            <InputText @bind-Value="Model.PhoneNumber" class="form-control" id="@nameof(Model.PhoneNumber)" title="@PhoneNumberText" placeholder="@PhoneNumberText" />
            <ValidationMessage For="() => Model.PhoneNumber" />
        </div>
    </div>
    @if (Model is RegisterUserModel register)
    {
        <div class="@ColWidth">
            <div class="form-group">
                <label class="sr-only" for="@nameof(register.Password)">@PasswordText</label>
                <InputText type="password" @bind-Value="register.Password" class="form-control" id="@nameof(register.Password)" title="@PasswordText" placeholder="@PasswordText" />
                <ValidationMessage For="() => register.Password" />
            </div>
            <div class="form-group">
                <label class="sr-only" for="@nameof(register.ConfirmPassword)">@ConfirmPasswordText</label>
                <InputText type="password" @bind-Value="register.ConfirmPassword" class="form-control" id="@nameof(register.ConfirmPassword)" title="@ConfirmPasswordText" placeholder="@ConfirmPasswordText" />
                <ValidationMessage For="() => register.ConfirmPassword" />
            </div>
        </div>
    }
</div>
<div class="row">
    <div class="@ColWidth">
        <div class="form-group">
            <div class="custom-file mt-3 mb-3">
                <input type="file" class="custom-file-input" id="@id" title="@ChoosePhotoText">
                <label class="custom-file-label" for="@id">@ChoosePhotoText</label>
            </div>
        </div>
    </div>
</div>
```

```C#
@code {
    private readonly string id = $"InputFile_{Guid.NewGuid():n}";
    private bool IsRegistration => (Model is RegisterUserModel);
    private string ColWidth => IsRegistration ? "col-6" : "col-12";

    [Parameter] public UpdateUserModel Model { get; set; }
    [Parameter] public string FirstNameText { get; set; } = "First name";
    [Parameter] public string LastNameText { get; set; } = "Last name";
    [Parameter] public string EmailText { get; set; } = "Email";
    [Parameter] public string PhoneNumberText { get; set; } = "Your phone number";
    [Parameter] public string PasswordText { get; set; } = "New password";
    [Parameter] public string ConfirmPasswordText { get; set; } = "Confirm your new password";
    [Parameter] public string ChoosePhotoText { get; set; } = "Choose a photo";
}
```

Right now, you are certainly seing error messages like _`The type or namespace name 'ConsoleLogLevel' could not be found (are you missing a using directive or an assembly reference?).`_
Fix these errors by adding the following to the _\_Imports.razor_ file located under the
_{APP NAMESPACE}.Client_ project's root directory:

```C#
 @using {APP NAMESPACE}.Client.Models
 @using BlazorFormManager
 @using BlazorFormManager.Components
 @using BlazorFormManager.Components.Debugging
 @using BlazorFormManager.Debugging
```

If you followed along, you should be able to build the Solution again without errors.
Try it by running the app (CTRL+F5). However, getting the application to run doesn't
mean it's bug-free.

Navigate to `/account/register`, fill in the inputs and then click the **Register me**
button. You'll get the message: `An error occurred while uploading the form data.`
To find out more, select **Debug** in the `console log` dropdown at the top right of
the page. Below the submit button you'll notice that you've got a 404 HTTP error.
This means that the resource (API endpoint) to which you tried to send the form does
not exist.

Before moving on to the next step, let's do some cleanup:

- In the **Pages** folder in the root of _{APP NAMESPACE}.Client_, delete the
  **Counter** and **FetchData** components.
- Under _**Dependencies/Projects**_, remove the reference to the project
  **{APP NAMESPACE}.Shared** and delete that project altogether from the Solution.
- Now go to the **_{APP NAMESPACE}.Client/Shared_** folder and delete the file
  _**SurveyPrompt.razor**_.
- Open up the _**NavMenu.razor**_ file and change the content of this
  `<div class="@NavMenuCssClass" @onclick="ToggleNavMenu">` to ressemble this:
  ```HTML
  <div class="@NavMenuCssClass" @onclick="ToggleNavMenu">
      <ul class="nav flex-column">
          <li class="nav-item px-3">
              <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                  <span class="oi oi-home" aria-hidden="true"></span> Home
              </NavLink>
          </li>
          <AuthorizeView>
              <NotAuthorized>
                  <li class="nav-item px-3">
                      <NavLink class="nav-link" href="account/register">
                          <i class="fa fa-user"></i>&nbsp;User Registration
                      </NavLink>
                  </li>
              </NotAuthorized>
              <Authorized>
                  <li class="nav-item px-3">
                      <NavLink class="nav-link" href="account/update">
                          <i class="fa fa-user"></i>&nbsp;Update User Info
                      </NavLink>
                  </li>
              </Authorized>
          </AuthorizeView>
      </ul>
  </div>
  ```
- In **{APP NAMESPACE}.Client/Pages/Index.razor**, replace the content with the markup
  below:
  ```HTML
  @page "/"
  <h3 class="pb-3 border-bottom">Welcome to Blazor Form Manager</h3>
  <p>Pick up a demo activity to get started.</p>
  <ul>
     <li>Composition Demo: <a href="account/register">User registration</a></li>
     <li>
         Inheritance Demo: <a href="account/update">Update user info</a>
         <AuthorizeView>
             <NotAuthorized>(authentication required)</NotAuthorized>
         </AuthorizeView>
     </li>
  </ul>
  ```

## Step 9: Creating the database

The ASP.NET Core hosted `Blazor WebAssembly App` template with individual user accounts
authentication generates 3 projects:

- {APP NAMESPACE}.Client
- {APP NAMESPACE}.Server
- {APP NAMESPACE}.Shared

We previously deleted the _{APP NAMESPACE}.Shared_ project because we don't need it here.
Looking closer into the _{APP NAMESPACE}.Server_ project, we'll see that the template has
layed out the structure for an initial `EntityFramework Core` migration in the
**{APP NAMESPACE}.Server/Data/Migrations** folder. If we apply that migration, it will
generate the boiler-plate `Transact SQL` (T-SQL) script and execute it in order to create
the database and the tables required for the application. But before we do, delete the
`WeatherForecastController.cs` file found under _{APP NAMESPACE}.Server/Controllers_.

Now, let's apply the migration named `00000000000000_CreateIdentitySchema.cs` located
under the _Data/Migrations_ folder. In Visual Studio's Package Manager Console, found
in the menu _Tools > NuGet Package Manager > Package Manager Console_, make sure that
you select _{APP NAMESPACE}.Server_ as the **default project** then type the following
command:

`Update-Database` and press the _Enter_ key. This creates the application's database.
You can view the database by opening the `SQL Server Object Explorer` found in the menu
_View_. Expand the nodes _SQL Server/(localdb)\MSSQLLocalDB/Databases_. There, you should
find a database named something like
_**aspnet-{APP NAMESPACE}.Server-XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX**_ where
_XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX_ is Guid (Globally Unique Identifier).

If you can't find the database, make sure to add the server by clicking the
`Add SQL Server` icon at the top left corner of the `SQL Server Object Explorer` window.

## Step 10: Customizing the `ApplicationUser` class

In the _{APP NAMESPACE}.Server/Models_ folder you'll find the `ApplicationUser.cs` file.
Open it up in the editor. We're going to customize this class by adding _FirstName_,
_LastName_, and _Photo_ properties. Make changes to have something like this:

_File: {APP NAMESPACE}.Server/Models/ApplicationUser.cs_

```C#
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace {APP NAMESPACE}.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        public byte[] Photo { get; set; }
    }
}
```

By adding the `[Required]` attribute we tell EntityFramework Core to not allow NULL
values for the properties (columns in the database) that are marked with it.

The `[StringLength(100)]` attributes make sure that the columns are not accepting values
longer than 100 characters.

Make sure that the project builds by pressing _Ctrl+B_ or by going to the menu
_Build > Build {APP NAMESPACE}.Server_.

In the `Package Manager Console`, update the database by typing the following commands:

`Add-Migration CustomizeApplicationUser` (press **_Enter_** key)

`Update-Database` (press **_Enter_** key)

Run the application with **Ctrl+F5**. Navigate to `/account/register`, fill in the form
and submit it. You'll still get a 404 HTTP error. Let's fix that now.

## Step 11: Creating the Web API endpoints

### 11.1 Creating the user registration Web API endpoint

In the project _{APP NAMESPACE}.Server_, right-click on the **Controllers** folder and
add an empty API Controller (_Add > Controller... > API Controller - Empty_). In the
**Add New Item** dialog, enter **_AccountController.cs_** for the controller's name.
Click _Add_. This will add an empty ApiController to the project. Make changes to the
_AccountController_ class to make it ressemble a variation of the following:

_File: {APP NAMESPACE}.Server/Controllers/AccountController.cs_

```C#
using {APP NAMESPACE}.Client.Models;
using {APP NAMESPACE}.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace {APP NAMESPACE}.Server.Controllers
{
    // [Authorize] // TODO: Uncomment later this AuthorizeAttribute
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register
        (
            [FromForm] RegisterUserModel model,
            [FromServices] SignInManager<ApplicationUser> signInManager
        )
        {
            string message;
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
                    message = "Your account has been successfully created. ";
                    _logger.LogInformation("User created a new account with password.");
                    var signedIn = false;

                    if (emailConfirmed)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        signedIn = true;

                        message += "You have been automatically signed in because " +
                            "account confirmation is disabled. ";

                        _logger.LogInformation(message);
                    }
                    else
                    {
                        message += "You must confirm your email address before you can log in. ";
                    }

                    message += photoMessage;
                    return Ok(new { success = true, message, signedIn });
                }

                var sb = new System.Text.StringBuilder();

                foreach (var error in result.Errors)
                    sb.AppendLine(error.Description);

                message = sb.ToString();
            }
            catch (Exception ex)
            {
                message = $"An unexpected error of type {ex.GetType().FullName} occurred while creating the user.";
                _logger.LogError(message);
            }
            return Ok(new { success = false, error = message });
        }

        private async Task<(bool success, string message)> SetPhotoAsync(ApplicationUser user)
        {
            bool success = true;
            string message = null;
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
                    (success, message) = (true, $"Total size of uploaded file: {content.Length / 1024d:N2} kb.\n");
                }
                else
                {
                    (success, message) = (false, "Only photos of type JPEG (with file extension .jpeg or .jpg) are supported.\n");
                }
            }
            return (success, message);
        }
    }
}
```

Wow! That's a lot of code at once. Let's explain the essential parts.

We start by securing our controller by adding the `[Authorize]` attribute, which makes
it accessible to only authenticated users. It's currently commented out to allow testing.

The `[Route("api/[controller]")]` attribute defines the URL pattern of the API endpoint.
In other words, to reach this controller, client applications must make API calls
beginning with `api/account`. The text casing isn't important: it can be `api/Account` or
`API/ACCOUNT`, it makes no difference.

It's important to note that we can change this URL pattern for our API endpoints. It
doesn't have to be necessarily `api/[controller]`. To find out more about routing in
ASP.NET Core Web APIs, search for _**`asp.net core web api routing`**_ on the web.

We then inject some dependencies into the `AccountController` class that will allow us
to perform user registration, namely the `UserManager<ApplicationUser>` class. This class
is a feature of the `Microsoft.AspNetCore.Identity` package that makes it possible to
manage application users in the database we created earlier.

The `Register` method is the API endpoint our form is pointing to as defined in the
`<FormManager>` component's declaration: `FormAction="api/account/register"`. Since we
secured our `AccountController` with the `[Authorize]` attribute, we must make this
method accessible to everybody using the `[AllowAnonymous]` attribute because we're
registering new (**unauthenticated** and **unauthorized**) users through this API.

The `Register` method accepts 2 parameters:

1. `[FromForm] RegisterUserModel model`: This is the model we submit via our registration
   form. That's why it's decorated with the `[FromForm]` attribute.
2. `[FromServices] SignInManager<ApplicationUser> signInManager`: This is a
   dependency-injected parameter which will allow us to sign the user automatically in if
   the current user registration policy allows it.

The rest of the code is straight forward:

- Create and initialize a new `ApplicationUser` instance with the received form model.
- Check if the user attached a valid photo (.jpg or .jpeg file); if so, retrieve the
  content of the uploaded file and assign it to the `ApplicationUser.Photo` property.
- Create the user in the database using an instance of the `UserManager<ApplicationUser>`
  class.
- Check the result:
  - If the user creation was successful, check if email confirmation is required; if not,
    sign the user in.
  - If the registration was not successful, report errors back to the user.

Try creating a user again at `/account/register`. This time, everything should work as
expected but we won't be able to sign in after creating a new account. For one reason:

By default, the template we used to create the Solution requires account confirmation.
Take a look at the `{APP NAMESPACE}.Server/Startup.cs` file, in the method where
services are configured:

_File: {APP NAMESPACE}.Server/Startup.cs_

```C#
public void ConfigureServices(IServiceCollection services)
{
    // rest of the code omitted

    services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();

    // rest of the code omitted
}
```

Let's move this hard-coded configuration option to the application's configuration file
`appsettings.json`. Since we're in developer mode, let's rather put this in the file
`appsettings.Development.json`. This application settings file is a JSON file that looks
like the one below:

_File: {APP NAMESPACE}.Server/appsettings.Development.json_

```JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "IdentityServer": {
    "Key": {
      "Type": "Development"
    }
  }
}
```

Let's add a new key to this file named `RequireConfirmedAccount` and set its value to
`false`, like so:

_File: {APP NAMESPACE}.Server/appsettings.Development.json_

```JSON
{
    "RequireConfirmedAccount": false
}
```

The resulting file should be similar to:

_File: {APP NAMESPACE}.Server/appsettings.Development.json_

```JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "IdentityServer": {
    "Key": {
      "Type": "Development"
    }
  },
  "RequireConfirmedAccount": false
}
```

Now, let's retrieve this value in the `Startup.ConfigureServices` method:

_File: {APP NAMESPACE}.Server/Startup.cs_

```C#
public void ConfigureServices(IServiceCollection services)
{
    // rest of the code omitted

    // Get the configuration value for new account confirmation
    // requirement; fallback to true by default should it be missing.
    var requireConfirmation = Configuration.GetValue("RequireConfirmedAccount", true);

    services.AddDefaultIdentity<ApplicationUser>(options =>
        options.SignIn.RequireConfirmedAccount = requireConfirmation)
        .AddEntityFrameworkStores<ApplicationDbContext>();

    // rest of the code omitted
}
```

Hit `Ctrl+F5` and navigate to `/account/register`. Try registering a new user with a
different email address. It works like a charm and you're immediately signed in and
redirected to `/account/update`. You'll be disappointed to see that there's nothing at
this address. At least for the moment.

### 11.2 Creating the user update Web API endpoint

To make the page component at `/account/update` work, we'll start with the endpoint that
will allow users to update their account information.

In the `AccountController.cs` file, add a new HTTP GET method named `Update` with the
following implementation:

_File: {APP NAMESPACE}.Server/Controllers/AccountController.cs_

```C#
using System.Security.Claims; // <- This goes to the top of the file

[HttpPost("Update")]
public async Task<IActionResult> Update([FromForm] UpdateUserModel model)
{
    string message;
    try
    {
        var user = await _userManager.FindByNameAsync(User.Identity.Name);

        if (user != null)
        {
            var (success, photoMessage) = await SetPhotoAsync(user);
            if (!success) return Ok(new { success, error = photoMessage });

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User updated account information.");
            message = $"Your account information has been updated. {photoMessage}";

            return Ok(new { success = true, message });
        }
        else
        {
            message = "User not found!";
        }
    }
    catch (Exception ex)
    {
        message = $"An unexpected error of type {ex.GetType().FullName} occurred while updating the user.";
    }

    return Ok(new { success = false, error = message });
}
```

### 11.3 Putting it all together

The whole `AccountController.cs` file should ressemble a variation of the following:

_File: {APP NAMESPACE}.Server/Controllers/AccountController.cs_

```C#
using {APP NAMESPACE}.Client.Models;
using {APP NAMESPACE}.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace {APP NAMESPACE}.Server.Controllers
{
    // [Authorize] // TODO: Uncomment later this AuthorizeAttribute
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register
        (
            [FromForm] RegisterUserModel model,
            [FromServices] SignInManager<ApplicationUser> signInManager
        )
        {
            string message;
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
                    message = "Your account has been successfully created. ";
                    _logger.LogInformation("User created a new account with password.");
                    var signedIn = false;

                    if (emailConfirmed)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        signedIn = true;

                        message += "You have been automatically signed in because " +
                            "account confirmation is disabled. ";

                        _logger.LogInformation(message);
                    }
                    else
                    {
                        message += "You must confirm your email address before you can log in.  ";
                    }

                    message += photoMessage;
                    return Ok(new { success = true, message, signedIn });
                }

                var sb = new System.Text.StringBuilder();

                foreach (var error in result.Errors)
                    sb.AppendLine(error.Description);

                message = sb.ToString();
            }
            catch (Exception ex)
            {
                message = $"An unexpected error of type {ex.GetType().FullName} occurred while creating the user.";
                _logger.LogError(message);
            }
            return Ok(new { success = false, error = message });
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromForm] UpdateUserModel model)
        {
            string message;
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);

                if (user != null)
                {
                    var (success, photoMessage) = await SetPhotoAsync(user);
                    if (!success) return Ok(new { success, error = photoMessage });

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;

                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User updated account information.");
                    message = $"Your account information has been updated. {photoMessage}";

                    return Ok(new { success = true, message });
                }
                else
                {
                    message = "User not found!";
                }
            }
            catch (Exception ex)
            {
                message = $"An unexpected error of type {ex.GetType().FullName} occurred while updating the user.";
            }

            return Ok(new { success = false, error = message });
        }

        private async Task<(bool success, string message)> SetPhotoAsync(ApplicationUser user)
        {
            bool success = true;
            string message = null;
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
                    message = $"Total size of uploaded file: {content.Length / 1024d:N2} kb.\n";
                }
                else
                {
                    (success, message) = (false, "Only photos of type JPEG (with file extension .jpeg or .jpg) are supported.\n");
                }
            }
            return (success, message);
        }
    }
}
```

## Step 12: Building a user account update component

This is the part where things are getting tricky. This is because authentication is now
involved, which is not an easy task getting the implementation done right. Especially
when using a JavaScript client to send HTTP requests. But don't worry! Since this is a
walkthrough, by the end of day we'll eventually get to our destination.

Back to the client application, in the folder _{APP NAMESPACE}.Client/Pages_, create a
new Razor component named `UserEditor.razor`. Put the following markup code into it:

_File: {APP NAMESPACE}.Client/Pages/UserEditor.razor_

```HTML
@page "/account/update"
@inherits FormManagerBase<UpdateUserModel>

<EditForm Model="Model" OnValidSubmit="HandleValidSubmit" OnInvalidSubmit="HandleInvalidSubmit"
            id="@FormId" action="api/account/update" enctype="multipart/form-data"
            @attributes="AdditionalAttributes">
    <DemoHeader Title="Form Manager Demo: Inheritance" SubTitle="Update User Information"
                LogLevel="LogLevel" OnLogLevelChanged="level => LogLevel = level" />
    <div class="row">
        <div class="col-md-4"><UserModelInputs Model="Model" /></div>
        <div class="col-md-4"><Base64RemoteImage Src="api/account/photo" @ref="remoteImgRef" /></div>
    </div>
    <SubmitButton Manager="this" Text="Save" ForceSubmit />
    <DataAnnotationsValidator />
</EditForm>
<FormSubmitResultView Result="SubmitResult" />
@if (IsDebug)
{
    <FormDebugInfo Model="XhrResult" Options="DebugOptions" />
    <UnsupportedBrowserProperties Model="AjaxUploadNotSupported?.ExtraProperties" />
}
@if (IsUploadingFiles)
{
    <UploadProgressBar Progress="Progress" UploadStatusMessage="@UploadStatus" OnCancelRequested="() => AbortRequested = true" />
}

@code {
    private Base64RemoteImage remoteImgRef;
}
```

This time we are implementing our component using inheritance instead of composition as
we did within the `Register.razor` file. As you can see, the HTML markup is a bit more
verbose than the `Register` component. Because this is exactly the markup the component
`<FormManager />` contains less what's in between the `<EditForm></EditForm>` tags.

We can see at the beginning of the markup above that our `<UserEditor />` component
inherits from the strongly-typed generic `FormManagerBase<TModel>` class, where TModel
is `UpdateUserModel`. We then have the same `DemoHeader`, `UserModelInputs`, and
`SubmitButton` components as in the registration component.

However, there's a new `<Base64RemoteImage />` component that the `<EditForm />`
component contains. Before continuing with the code-behind let's create this one now.
In the `{APP NAMESPACE}.Client/Shared` folder, create a new Razor component file named
`Base64RemoteImage.razor` and put the following markup into it:

_File: {APP NAMESPACE}.Client/Shared/Base64RemoteImage.razor_

```HTML
@inject HttpClient Http

@if (!string.IsNullOrEmpty(base64Photo))
{
    <img src="data:image/jpeg;base64, @base64Photo" alt="User's profile picture"
         class="img-fluid" style="max-height:275px;"/>
}
```

followed by this C# code block:

```C#
@code {
    private string base64Photo;

    [Parameter] public string Src { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        try
        {
            var result = await Http.GetAsync(Src);
            result.EnsureSuccessStatusCode();
            var bytes = await result.Content.ReadAsByteArrayAsync();
            base64Photo = Convert.ToBase64String(bytes);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex);
        }
    }
}
```

What this component does is retrieve a user's photo from an API endpoint defined by the
parameterized `Src` property using a dependency-injected `HttpClient` instance. When the
component is initialized, the asynchronous `RefreshAsync` method is called, which makes
an API call to the server using the specified `Src` property's value.

When the server responds successfully, the content is read into a one-dimensional array
of type `byte[]` and then converted into a [base64-encoded](https://en.wikipedia.org/wiki/Base64)
string. This string represents an in-memory image defined on the `src` attribute of an
`<img />` tag. This is not ideal but for now it'll do the job: display the currently
logged-in user's photo.

Let's get back to making the `<UserEditor />` component more useful.

In the same **Pages** folder, create a new **class** named `UserEditor.razor.cs`. After
adding this file, it should open up in an editor window with content similar to this:

_File: {APP NAMESPACE}.Client/Pages/UserEditor.razor.cs_

```C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace {APP NAMESPACE}.Client.Pages
{
    public class UserEditor
    {
    }
}
```

You'll notice that the `Error List` window displays the following message:
`Missing partial modifier on declaration of type 'UserEditor'; another partial declaration of this type exists`.
This is because we now have two declarations of the same class: one that was generated
by Visual Studio when we created the `UserEditor.razor` file, and one that we just
created. Since both these files contain a declaration of the `UserEditor class`, we have
to add the `partial` modifier to ours.

Because of auto-generated code by design-time tools in Visual Studio, the `partial`
modifier allows us to define a same type multiple times in different files.

To correct this error, add the `partial` modifier to the new _UserEditor_ class. We also
seize the opportunity to initialize the component's `Model` property (inherited from
`FormManagerBase<UpdateUserModel>`). Change the file's content to this:

_File: {APP NAMESPACE}.Client/Pages/UserEditor.razor.cs_

```C#
using {APP NAMESPACE}.Client.Models;
using System.Threading.Tasks;

namespace {APP NAMESPACE}.Client.Pages
{
    public partial class UserEditor
    {
        protected override Task OnInitializedAsync()
        {
            Model = new UpdateUserModel();
            return base.OnInitializedAsync();
        }
    }
}
```

Let's continue with the implementation of the component. From its markup we know that it
has a model of type `UpdateUserModel`. Since the purpose of the component is to update
the details of an existing user, we need a way to retrieve some of the information of the
currently logged-in user. Let's put the implementation of this class on hold for a
moment and focus on the creation of the API endpoint for this new requirement.

### Step 12.1: Implementing token-based authentication

Back to the **{APP NAMESPACE}.Server** project where we're going to add a new method to
the `AccountController` class. The purpose of this method is to return user information
for the currently logged-in user. Copy and paste the following methods into the
`AccountController` class, preferrably above the `Update` method we defined earlier.

_File: {APP NAMESPACE}.Server/Controllers/AccountController.cs_

```C#
// ...code omitted for brevity
public class AccountController : ControllerBase
{
    // ...code omitted for brevity
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        var user = _userManager.Users.FirstOrDefault();
        if (user != null)
        {
            return Ok(new { user.FirstName, user.LastName, user.Email, user.PhoneNumber });
        }
        return NotFound();
    }

    [HttpGet("Photo")]
    public IActionResult Photo()
    {
        var user = await _userManager.Users.FirstOrDefault();
        if (user != null && user.Photo != null)
        {
            return File(user.Photo, "image/jpeg");
        }
        return NotFound();
    }
    // ...code omitted for brevity
}
```

The above `GetInfo` method attempts to retrieve the first available user in the database.
If found, returns an anonymous type with a few properties like `FirstName`, etc;
otherwise, a 404 HTTP error (NotFound) is returned.

The `Photo` method returns the first user's binary photo as a `FileContentResult`.

The methods are decorated with the `[AllowAnonymous]` attribute, which allows non
authenticated users to call the APIs. This is a temporary implementation, just to make
sure that our component works as expected.

Before we continue, let's try out the `<UserEditor />` component we've built so far.
Press `Ctrl+F5` to run the application and navigate to `/account/update`. The page
displays a blank form because the component isn't doing anything useful right now.

Let's change the `partial class UserEditor` so that we can call the above test APIs.

_File: {APP NAMESPACE}.Client/Pages/UserEditor.razor.cs_

```C#
using BlazorFormManager;
using {APP NAMESPACE}.Client.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace {APP NAMESPACE}.Client.Pages
{
    public partial class UserEditor
    {
        [Inject] private HttpClient Http { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // This isn't an unnecessary assignment; EditForm needs an initialized Model.
            // Do the initialization before calling any asynchronous method.
            Model = new UpdateUserModel();
            try
            {
                var user = await Http.GetFromJsonAsync<UpdateUserModel>("api/account/info");
                Model = user;
            }
            catch (Exception ex)
            {
                SubmitResult = FormManagerSubmitResult.Failed(null, ex.ToString(), false);
            }
            await base.OnInitializedAsync();
        }
    }
}
```

Make sure that you already have registered a user. If not, go to `/account/register` and
do so. If you now run the application again after these changes, you'll once again get a
blank form. But if you scroll down below the submit button you'll see a long detailed
error message starting with
`Microsoft.AspNetCore.Components.WebAssembly.Authentication.AccessTokenNotAvailableException`.

The URLs `api/account/info` and `api/account/photo` don't work within our application but
if you type them into your browser's address bar (e.g. https://localhost:44306/api/account/info
or https://localhost:44306/api/account/photo), you'll be surprised they're working.

So the question is: "Why are they not working in the app?". The response has to do with
the injected `HttpClient`. If you look into the `{APP NAMESPACE}.Client.Program.cs` file,
you'll find out that this HttpClient is configured as a named HttpClient and registered
as a transient service:

_File: {APP NAMESPACE}.Client/Program.cs_

```C#
// code omitted for brevity
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("app");

        builder.Services.AddHttpClient("{APP NAMESPACE}.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
            .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        // Supply HttpClient instances that include access tokens when making requests to the server project
        builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("{APP NAMESPACE}.ServerAPI"));

        builder.Services.AddApiAuthorization();

        await builder.Build().RunAsync();
    }
}
```

To be more specific, the line with the call to the method
`AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()` is the reason why our
`<UserEditor />` component is not working. This is normal because, usually, the API
endpoints within the application's base URI are secured and need an access token. And
this `BaseAddressAuthorizationMessageHandler` class makes sure an access token gets
attached to every outgoing HTTP request made with an injected instance of `HttpClient`.
Since we're not authenticated yet, thus the exception thrown when making an HTTP call
with that client.

If you comment out that line and re-run the app it should display the first user it can
grab in the database.

_File: {APP NAMESPACE}.Client/Program.cs_

```C#
// code omitted for brevity
public class Program
{
    public static async Task Main(string[] args)
    {
        // code omitted for brevity

        builder.Services.AddHttpClient("{APP NAMESPACE}.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            //.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

        // code omitted for brevity
    }
}
```

Make some changes to the user's account details and click the `Save` button. It should
work like a charm. Now that we know our `<UserEditor />` component is working, let's
finalize it by securing the application again, make changes to the following files as
shown next:

_File: {APP NAMESPACE}.Client/Program.cs_

```C#
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace {APP NAMESPACE}.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddHttpClient("{APP NAMESPACE}.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("{APP NAMESPACE}.ServerAPI"));

            builder.Services.AddApiAuthorization();

            await builder.Build().RunAsync();
        }
    }
}
```

_File: {APP NAMESPACE}.Server/Controllers/AccountController.cs_

```C#
using {APP NAMESPACE}.Client.Models;
using {APP NAMESPACE}.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace {APP NAMESPACE}.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register
        (
            [FromForm] RegisterUserModel model,
            [FromServices] SignInManager<ApplicationUser> signInManager
        )
        {
            string message;
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
                    message = "Your account has been successfully created. ";
                    _logger.LogInformation("User created a new account with password.");
                    var signedIn = false;

                    if (emailConfirmed)
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        signedIn = true;

                        message += "You have been automatically signed in because " +
                            "account confirmation is disabled. ";

                        _logger.LogInformation(message);
                    }
                    else
                    {
                        message += "You must confirm your email address before you can log in. ";
                    }

                    message += photoMessage;
                    return Ok(new { success = true, message, signedIn });
                }

                var sb = new System.Text.StringBuilder();

                foreach (var error in result.Errors)
                    sb.AppendLine(error.Description);

                message = sb.ToString();
            }
            catch (Exception ex)
            {
                message = $"An unexpected error of type {ex.GetType().FullName} occurred while creating the user.";
                _logger.LogError(message);
            }
            return Ok(new { success = false, error = message });
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetInfo()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user != null)
            {
                return Ok(new { user.FirstName, user.LastName, user.Email, user.PhoneNumber });
            }
            return NotFound();
        }

        [HttpGet("Photo")]
        public async Task<IActionResult> Photo()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user != null && user.Photo != null)
            {
                return File(user.Photo, "image/jpeg");
            }
            return NotFound();
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromForm] UpdateUserModel model)
        {
            string message;
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);

                if (user != null)
                {
                    var (success, photoMessage) = await SetPhotoAsync(user);
                    if (!success) return Ok(new { success, error = photoMessage });

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;

                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User updated account information.");
                    message = $"Your account information has been updated. {photoMessage}";

                    return Ok(new { success = true, message });
                }
                else
                {
                    message = "User not found!";
                }
            }
            catch (Exception ex)
            {
                message = $"An unexpected error of type {ex.GetType().FullName} occurred while updating the user.";
            }

            return Ok(new { success = false, error = message });
        }

        private async Task<(bool success, string message)> SetPhotoAsync(ApplicationUser user)
        {
            bool success = true;
            string message = null;
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
                    message = $"Total size of uploaded file: {content.Length / 1024d:N2} kb.\n";
                }
                else
                {
                    (success, message) = (false, "Only photos of type JPEG (with file extension .jpeg or .jpg) are supported.\n");
                }
            }
            return (success, message);
        }
    }
}
```

In the _Startup.cs_ file we only need to change the chained method calls from:

```C#
services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();
```

to:

```C#
services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
    {
        options.IdentityResources["openid"].UserClaims.Add("name");
        options.ApiResources.Single().UserClaims.Add("name");
        options.IdentityResources["openid"].UserClaims.Add("role");
        options.ApiResources.Single().UserClaims.Add("role");
    });
```

This ensures that user claims from parsed authorization tokens are attached back to API
calls made to the server.

The final result should be similar to:

_File: {APP NAMESPACE}.Server/Startup.cs_

```C#
using {APP NAMESPACE}.Server.Data;
using {APP NAMESPACE}.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace {APP NAMESPACE}.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            // Get the configuration value for new account confirmation
            // requirement; fallback to true by default should it be missing.
            var requireConfirmation = Configuration.GetValue("RequireConfirmedAccount", true);

            services.AddDefaultIdentity<ApplicationUser>(options =>
                options.SignIn.RequireConfirmedAccount = requireConfirmation)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
                {
                    options.IdentityResources["openid"].UserClaims.Add("name");
                    options.ApiResources.Single().UserClaims.Add("name");
                    options.IdentityResources["openid"].UserClaims.Add("role");
                    options.ApiResources.Single().UserClaims.Add("role");
                });

            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
```

Finally, the _UserEditor.razor.cs_ file should ressemble a variation of the following:

_File: {APP NAMESPACE}.Client/Pages/UserEditor.razor.cs_

```C#
using BlazorFormManager;
using {APP NAMESPACE}.Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace {APP NAMESPACE}.Client.Pages
{
    [Authorize]
    public partial class UserEditor
    {
        [Inject] private HttpClient Http { get; set; }
        [Inject] private IAccessTokenProvider TokenProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // This isn't an unnecessary assignment; EditForm needs an initialized Model.
            // Do the initialization before calling any asynchronous method.
            Model = new UpdateUserModel();
            try
            {
                var user = await Http.GetFromJsonAsync<UpdateUserModel>("api/account/info");
                Model = user;
                await SetRequestHeadersAsync();
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
            }
            catch(Exception ex)
            {
                SubmitResult = FormManagerSubmitResult.Failed(null, ex.ToString(), false);
            }
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Attempts to retrieve an access token and add the "authorization" request
        /// header and a few others that will be used to configure the XMLHttpRequest
        /// object when submitting the form via AJAX.
        /// </summary>
        /// <returns></returns>
        private async Task SetRequestHeadersAsync()
        {
            var tokenResponse = await TokenProvider.RequestAccessToken();
            if (tokenResponse.TryGetToken(out var token))
            {
                RequestHeaders = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "authorization", $"Bearer {token.Value}" },
                    { "x-requested-with", "XMLHttpRequest" },
                    { "x-powered-by", "BlazorFormManager" },
                };
            }
            else
            {
                throw new InvalidOperationException("Could not get access token.");
            }
        }

        /// <summary>
        /// Check if a file was uploaded then refresh the photo when applicable.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override async Task HandleSubmitDoneAsync(FormManagerSubmitResult result)
        {
            if (result.Succeeded)
            {
                if (result.UploadContainedFiles)
                {
                    // refresh the photo
                    // remoteImgRef is privately declared in UserEditor.razor
                    await remoteImgRef.RefreshAsync();
                }
                if (result.XHR.IsJsonResponse)
                {
                    // From the result.XHR.ResponseText property,
                    // parse this kind of JSON object:
                    // { success: true, message: "Some message" }
                }
            }
            await base.HandleSubmitDoneAsync(result);
        }
    }
}
```
