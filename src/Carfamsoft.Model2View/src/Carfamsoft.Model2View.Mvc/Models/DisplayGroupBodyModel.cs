using Carfamsoft.Model2View.Annotations;
using Carfamsoft.Model2View.Shared;

namespace Carfamsoft.Model2View.Mvc.Models
{
    public class DisplayAutoEditModel
    {
        public object ViewModel { get; set; }
        public System.Type ViewModelType { get; set; }
        public ControlRenderOptions RenderOptions { get; set; }
    }

    public class DisplayAutoEditFormModel : DisplayAutoEditModel
    {
        public string FormId { get; set; }
        public string FormAction { get; set; }
        public string FormMethod { get; set; } = "post";
        public string EncodingType { get; set; } = "multipart/form-data";
        public object FormHeader { get; set; }
        public object BeforeDisplayGroups { get; set; }
        public object AfterDisplayGroups { get; set; }
        public object ChildContent { get; set; }
        public object FormFooter { get; set; }
    }

    public class DisplayModelBase
    {
        public string InputId { get; set; }
        public string InputName { get; set; }
        public object ViewModel { get; set; }
        public string PropertyNavigationPath { get; set; }
        public ControlRenderOptions RenderOptions { get; set; }
    }

    public class DisplayGroupModel : DisplayModelBase
    {
        public FormDisplayGroupMetadata Metadata { get; set; }
    }

    public class DisplayGroupBodyModel : DisplayModelBase
    {
        public AutoInputMetadata[] Items { get; set; }
        public string CssClass { get; set; }
    }

    public class DisplayAutoInputModel : DisplayModelBase
    {
        public AutoInputMetadata Metadata { get; set; }
    }

    public class DisplayInputGroupEntryModel
    {
        public string Icon { get; set; }
        public string PrependText { get; set; }
        public object ChildContent { get; set; }
        public object ValidationContent { get; set; }
    }
}
