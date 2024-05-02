namespace BlazorFormManager.Components.Web
{
	/// <summary>
	/// Provides enumerated values for file info table columns
	/// </summary>
	[System.Flags]
    public enum FileInfoTableColumns
	{
		/// <summary>
		/// No column.
		/// </summary>
		None = 0,

		/// <summary>
		/// Show row number column.
		/// </summary>
		RowNumber = 1,

		/// <summary>
		/// Show file name column.
		/// </summary>
		Name = 2,

		/// <summary>
		/// Show file size column.
		/// </summary>
		Size = 4,

		/// <summary>
		/// Show image dimensions column.
		/// </summary>
		Dimensions = 8,

		/// <summary>
		/// Show file type column.
		/// </summary>
		Type = 16,

		/// <summary>
		/// Show file last modified date column.
		/// </summary>
		LastModifiedDate = 32,

		/// <summary>
		/// Show the image preview column
		/// </summary>
		Image = 64,

		/// <summary>
		/// Show all columns.
		/// </summary>
		All = RowNumber | Name | Size | Dimensions | Type | LastModifiedDate | Image,
	}
}
