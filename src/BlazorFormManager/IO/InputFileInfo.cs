using BlazorFormManager.Drawing;
using System;
using System.IO;

namespace BlazorFormManager.IO
{
    /// <summary>
    /// Represents information about a read input file.
    /// </summary>
    public sealed class InputFileInfo : IComparable<InputFileInfo>
    {
        private string? _name;
        private string? _extension;

        private const int KB = 1024;
        private const int MB = KB * 1024;
        private const int GB = MB * 1024;

        /// <summary>
        /// The hashcode for the current instance.
        /// </summary>
        public readonly string HashCode = $"{Guid.NewGuid().GetHashCode():x}";

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
        public string? Name
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
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the dimensions of this <see cref="InputFileInfo"/> if it's an image.
        /// </summary>
        public ImageSize? Dimensions { get; set; }

        /// <summary>
        /// Gets or set the object URL.
        /// </summary>
		public string? ObjectUrl { get; set; }

		/// <summary>
		/// Gets or sets the use for the file referenced by this <see cref="InputFileInfo"/>.
		/// </summary>
		public string? Purpose { get; set; }

        /// <summary>
        /// Gets or sets an abitrary index.
        /// </summary>
		public int Index { get; set; }

        /// <summary>
        /// Indicates whether the file has been rejected.
        /// </summary>
		public bool Rejected { get; set; }

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
        public bool IsImage => Type?.IndexOf("image", StringComparison.OrdinalIgnoreCase) > -1;

        /// <summary>
        /// Determines whether this file is a video.
        /// </summary>
        public bool IsVideo => Type?.IndexOf("video", StringComparison.OrdinalIgnoreCase) > -1;

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
                if (Type?.IndexOf(item, StringComparison.OrdinalIgnoreCase) > -1)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the actual file size is greater than the specified size in megabytes.
        /// </summary>
        /// <param name="sizeInMegabytes">The file size in megabytes.</param>
        /// <returns></returns>
        public bool BiggerThan(long sizeInMegabytes) => Size >= sizeInMegabytes * MB;

        /// <summary>
        /// Returns the string representation of the current <see cref="InputFileInfo"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Name: {Name}; Size: {FileSizeToString(Size)}; Type: {Type}; Last modified: {LastModifiedDate:dd/MM/yyyy HH:mm:ss}";
        }

        /// <summary>
        /// Compares the <see cref="HashCode"/> of this instance with <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The other <see cref="InputFileInfo"/> to compare to.</param>
        /// <returns></returns>
		public int CompareTo(InputFileInfo? other)
		{
            return HashCode.CompareTo($"{other?.HashCode}");
		}

        /// <summary>
        /// Determines this instance and another specified object have the same value.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>
        ///  true if the value of the <paramref name="obj"/> parameter is the same as the value of this
        ///  instance; otherwise, false. If <paramref name="obj"/> is null, the method returns false.
        /// </returns>
		public override bool Equals(object? obj)
		{
			if (obj is not InputFileInfo other) return false;
			return HashCode.Equals(other.HashCode);
		}

        /// <summary>
        /// Returns the hash code for this <see cref="InputFileInfo"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return HashCode.GetHashCode();
		}

		/// <summary>
		/// Converts the specified file size to a human-readable string representation.
		/// </summary>
		/// <param name="size">The size of a file.</param>
		/// <returns></returns>
		public static string FileSizeToString(double size)
        {
            if (size == 0) return string.Empty;
            if (size < KB) return $"{size} B";
            if (size < MB) return $"{size / KB:N1} KB";
            if (size < GB) return $"{size / MB:N1} MB";
            return $"{size / GB:N2} GB";
        }
	}
}