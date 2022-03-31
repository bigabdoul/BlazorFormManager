using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Testing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;

namespace AutoInputViewsDemo.Controllers
{
    public class HomeController : Controller
    {
        // These options could be retrieved from a database or another server-side store;
        // otherwise, there would be no point in making an HTTP request just to retrieve
        // static / hard-coded values. But hey, this is a demo project!
        private static readonly SelectOption[] ColorOptions = new[]
        {
            new SelectOption("red", "Red"),
            new SelectOption("yellow", "Yellow"),
            new SelectOption("green", "Green"),
            new SelectOption("blue", "Blue"),
            new SelectOption("green", "Black"),
            new SelectOption("green", "White"),
        };

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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult UpdateUser()
        {
            SetOptionsGetter();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateUser(AutoUpdateUserModel model)
        {
            if (model != null)
            {
                ViewBag.Message = $"User {model.FirstName} {model.LastName} updated successfully!";
            }
            SetOptionsGetter();
            return View(model);
        }

        public ActionResult GetOptions()
        {
            return Json(new[]
            {
                new SelectOptionList(nameof(AutoUpdateUserModel.AgeRange), AgeRangeOptions),
                new SelectOptionList(nameof(AutoUpdateUserModel.FavouriteColor), ColorOptions),
            });
        }

        private void SetOptionsGetter()
        {
            ViewBag.OptionsGetter = (OptionsGetterDelegate)OptionsGetter;
        }

        private static IEnumerable<SelectOption> OptionsGetter(PropertyInfo property)
        {
            if (property.Name == nameof(AutoUpdateUserModel.AgeRange))
                return AgeRangeOptions;
            if (property.Name == nameof(AutoUpdateUserModel.FavouriteColor))
                return ColorOptions;
            return null;
        }
    }
}