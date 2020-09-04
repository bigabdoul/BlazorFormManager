using BlazorFormManager.Drawing;
using System;
using System.IO;

namespace BlazorFormManager.IO
{
    /// <summary>
    /// Represents information about a read input file.
    /// </summary>
    public sealed class InputFileInfo
    {
        private string _name;
        private string _extension;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputFileInfo"/> class.
        /// </summary>
        public InputFileInfo()
        {
        }

        /// <summary>
        /// Gets or sets the last date on which a file was modified.
        /// </summary>
        public DateTime LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the name of a file.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (!string.Equals(_name, value))
                {
                    _name = value;
                    if (!string.IsNullOrWhiteSpace(_name))
                        _extension = Path.GetExtension(_name);
                    else
                        _extension = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the size in bytes of a file.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the type of a file.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the dimensions of this <see cref="InputFileInfo"/> if it's an image.
        /// </summary>
        public ImageSize Dimensions { get; set; }

        /// <summary>
        /// Returns the string representation of the <see cref="Size"/> property value.
        /// </summary>
        public string SizeAsString => FileSizeToString(Size);

        /// <summary>
        /// Returns the string representation of the <see cref="LastModifiedDate"/> property value.
        /// </summary>
        public string LastModifiedDateAsString => $"{LastModifiedDate:dd/MM/yyyy HH:mm:ss}";

        /// <summary>
        /// Determines whether this file is an image.
        /// </summary>
        public bool IsImage => Type.IndexOf("image", StringComparison.OrdinalIgnoreCase) > -1;

        /// <summary>
        /// Determines whether this file is a video.
        /// </summary>
        public bool IsVideo => Type.IndexOf("video", StringComparison.OrdinalIgnoreCase) > -1;

        /// <summary>
        /// Determines whether this file is archived, compressed/zipped.
        /// This is a non-exhaustive test for the most common formats.
        /// </summary>
        public bool IsArchive => 
            HasExtension(".rar, .zip") || 
            HasTypeIndexOf("archive", "zip", "compressed", "x-compress", "x-gtar", "x-tar"
                , "diskimage", "x-iso9660-image", "stuffit", "x-snappy-framed");

        /// <summary>
        /// Determines whether this file has an extension of one of the specified <paramref name="extensions"/>.
        /// </summary>
        /// <param name="extensions">A list of file extensions to compare against.</param>
        /// <returns></returns>
        public bool HasExtension(params string[] extensions)
        {
            foreach (var item in extensions)
            {
                if (string.Equals(_extension, item, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether this file's <see cref="Type"/> 
        /// contains a string in the specified <paramref name="types"/>.
        /// </summary>
        /// <param name="types">A list of string to compare against.</param>
        /// <returns></returns>
        public bool HasTypeIndexOf(params string[] types)
        {
            foreach (var item in types)
            {
                if (Type.IndexOf(item, StringComparison.OrdinalIgnoreCase) > -1)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the string representation of the current <see cref="InputFileInfo"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Name: {Name}; Size: {FileSizeToString(Size)}; Type: {Type}; Last modified: {LastModifiedDate:dd/MM/yyyy HH:mm:ss}";
        }

        /// <summary>
        /// Converts the specified file size to a human-readable string representation.
        /// </summary>
        /// <param name="size">The size of a file.</param>
        /// <returns></returns>
        public static string FileSizeToString(double size)
        {
            if (size == 0) return string.Empty;
            if (size < 1024) return $"{size} B";
            if (size < 1024 * 1024) return $"{size/(double)1024:N2} KB";
            if (size < 1024 * 1024 * 1024) return $"{size / (double)(1024 * 1024):N2} MB";
            return $"{size/(1024 * 1024 * 1024):N2} GB";
        }
    }
}