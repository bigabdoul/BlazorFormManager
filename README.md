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

## Quick start

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

      <FormManager @ref="form" Model="User" FormAction="api/account/register">
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

Check out the [Walkthrough](WALK-THROUGH.md) file for detailed instructions that guide you through
building a set of projects similar to the demo projects found in this repository.

## Engage, contribute, and give feedback

If you want to make improvements to this project, some of the best ways to contribute are
to try things out, file issues, join in design conversations, and make pull-requests.
