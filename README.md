# BlazorFormManager

BlazorFormManager is an open-source Razor Class Library (RCL) for (both client and
server-side) Blazor projects. It provides core functionalities for handling AJAX form
submissions with zero or more files, and report back data upload progress. It does so
by enhancing the existing functionalities of an EditForm, including client-side
validations, form data and file upload progress report, abortion of an on-going
upload, and console logging support for troubleshooting.

It is flexible enough to allow advanced control, such as setting HTTP request headers,
over instances of the XMLHttpRequest object used to send requests, and supports 
automatic form (UI) generation from metadata-decorated .NET object models.

# New

## Google reCAPTCHA support

Easily integrate Google reCAPTCHA technology into any form based on the `FormManagerBase`
class, such as `AutoEditForm`.

```HTML
// File: Register.razor
@using BlazorFormManager.Components.Forms
@using Carfamsoft.Model2View.Annotations
@using System.ComponentModel.DataAnnotations

<AutoEditForm ReCaptcha="recaptchaOptions" Model="registerModel" Messages="Messages"
    EnableProgressBar="false" HideOnSuccess RequireModel EnableChangeTracking>
    <AfterDisplayGroups>
        <SubmitButton Text="Create account" Icon="fas fa-user-plus" ForceSubmit CenterText/>
    </AfterDisplayGroups>
    <ChildContent>
        <DataAnnotationsValidator />
        <FormRedirect Uri="@ReturnUrl" FallbackUri="/account/register-success" 
            Delay="2000" ForceReload EnforceLocalUri/>
    </ChildContent>
</AutoEditForm>

@code {
    ReCaptchaOptions recaptchaOptions = new ReCaptchaOptions
    {
        Version = "v3",
        Invisible = true,
        SiteKey = "6Ldq7iYU...RMVG...JEQTFOa",
        LanguageCode = "en-US",
        AllowLocalHost = true,
    };

    private readonly RegisterModel registerModel = new RegisterModel();

    /// <summary>
    /// Gets or sets various messages to display based
    /// on the form submission outcome (success, failure).
    /// </summary>
    [Parameter] public MessageDictionary Messages { get; set; }

    /// <summary>
    /// Redirection URL on successful form submission.
    /// </summary>
    [Parameter] public string ReturnUrl { get; set; }

    [FormDisplayDefault(ColumnCssClass = "col-md-6")]
    public class RegisterModel
    {
        [StringLength(50)]
        [FormDisplay(GroupName = "Identification", Icon = "fas fa-user", Description = "Enter your first name")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [FormDisplay(GroupName = "Identification", Icon = "fas fa-user", Description = "Enter your family name")]
        public string LastName { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "A valid email is required")]
        [FormDisplay(GroupName = "Contact", Icon = "fas fa-envelope", Description = "Enter your e-mail address")]
        public string Email { get; set; }
        
        [Phone]
        [FormDisplay(GroupName = "Contact", Description = "Enter your mobile phone number (optional)", Icon = "fas fa-phone-alt")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "A password is required")]
        [FormDisplay(GroupName = "Password", Icon = "fas fa-key", Description = "Enter a new password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        [Required(ErrorMessage = "Confirmation password is required")]
        [FormDisplay(GroupName = "Password", Icon = "fas fa-key", Description = "Confirm the new password")]
        public string ConfirmPassword { get; set; }
        
        [FormDisplay(Type = "hidden", InputName = "__RequestVerificationToken")]
        public string RequestVerificationToken { get; set; }

        // define other required or optional properties for registration
    }
}
```

This inserts an invisible Google reCAPTCHA version 3 in English into the DOM.
The `SiteKey` property must be a valid key for the domain on which the application
is deployed.

When the form is submitted, the reCAPTCHA response and version will be sent to the
server with the keys `g-recaptcha-response` and `g-recaptcha-version` respectively.

Read the Google's Developer documentation on how to create a site key and how to 
validate a reCAPTCHA response on the server.

A complete example on how to integrate reCAPTCHA with a server-side Blazor
ASP.NET MVC Core application will be soon provided.

## Integrated Rich Text Editor

If you need to expose a rich text editor through your model's properties, just
set the `EnableRichText` parameter / property on `AutoEditForm` like so:

```HTML
<AutoEditForm Model="editModel" EnableRichText>
</AutoEditForm>
```

Any property of the model with the `FormDisplay(Tag = "textarea")` custom attribute
will be rendered as a rich text editor. Alternatively, explicitly decorating a 
property with the `FormDisplay(RichText = true)` custom attribute will render it as
a rich text editor as well and there's no need to specify the `EnableRichText` parameter.

Behind the scenes, the Quill Rich Text Editor (https://quilljs.com) is used.
Dependencies (script and stylesheet) are only inserted into the DOM when the above 
conditions are met.

# Drag and drop with dynamic image preview generation and resizing

Using `AutoEditForm` it's very easy to add drag and drop support with simple image
manipulation utilities.

Drag and drop options are configurable both at design and run-time:

- Enable or disable drop event notifications.
- Specify the maximum number of files and total file size allowed to be dropped.
- Display details about dropped files such as file name, size, type, last modified date,
  and image dimensions.
- Option to store only the files without image preview generation and upload them on form
  submission.
- Enable/disable resizing of image previews, set preferred width and height, preserve
  original aspect ratios.
- Individual file and total files reading progress indicators with elapsed time display.

For usage, clone this repo and launch the demo project.

# Introducing AutoEditForm

Automatically generate an EditForm with all appropriate inputs using only a model and
custom attributes. These new form display custom attributes control the way the UI is
presented and reduce the amount of code required to have a beautifully-layed-out and
fully-functional editable form. Adding a tiny amount of CSS you can further style your
form as you desire.

## AutoEditForm quick start (pseudo-code)

For a working sample please checkout the demo application in the project's repository.

### The sample view model:

```C#
using Carfamsoft.Model2View.Annotations;
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
    [FormDisplay(GroupName = "Contact details", Icon = "fas fa-envelope", Type = "email")]
    public string Email { get; set; }

    [StringLength(30)]
    [FormDisplay(GroupName = "Contact details", Icon = "fas fa-phone", Type = "phone")]
    public string PhoneNumber { get; set; }

    [FormDisplay(GroupName = "Please select", Tag = "select", Name = "", Order = 2)]
    public int AgeRange { get; set; }

    [Range(typeof(DayOfWeek), "Monday", "Friday")]
    [FormDisplay(GroupName = "Please select", Tag = "select", Name = "", Order = 1, Prompt = "[Favourite Working Day]", Icon = "fas fa-calendar")]
    public string FavouriteWorkingDay { get; set; }

    [FormDisplay(Type = "radio", Order = 3, Name = "What's your favourite color?")]
    public string FavouriteColor { get; set; }

    [FormDisplay(Order = 4, Name = "Enable two-factor authentication", Description = "Log in with your email and an SMS confirmation")]
    public bool TwoFactorEnabled { get; set; }
}
```

### Understanding the custom attributes

Currently, there are only 3 custom attributes available for decorating the model of the
`AutoFormEdit` component.

1. `DisplayIgnoreAttribute`
2. `FormDisplayAttribute`
3. `FormDisplayDefaultAttribute`

The `FormDisplayDefaultAttribute` class specifies some default properties for the
`FormDisplayAttribute` class. Some of these default properties can still be locally
overridden.

The `DisplayIgnoreAttribute` class, as its name suggests, instructs `AutoInputBase`
to ignore the property when generating corresponding HTML elements. The property's value
is still accessible in the model though and will be posted back during form submission.
As a side-effect, it acts like a hidden input field.

`AutoInputBase` is the base component that handles all inputs/elements code generation.

As you can see, you can still use validation attributes and have the form validated by
a validator such as the `<DataAnnotationsValidator />` component. For instance, the
`RangeAttribute` not only makes sure that the user selects values between the mininum
(Monday) and maximum (Friday) values but `AutoInputBase` also generates the appropriate
enumeration values from Monday to Friday. This is only possible if we specify the
`Tag` to be `select` or `radio`.

```C#
[Range(typeof(DayOfWeek), "Monday", "Friday")]
[FormDisplay(Tag = "select")]
public string FavouriteWorkingDay { get; set; }
```

For now, supported types for the `RangeAttribute` are `string`, `int`, `double`, `enum`,
and `DateTime`.

We can dynamically (at run-time) generate values for this property and others as well.
This is what we'll do for the `AgeRange` and `FavouriteColor` properties.

You'll probably have also noticed the properties `Tag` and `Type` of the
`FormDisplayAttribute`.

```C#
[FormDisplay(Tag = "select")]
public int AgeRange { get; set; }

[FormDisplay(Type = "phone")]
public string PhoneNumber { get; set; }
```

Behaviour of some custom attribute properties of the `FormDisplayAttribute` class:

- `Tag` determines the HTML element to generate, e.g. `input`, `select`, `textarea`...
- `Type` determines the type of an `input` element, e.g. `email`, `number`, `date`...
- `Name` determines the `label` content. If it's an empty (not `null`) string, no label
  is displayed.
- `GroupName` displays similiar or related properties on the same row (e.g.
  `<div class="row">...</div>`).

If you don't specify these properties, `AutoInputBase`, based on the property type
(`string`, `int`, `bool`, `DateTime`, etc.), will determine the most suitable element and
input type to generate.

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

Here, the `OptionsGetter="GetOptions"` attribute-value pair, as previously mentioned,
allows us to generate `select` and `<input type="radio"/>` options at run-time.

### Essential parts of the (pseudo) code-behind file (AutoEditFormUpdate.razor.cs):

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
      // retrieve from the server the model being edited
      Model = await Http.GetFromJsonAsync<AutoUpdateUserModel>("api/account/info");

      // required to submit 'authentication: Bearer...' and other useful request headers
      RequestHeaders = await HeadersProvider.CreateAsync();

      // depending on the requirements, cache the options
      // or request always fresh values from the server
      if (_options == null)
        _options = await Http.GetFromJsonAsync<IEnumerable<SelectOptionList>>("api/account/options");
    }
    catch (AccessTokenNotAvailableException ex)
    {
      ex.Redirect();
    }
    await base.OnInitializedAsync();
  }

  // AutoInputBase needs this function to filter out the
  // appropriate options for a given HTML element being generated
  private IEnumerable<SelectOption> GetOptions(string propertyName)
    => _options?.Where(opt => opt.PropertyName == propertyName).FirstOrDefault()?.Items;
}
```

On the server, in a controller:

```C#
using BlazorFormManager;
using System;
using System.Collections.Generic;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
  [HttpGet("options")]
  public IEnumerable<SelectOptionList> Options()
  {
      // These options could be retrieved from a database or another server-side store;
      // otherwise, there would be no point in making an HTTP request just to retrieve
      // static / hard-coded values. But hey, this is a demo project!
      var ageOptions = new[]
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

      var colorOptions = new[]
      {
        new SelectOption("red", "Red"),
        new SelectOption("green", "Green"),
        new SelectOption("blue", "Blue"),
      };

      return new[]
      {
        // Reference to the AutoUpdateUserModel class is required!
        // nameof(AutoUpdateUserModel.AgeRange) and nameof(AutoUpdateUserModel.FavouriteColor)
        // refer to the model's property names to which the options respectively apply.
        new SelectOptionList(nameof(AutoUpdateUserModel.AgeRange), ageOptions),
        new SelectOptionList(nameof(AutoUpdateUserModel.FavouriteColor), colorOptions),
      };
  }
}
```

### With the help of a bit CSS...

```CSS
.auto-edit-form .form-body .col {
  flex: 0 0 auto;
}

.auto-edit-form .form-body .child-content {
  padding: 1rem 2rem !important;
}

@media (min-width: 768px) {

  .auto-edit-form .form-body {
    display: flex;
    flex-direction: row;
  }

    .auto-edit-form .form-body .col {
      width: 33.33333333%;
    }

    .auto-edit-form .form-body .form-display-group {
      flex: 2;
    }

    .auto-edit-form .form-body .child-content {
      flex: 1;
    }
}

@media (max-width: 767.98px) {
  .auto-edit-form .form-body .col {
    width: 50%;
  }
}

@media (max-width: 480px) {
  .auto-edit-form .form-body .col {
    width: 100%;
  }
}
```

### Produces this form layout!

---

![The output](https://github.com/bigabdoul/BlazorFormManager/blob/assets/images/auto-edit-form-003.jpg?raw=true)

---

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
