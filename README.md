# BlazorFormManager

BlazorFormManager is an open-source Razor Class Library (RCL) for (both client and
server-side) Blazor projects. It provides core functionalities for handling AJAX form
submissions with zero or more files, and report back data upload progress. It does so
by enhancing the existing functionalities of an EditForm, including client-side
validations, form data and file upload progress report, abortion of an on-going
upload, and console logging support for troubleshooting.

It is flexible enough to allow advanced control, such as setting HTTP request headers,
over instances of the XMLHttpRequest object used to send requests, all from the C#/.NET
perspective.

# Introducing AutoEditForm

Automatically generate an EditForm with all appropriate inputs using only a model and
custom attributes. These new form display custom attributes control the way the UI is
presented and reduce the amount of code required to have a beautifully-layed-out and
fully-functional editable form. Adding a tiny amount of CSS you can further style your
form as you desire.

## AutoEditForm quick start (pseudo-code)

For a working sample please checkout the demo application in the project's repository.

### The sample model:

```C#
using BlazorFormManager.ComponentModel.ViewAnnotations;
using System;
using System.ComponentModel.DataAnnotations;

[FormDisplayDefault(ShowGroupName = true)]
public class AutoUpdateUserModel
{
    [DisplayIgnore]
    public string Id { get; set; }

    [Required]
    [StringLength(100)]
    [FormDisplay(GroupName = "Personal info", Icon = "fas fa-user")]
    public string FirstName { get; set; }

    [Required]
    [StringLength(100)]
    [FormDisplay(GroupName = "Personal info", Name = "Last / Family Name", Icon = "fas fa-user")]
    public string LastName { get; set; }

    [Required]
    [StringLength(255)]
    [EmailAddress]
    [FormDisplay(GroupName = "Contact details", Icon = "fas fa-envelope", UITypeHint = "email")]
    public string Email { get; set; }

    [StringLength(30)]
    [FormDisplay(GroupName = "Contact details", Icon = "fas fa-phone", UITypeHint = "phone")]
    public string PhoneNumber { get; set; }

    [FormDisplay(GroupName = "Please select", UIHint = "select", Name = "", Order = 2)]
    public int AgeRange { get; set; }

    [Range(typeof(DayOfWeek), "Monday", "Friday")]
    [FormDisplay(GroupName = "Please select", UIHint = "select", Name = "", Order = 1, Prompt = "[Favourite Working Day]", Icon = "fas fa-calendar")]
    public string FavouriteWorkingDay { get; set; }

    [FormDisplay(UITypeHint = "radio", Order = 3, Name = "What's your favourite color?")]
    public string FavouriteColor { get; set; }

    [FormDisplay(Order = 4, Name = "Enable two-factor authentication", Description = "Log in with your email and an SMS confirmation")]
    public bool TwoFactorEnabled { get; set; }
}
```

### The matching markup on the client (AutoEditFormUpdate.razor):

```HTML
@attribute [Authorize]
@page "/account/autoeditform"

<AutoEditForm Model="Model" FormAction="api/account/update" RequestHeaders="RequestHeaders" OnSubmitDone="HandleSubmitDone" OptionsGetter="GetOptions" @ref="Manager">
    <AfterDisplayGroups>
        <hr />
        <SubmitButton Manager="Manager" Text="Update" />
    </AfterDisplayGroups>
    <ChildContent>
        <Base64RemoteImage Src="api/account/photo" @ref="RemoteImgRef" />
        <CustomInputFile Name="Photo" />
        <DataAnnotationsValidator />
    </ChildContent>
</AutoEditForm>
```

### The code behind the markup (essential parts):

```C#
using BlazorFormManager.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public partial class AutoEditFormUpdate
{
  private static IEnumerable<SelectOptionList> _options;
  private IDictionary<string, object> RequestHeaders { get; set; }
  private AutoUpdateUserModel Model { get; set; }
  private AutoEditForm<AutoUpdateUserModel> Manager { get; set; }
  private Base64RemoteImage RemoteImgRef { get; set; }
  ...

  protected override async Task OnInitializedAsync()
  {
    try
    {
      Model = await Http.GetFromJsonAsync<AutoUpdateUserModel>("api/account/info");
      RequestHeaders = await HeadersProvider.CreateAsync();
      if (_options == null) _options = await Http.GetFromJsonAsync<IEnumerable<SelectOptionList>>("api/account/options");
    }
    catch (AccessTokenNotAvailableException ex)
    {
      ex.Redirect();
    }
    await base.OnInitializedAsync();
  }


  private IEnumerable<SelectOption> GetOptions(string propertyName)
    => _options?.Where(opt => opt.PropertyName == propertyName).FirstOrDefault()?.Items;
}
```

On the server, in a controller:

```C#
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
  [HttpGet("options")]
  public IEnumerable<SelectOptionList> GetOptions()
  {
      // These options could be retrieved from a database or another server-side store;
      // otherwise, there would be no point in making an HTTP request just to retrieve
      // static / hard-coded values. But hey, this is a demo project!
      var ageOptions = new[]
      {
          new SelectOption{ Id = "0", Value = "[Your most appropriate age]", IsPrompt = true },
          new SelectOption{ Id = "1", Value = "Minor (< 18)" },
          new SelectOption{ Id = "2", Value = "Below or 25" },
          new SelectOption{ Id = "3", Value = "Below or 30" },
          new SelectOption{ Id = "4", Value = "Below or 40" },
          new SelectOption{ Id = "5", Value = "Below 50" },
          new SelectOption{ Id = "6", Value = "Between 50 and 54" },
          new SelectOption{ Id = "7", Value = "Between 55 and 60" },
          new SelectOption{ Id = "8", Value = "Above 60" },
          new SelectOption{ Id = "8", Value = "Above 70" },
          new SelectOption{ Id = "8", Value = "Above 80" },
      };

      var colorOptions = new[]
      {
          new SelectOption("red", "Red"),
          new SelectOption("green", "Green"),
          new SelectOption("blue", "Blue"),
      };

      return new[]
      {
          new SelectOptionList(nameof(AutoUpdateUserModel.AgeRange), ageOptions),
          new SelectOptionList(nameof(AutoUpdateUserModel.FavouriteColor), colorOptions),
      };
  }
}
```

### With the help of a tiny amount of CSS...

```CSS
.auto-edit-form .form-body .child-content {
    padding: 1rem 2rem !important;
}

@media (min-width: 768px) {
    .auto-edit-form .form-body {
        display: flex;
        flex-direction: row;
    }

    .auto-edit-form .form-body .form-display-group {
        flex: 2;
    }

    .auto-edit-form .form-body .child-content {
        flex: 1;
    }
}
```

### Produce this form!

***

![The output](https://github.com/bigabdoul/BlazorFormManager/blob/assets/images/auto-edit-form-003.jpg?raw=true)

***

## BlazorFormManager quick start

Clone this repository into a directory on your device:

`> git clone https://github.com/bigabdoul/BlazorFormManager.git`

There are 3 projects found under the directory BlazorFormManager and the structure is
similar to:

- BlazorFormManager
  - src
    - BlazorFormManager
    - Demos
      - BlazorFormManager.Demo.Client
      - BlazorFormManager.Demo.Server

In the project's root directory you can find a Visual Studio Solution (.sln) file.
To launch with Visual Studio, double-click that file and make sure
`BlazorFormManager.Demo.Server` is the start-up project. Press CTRL+F5 (or the
appropriate key combination on your device).

### Quick Integration Steps

In your `{APP NAMESPACE}.Client` project, open the `index.html` file located under the
**wwwroot** folder and add the following lines:

1.  At the top, right before the closing `</head>` tag:
    - `<link href="_content/BlazorFormManager/css/styles.css" rel="stylesheet" />`,
      and optionally
    - `<link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.14.0/css/all.min.css" />`
2.  At the bottom, right before the closing `</body>` tag:
    - `<script src="_content/BlazorFormManager/js/BlazorFormManager.js"></script>`, and optionally
    - `<script src="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.14.0/js/all.min.js"></script>`
3.  Add the following namespace to the _\_Imports.razor_ file:

    _File: {APP NAMESPACE}.Client/\_Imports.razor_

    ```C#
    @using BlazorFormManager
    @using BlazorFormManager.Components
    @using BlazorFormManager.Components.Debugging
    @using BlazorFormManager.Debugging
    ```

4.  For instance, create a new user registration Razor Component with the `FormManager`
    component:

    ```HTML
    @page "/account/register"
    @using System.ComponentModel.DataAnnotations

    <FormManager @ref="form" Model="User" FormAction="api/account/register" EnableProgressBar>
      <div class="row">
          <div class="col-md-8">
            <div class="row">
              <div class="col-6">
                <div class="form-group">
                  <label class="sr-only" for="FirstName">First Name</label>
                  <InputText @bind-Value="User.FirstName" class="form-control" id="FirstName" title="First Name" placeholder="First Name" />
                  <ValidationMessage For="() => User.FirstName" />
                </div>
                <div class="form-group">
                    <label class="sr-only" for="LastName">Last Name</label>
                    <InputText @bind-Value="User.LastName" class="form-control" id="LastName" title="Last Name" placeholder="Last Name" />
                    <ValidationMessage For="() => User.LastName" />
                </div>
                <div class="form-group">
                    <label class="sr-only" for="Email">Email</label>
                    <InputText @bind-Value="User.Email" type="email" class="form-control" id="Email" title="Email" placeholder="Email" />
                    <ValidationMessage For="() => User.Email" />
                </div>
                <div class="form-group">
                    <label class="sr-only" for="PhoneNumber">Phone Number</label>
                    <InputText @bind-Value="User.PhoneNumber" class="form-control" id="PhoneNumber" title="Phone Number" placeholder="Phone Number" />
                    <ValidationMessage For="() => User.PhoneNumber" />
                </div>
              </div>
              <div class="col-6">
                <div class="form-group">
                  <div class="custom-file mt-3 mb-3">
                      <input type="file" class="custom-file-input" id="@id" title="Choose a photo">
                      <label class="custom-file-label" for="@id">Choose a photo</label>
                  </div>
                </div>
              </div>
            </div>
          </div>
      </div>
      <SubmitButton Manager="form" Text="Sign up" />
      <DataAnnotationsValidator />
    </FormManager>
    ```

    ```C#
    @code {
      private FormManager<RegisterUserModel> form;
      private RegisterUserModel User = new RegisterUserModel();

      public class RegisterUserModel
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

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
      }
    }
    ```

## Demonstration projects

The demonstration (or demo) projects have been created with Visual Studio 2019's Blazor
WebAssembly App template (ASP.NET Core 3.1 hosted with authentication using individual
user accounts).

## Walkthrough documentation

Check out the [Walkthrough](WALK-THROUGH.md) file for detailed instructions that guide
you through building a set of projects similar to the demo projects found in this
repository.

## Known issues

When trying to submit the form, under some circumstances, especially when updating a
model, the form does not respond to a button's submit request, e.g. an input of type
submit:

```HTML
<FormManager @ref="form" Model="User" FormAction="api/account/register">
  <button type="submit">Submit</button>
</FormManager>
```

There's another way of achieving the same result: by changing the button's type from
`submit` to `button` and adding an `@onclick` event handler.

```HTML
<FormManager @ref="form" Model="User" FormAction="api/account/register">
  <button type="button" @onclick="form.SubmitFormAsync">Submit</button>
</FormManager>
```

## Engage, contribute, and give feedback

If you want to make improvements to this project, some of the best ways to contribute are
to try things out, file issues, join in design conversations, and make pull-requests.
