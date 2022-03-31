using Carfamsoft.Model2View.Mvc;
using Carfamsoft.Model2View.Shared;
using System.Diagnostics;
using System.Globalization;
using Xunit;
using static System.Threading.Thread;

namespace Carfamsoft.Model2View.Testing.Tests
{
    public class NestedTagBuilderTest
    {
        [Fact]
        public void Should_Render_Object_As_Html()
        {
            // arrange
            CurrentThread.CurrentUICulture = new CultureInfo("en");

            var model = GetRegisterBusinessModel();
            var container = new NestedTagBuilder("div");

            // act
            var result = container.RenderAsHtmlString(model);

            Debug.WriteLine(result);

            // assert

            Assert.True(true == result?.Contains("First name"));
        }

        [Fact]
        public void Should_Render_Object_As_Html_French()
        {
            // arrange
            CurrentThread.CurrentUICulture = new CultureInfo("fr");

            var model = GetRegisterBusinessModel();
            var container = new NestedTagBuilder("div");

            // act
            var result = container.RenderAsHtmlString(model);

            Debug.WriteLine(result);

            // assert

            Assert.True(true == result?.Contains("Prénom(s)"));
        }

        [Fact]
        public void Should_Render_Object_Without_Attributes()
        {
            // arrange
            var model = GetRegisterModel();
            var container = new NestedTagBuilder("div");

            // act
            var result = container.RenderAsHtmlString(model, "model");

            Debug.WriteLine(result);

            // assert

            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void Should_RenderAsHtmlString()
        {
            // arrange
            var model = GetRegisterUserModel();
            var container = new NestedTagBuilder("div");

            // act
            var result = container.RenderAsHtmlString(model, "model");

            Debug.WriteLine(result);

            // assert

            Assert.True(true == result?.Contains("abdoul.kaba@example.com"));
        }

        [Fact]
        public void Should_Render_AutoEditForm()
        {
            // arrange
            CurrentThread.CurrentUICulture = new CultureInfo("fr");

            var model = new AutoUpdateUserModel
            {
                FirstName = "Abdoul",
                LastName = "Kaba",
                Contact = new ContactInfo
                {
                    Email = "abdoul.kaba@example.com",
                    PhoneNumber = "+22312345678",
                }
            };
            var formAction = "https://example.com/register";
            var form = new FormAttributes
            {
                Id = "registerUserForm",
                Action = formAction,
                //OptionsGetter = null,
                //DisabledGetter = null,
            };

            var renderOptions = new ControlRenderOptions
            {
                CamelCaseId = true,
                GenerateIdAttribute = true,
                GenerateNameAttribute = true,
                OptionsGetter = property =>
                {
                    if (property.Name == nameof(AutoUpdateUserModel.AgeRange))
                    {
                        return new[]
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
                    }
                    return null;
                }
            };

            var builder = new NestedTagBuilder("div");

            // act

            var result = builder.RenderAutoEditForm(form, model, renderOptions).RenderButton(text: "&nbsp;Save", config: button =>
            {
                button.AddChild(new NestedTagBuilder("i").AddClass("fas fa fa-save"));
            }).GetInnerHtml();

            Debug.WriteLine(result);

            // assert

            Assert.True(true == result?.Contains($"action=\"{formAction}\""));
            Assert.True(true == result.Contains("abdoul.kaba@example.com"));
        }

        static RegisterUserModel GetRegisterUserModel() => new()
        {
            FirstName = "Abdoul",
            LastName = "Kaba",
            Password = "123456",
            ConfirmPassword = "123456",
            Contact = new ContactInfo
            {
                Email = "abdoul.kaba@example.com",
                PhoneNumber = "+22312345678",
            }
        };

        static RegisterModel GetRegisterModel() => new()
        {
            UserName = "bigabdoul",
            FirstName = "Abdoul",
            LastName = "Kaba",
            Gender = "M",
            Email = "abdoul.kaba@example.com",
            CountryId = 223,
            City = "Bamako",
            Address = "Adeken",
            Password = "123456",
            ConfirmPassword = "123456",
            MobilePhone = "+22312345678",
        };

        static RegisterBusinessModel GetRegisterBusinessModel() => new()
        {
            UserName = "bigabdoul",
            FirstName = "Abdoul",
            LastName = "Kaba",
            Gender = "M",
            Email = "abdoul.kaba@example.com",
            CountryId = 223,
            City = "Bamako",
            Address = "Adeken",
            Password = "123456",
            ConfirmPassword = "123456",
            MobilePhone = "+22312345678",
        };
    }
}
