namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Provides enumerated method names corresponding to JavaScript's FileReader API.
    /// </summary>
    public enum FileReaderMethod
    {
        /// <summary>
        /// Nothing will be done.
        /// </summary>
        None = 0,

        /// <summary>
        /// In JavaScript:
        /// Starts reading the contents of the specified Blob, once finished, the 
        /// result attribute contains an ArrayBuffer representing the file's data.
        /// </summary>
        ReadAsArrayBuffer = 1,

        /// <summary>
        /// In JavaScript:
        /// Starts reading the contents of the specified Blob, once finished, the 
        /// result attribute contains the raw binary data from the file as a string.
        /// </summary>
        ReadAsBinaryString = 2,

        /// <summary>
        /// In JavaScript:
        /// Starts reading the contents of the specified Blob, once finished, the 
        /// result attribute contains a data: URL representing the file's data.
        /// </summary>
        ReadAsDataURL = 3,

        /// <summary>
        /// In JavaScript:
        /// Starts reading the contents of the specified Blob, once finished, the 
        /// result attribute contains the contents of the file as a text string. 
        /// An optional encoding name can be specified.
        /// </summary>
        ReadAsText = 4,

        /// <summary>
        /// In JavaScript:
        /// Enumerates only the file list without reading their content.
        /// </summary>
        Enumerate = 5,
    }
}
