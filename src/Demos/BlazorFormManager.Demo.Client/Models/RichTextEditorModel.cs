using Carfamsoft.Model2View.Annotations;

namespace BlazorFormManager.Demo.Client.Models
{
    public class RichTextEditorModel : AutoUpdateUserModel
    {
        [FormDisplay(Tag = "textarea", GroupName = "Profile", Order = 1)]
        public string About { get; set; }

        [FormDisplay(Tag = "textarea", GroupName = "Profile", Order = 2)]
        public string Experience { get; set; }

        [FormDisplay(Tag = "textarea", GroupName = "Profile", Order = 3)]
        public string Misc { get; set; }
    }
}
