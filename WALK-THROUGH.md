# Walkthrough: Integrating BlazorFormManager

To integrate BlazorFormManager into your own project, follow the steps below (skip step 1
if you already have an existing project). The instructions guide you through building a
set of projects similar to the demo projects found in this repository.

1. ### Creating a new solution
   Create an ASP.NET Core hosted Blazor WebAssembly App with authentication using
   individual user accounts named `{APP NAMESPACE}`, which is placeholder; replace
   it with the name (without whitespaces) of your project.
2. ### Updating the solution
   As a best-practice, in Visual Studio 2019 Community (or higher), update the solution
   using the `Manage NuGet Packages for Solution...` context menu by right-clicking on
   the Solution file item. In the `NuGet - Solution` window, click the `Updates` tab,
   wait until Visual Studio is done refreshing all projects to be updated. Check the
   `Select all packages` option and click the `Update` button on the same line. If
   prompted, read and accept the `License Acceptance` dialog. To install and manage
   packages using the dotnet CLI (outside Visual Studio), please visit
   https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-dotnet-cli
3. ### Building the solution

   After all packages have been restored to their respective latest version, make sure
   that the Solution builds without errors (e.g. `dotnet build` from the solution's root
   directory).

4. ### Referencing the BlazorFormManager Razor Class Library (RCL)

   Add a reference to the BlazorFormManager RCL project. Alternatively, you can install
   the binaries through the NuGet Gallery at https://www.nuget.org/packages/BlazorFormManager.

   In Visual Studio's package manager console: `Install-Package BlazorFormManager`

   or:

   In your project's root directory, from the dotnet CLI:
   `dotnet add package BlazorFormManager`

5. ### Integrating the BlazorFormManager RCL into your client project

   In your `{APP NAMESPACE}.Client` project, open the index.html file located under the
   **wwwroot** folder and add the following lines:

   1. At the top, right before the closing `</head>` tag:
      - `<link href="_content/BlazorFormManager/css/styles.css" rel="stylesheet" />`,
        and optionally
      - `<link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.14.0/css/all.min.css" />`
   2. At the bottom, right before the closing `</body>` tag:
      - `<script src="_content/BlazorFormManager/js/BlazorFormManager.js"></script>`, and optionally
      - `<script src="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.14.0/js/all.min.js"></script>`

6. ### Creating a user registration component

   In your `{APP NAMESPACE}.Client` project, under the `Pages` folder, create a Razor
   Component named `Register.razor`, for instance. Replace the content of the created
   .razor file with the following C# code:

   ```C#
   @page "/account/register"
   @using BlazorFormManager.Components;
   @using BlazorFormManager.Debugging;
   @using {APP NAMESPACE}.Client.Models;
   @using Microsoft.AspNetCore.Components;
   @using System;
   @using System.Text.Json;
   ```

   followed by this HTML markup:

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

   What we did above is embedding and configuring the `<FormManager />` component
   into the registration "page". The code is pretty simple self-explanatory somehow due
   the expressive nature of Razor Components. The `@ref` attribute acquires a reference
   to the rendered DOM element and stores it into the `manager` member variable below.

   Usually, I separate the code from the UI elements by placing it in a "code-behind"
   class file. But for the sake of simplicity, below the above markup, copy and paste
   the following C# code block:

   ```C#
   @code {
     private static readonly JsonSerializerOptions CaseInsensitiveJson =
      new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
     private FormManager<RegisterUserModel> manager;
     private ConsoleLogLevel LogLevel => manager?.LogLevel ?? ConsoleLogLevel.None;
     private readonly RegisterUserModel Model = new RegisterUserModel();

     [Inject] NavigationManager NavigationManager { get; set; }

     private void HandleSubmitDone(FormManagerSubmitResult result)
     {
         if (result.Succeeded && result.XHR.IsJsonResponse)
         {
             try
             {
                 var model = JsonSerializer.Deserialize<RegisterUserModelResult>(
                     result.XHR.ResponseText, CaseInsensitiveJson);

                 if (!model.Success)
                 {
                     manager.SubmitResult = FormManagerSubmitResult.Failed(result, model.Error);
                 }
                 else if (!string.IsNullOrEmpty(model.Message))
                 {
                     manager.SubmitResult = FormManagerSubmitResult.Success(result, model.Message);
                     if (model.SignedIn)
                     {
                         NavigationManager.NavigateTo("/account/update", true);
                         return;
                     }
                     StateHasChanged();
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

   Remember to replace **{APP NAMESPACE}** with the namespace of your project (e.g.
   FirstBlazorProject). A couple of steps are ahead before getting a functional
   application using the above code.

7. ### Creating the models

   In the **{APP NAMESPACE}.Client** root directory (in the same directory as the
   **Pages** folder), create the **Models** folder then create a C# class file named
   `UserModels.cs`. Copy-paste the following code (and replace {APP NAMESPACE}
   appropriately):

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

   At this point the solution no longer builds. Don't worry, we're going to fix
   everything shortly.

8. ### Creating more components

   In the **Shared** folder (which is in the same as the **Models** folder), create the
   missing components, namely _DemoHeader_, _UserModelInputs_, and _SubmitButton_. These
   components may contain others that we will also define later.

   #### DemoHeader.razor component:

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

   #### UserModelInputs.razor component:

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
       @if (Model is RegisterUserModel registerModel)
       {
           <div class="@ColWidth">
               <div class="form-group">
                   <label class="sr-only" for="@nameof(registerModel.Password)">@PasswordText</label>
                   <InputText type="password" @bind-Value="registerModel.Password" class="form-control" id="@nameof(registerModel.Password)" title="@PasswordText" placeholder="@PasswordText" />
                   <ValidationMessage For="() => registerModel.Password" />
               </div>
               <div class="form-group">
                   <label class="sr-only" for="@nameof(registerModel.ConfirmPassword)">@ConfirmPasswordText</label>
                   <InputText type="password" @bind-Value="registerModel.ConfirmPassword" class="form-control" id="@nameof(registerModel.ConfirmPassword)" title="@ConfirmPasswordText" placeholder="@ConfirmPasswordText" />
                   <ValidationMessage For="() => registerModel.ConfirmPassword" />
               </div>
           </div>
       }
   </div>
   <div class="row">
       <div class="@ColWidth">
           <div class="form-group">
               <div class="custom-file mt-3 mb-3">
                   <input type="file" class="custom-file-input" id="@id" title="@ChoosePhotoText" @ref="fileInputRef">
                   <label class="custom-file-label" for="@id">@ChoosePhotoText</label>
               </div>
           </div>
       </div>
   </div>
   ```

   ```C#
   @code {
       private ElementReference fileInputRef;
       private readonly string id = $"InputFile_{Guid.NewGuid():n}";
       private bool IsRegistration => (Model is RegisterUserModel);
       private string ColWidth => IsRegistration ? "col-6" : "col-12";

       public ElementReference FileRef => fileInputRef;

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

   #### SubmitButton.razor component:

   ```HTML
    @if (Manager != null)
    {
      <div class="form-group">
        <button type="@ButtonType" class="btn btn-primary" @onclick="HandleClick" disabled="@Manager.IsRunning">
            <span><i class="fa fa-save"></i>&nbsp;@Text</span>
        </button>
      </div>
    }
   ```

   ```C#
   @code {
      [Parameter] public FormManagerBase Manager { get; set; }
      [Parameter] public string Text { get; set; }
      [Parameter] public bool ForceSubmit { get; set; }

      private string ButtonType => ForceSubmit ? "button" : "submit";

      private async Task HandleClick()
      {
          if (ForceSubmit) await Manager.SubmitFormAsync();
      }
   }
   ```

   Right now, you are certainly seing error messages like _The type or namespace name
   'ConsoleLogLevel' could not be found (are you missing a using directive or an assembly
   reference?)_. Fix these errors by adding the following to the _\_Imports.razor_ file
   located under the _{APP NAMESPACE}.Client_ project's root directory:

   ```C#
    @using {APP NAMESPACE}.Client.Models
    @using BlazorFormManager
    @using BlazorFormManager.Components
    @using BlazorFormManager.Components.Debugging
    @using BlazorFormManager.Debugging
   ```

   If you followed along, you should be able to build the solution again without errors.
   Try it by running the app (CTRL+F5). However, getting the application to run doesn't
   mean it's bug-free.

   Navigate to `/account/register`, fill in the inputs and then click the **Register me**
   button. You'll get the message: `An error occurred while uploading the form data.`
   To find out more, select **Debugging** in the `console log` dropdown at the top right of
   the page. Below the error message you'll notice that you've got a 404 HTTP error.
   This means that the resource (API endpoint) to which you tried to send the form does
   not exist.

9. ### Building the Web API endpoint
   Documentation work in progress...