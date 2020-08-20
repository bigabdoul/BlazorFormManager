using Microsoft.AspNetCore.Components.Forms;

namespace BlazorFormManager.ComponentModel
{
    /// <summary>
    /// Provides a mecanism to update the <see cref="InputBase{T}.CurrentValue"/> property of an input component.
    /// </summary>
    public interface IAutoInputComponent
    {
        /// <summary>
        /// When implemented by a class, updates the <see cref="InputBase{T}.CurrentValue"/> property of an input component.
        /// </summary>
        /// <param name="value">The value to set.</param>
        void SetCurrentValue(object value);
    }
}