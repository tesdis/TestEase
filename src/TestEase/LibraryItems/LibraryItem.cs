namespace TestEase.LibraryItems
{
    // ReSharper disable ExceptionNotThrown
    using System;
    using System.IO;
    using System.Security;

    using TestEase.Helpers;

    /// <summary>
    /// Wrapper for each library item
    /// </summary>
    public class LibraryItem
    {
        /// <summary>
        /// Text contents of library item
        /// </summary>
        private string libraryItemText;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryItem"/> class.
        /// </summary>
        /// <param name="fileInfo">
        /// <see cref="FileSystemInfo"/> to be used for the library item
        /// </param>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        public LibraryItem(FileSystemInfo fileInfo)
        {
            this.ItemFilePath = fileInfo.FullName;
        }

        /// <summary>
        /// Text contents of library item
        /// </summary>
        /// <exception cref="FileNotFoundException">The file cannot be found, such as when <see cref="FileMode"/> is FileMode.Truncate or FileMode.Open, and the file specified by path does not exist. The file must already exist in these modes. </exception>
        /// <exception cref="IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by path already exists, occurred. -or-The stream has been closed.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
        /// <exception cref="UnauthorizedAccessException">The requested is not permitted by the operating system for the specified path, such as when <see cref="FileAccess"/> is Write or ReadWrite and the file or directory is set for read-only access. </exception>
        /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
        public string LibraryItemText =>
            this.libraryItemText ?? (this.libraryItemText = FileHelper.GetFileContents(this.ItemFilePath));

        /// <summary>
        /// Gets or sets the item file path.
        /// </summary>
        public string ItemFilePath { get; set; }
    }
}