using System;

namespace BlazorFormManager
{
    /// <summary>
    /// Contains data related to the <see cref="Components.Forms.FormManagerBase.OnModelRequested"/> event.
    /// </summary>
    public class ModelRequestedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelRequestedEventArgs"/> class.
        /// </summary>
        public ModelRequestedEventArgs()
        {
        }

        /// <summary>
        /// Gets or sets the model to be submitted.
        /// </summary>
        public object? Model { get; set; }
    }
}
