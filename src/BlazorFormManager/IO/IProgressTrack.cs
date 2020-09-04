namespace BlazorFormManager.IO
{
    /// <summary>
    /// When implemented by class, represents an object that keeps track of an ongoing operation.
    /// </summary>
    public interface IProgressTrack
    {
        /// <summary>
        /// Gets or sets the current value of the progress.
        /// </summary>
        long Current { get; set; }

        /// <summary>
        /// Gets or sets the total amount to completion.
        /// </summary>
        long Total { get; set; }

        /// <summary>
        /// Gets the progress percentage.
        /// </summary>
        int ProgressPercentage { get; }
    }
}