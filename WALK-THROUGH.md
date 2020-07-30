# Walkthrough: Integrating BlazorFormManager

This walkthrough requires some familarity with the latest version of `Visual Studio 2019`
or higher. You can also use `Visual Studio Code` or any other code editor that supports
the `dotnet CLI` but they don't apply to this document.

You can download and install for free the latest version of `Visual Studio 2019 Community`
here: https://visualstudio.microsoft.com/downloads

To integrate BlazorFormManager into your own project, follow the steps below. Feel free
to skip Step 1 if you already have an existing project whose layout corresponds to the
`Blazor WebAssembly App` project template created with the latest version of Visual
Studio 2019 or higher.

The instructions guide you through building a set of projects similar to the demo
projects found in this repository.

1. ### Creating a new solution

   Create an ASP.NET Core hosted Blazor WebAssembly App with authentication using
   individual user accounts named `{APP NAMESPACE}`. {APP NAMESPACE} is a placeholder
   that you should replace with the name (without whitespaces) of your project
   (e.g. _BlazorFormManagerWalkthrough_).

2. ### Updating the solution

   As a best-practice, in Visual Studio 2019 Community (or higher), update the solution
   using the `Manage NuGet Packages for Solution...` context menu option: right-click on
   the Solution file item. In the `NuGet - Solution` window, click the `Updates` tab,
   wait until Visual Studio is done refreshing all projects to be updated. Check the
   `Select all packages` option and click the `Update` button on the same line. If
   prompted, read and accept the `License Acceptance` in the dialog. To install and manage
   packages using the dotnet CLI (outside Visual Studio), please visit
   https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-dotnet-cli

3. ### Building the solution

   After all packages have been restored to their respective latest version, make sure
   that the Solution builds without errors (e.g. `dotnet build` from the solution's root
   directory).

4. ### Referencing the BlazorFormManager Razor Class Library (RCL)

   Add a reference to the BlazorFormManager RCL project. Alternatively, you can install
   the binaries through the NuGet Gallery at https://www.nuget.org/packages/BlazorFormManager.

   In Visual Studio's package manager console, type: `Install-Package BlazorFormManager`

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
   into the registration "page". The code is pretty simple and self-explanatory somehow
   due the expressive nature of Razor Components.

   The `<DataAnnotationsValidator />` component ensures client-side validation rules are
   enforced thanks to the data annotation attributes `[Required], [StringLength]` and so
   on. We'll see them soon when defining the models.

   The `@ref` attribute acquires a reference to the rendered DOM element and stores it
   into the `manager` member variable below.

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

   The `HandleSubmitDone` routine is called an event callback handler. Whenever an
   instance of the FormManagerBase class finishes submitting the form, and regardless
   of the outcome (success or failure), the `OnSubmitDone` event callback is triggered,
   hence executing the aforementioned routine.

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

   This component contains the page's title and subtitle headings. It also contains a
   selector allowing the user to control the reporting level in the browser's console
   during debugging. This is useful when you as developer need to figure out problems
   that might occur during development. Of course, this shouldn't be shipped into
   production.

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

   This component will be used both for user registration and editing their personal
   information. It contains the following fields:

   - FirstName (required)
   - LastName (required)
   - Email (required)
   - PhoneNumber (optional)
   - Password (required, registration only)
   - ConfirmPassword (required, registration only)

   A file selector is included at the end to allow users to attach their photo.

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

   #### SubmitButton.razor component:

   This component, as its name suggests, allows submission of the form. It is passed a
   parameter that holds a reference to an instance of the FormManagerBase class. This
   makes it possible to get notified about running state changes and disable/enable
   the button accordingly.

   The `@onclick` event handler, in tandem with the `ForceSubmit` flag, decide whether
   the FormManagerBase should submit the form or whether the browser should do it. In
   some situations, as of this writing and for an unknown reason, Blazor won't allow
   the browser to submit the form even though all validation checks successfully pass.
   We are then required to "force" the form submission using the `BlazorFormManager.js`
   script working behind the scenes.

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

   Right now, you are certainly seing error messages like _`The type or namespace name 'ConsoleLogLevel' could not be found (are you missing a using directive or an assembly reference?).`_ Fix these errors by adding the following to the _\_Imports.razor_ file
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

   Before moving on to the next step, let's do some cleanup:

   - In the **Pages** folder in the root of _{APP NAMESPACE}.Client_, delete the
     **Counter** and **FetchData** components.
   - Under _**Dependencies/Projects**_, remove the reference to the project
     **{APP NAMESPACE}.Shared** and delete that project altogether from the Solution.
   - Now go to the **_{APP NAMESPACE}.Client/Shared_** folder and delete _SurveyPrompt.razor_.
   - In **{APP NAMESPACE}.Shared/Index.razor**, remove the `<SurveyPrompt />` component.
   - Replace its content with the markup below:
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

9. ### Creating the Web API endpoint

   Documentation work in progress...
