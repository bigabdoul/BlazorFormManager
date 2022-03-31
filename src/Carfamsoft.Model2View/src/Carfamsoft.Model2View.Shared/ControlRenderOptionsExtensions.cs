namespace Carfamsoft.Model2View.Shared
{
    /// <summary>
    /// Provides extension methods for instances of the <see cref="ControlRenderOptions"/> class.
    /// </summary>
    public static class ControlRenderOptionsExtensions
    {
        /// <summary>
        /// Sets the value of the <see cref="ControlRenderOptions.OptionsGetter"/> property.
        /// </summary>
        /// <param name="instance">An initialized instance of the <see cref="ControlRenderOptions"/> class.</param>
        /// <param name="optionsGetter">The value to set.</param>
        /// <returns></returns>
        public static ControlRenderOptions SetOptionsGetter(this ControlRenderOptions instance, OptionsGetterDelegate optionsGetter)
        {
            instance.OptionsGetter = optionsGetter;
            return instance;
        }
    }
}
