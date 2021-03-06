﻿namespace TestEase.Helpers
{
    using System;
    using System.IO;
    using System.Security;

    /// <summary>
    /// Static helper class for working with files in the library
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Helper to read the contents of a file
        /// </summary>
        /// <param name="path">
        /// Path of the file to read.
        /// </param>
        /// <returns>
        /// The <see cref="string"/> contents of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path" /> is null. </exception>
        /// <exception cref="FileNotFoundException">The file cannot be found, such as when <see cref="FileMode"/> is FileMode.Truncate or FileMode.Open, and the file specified by <paramref name="path" /> does not exist. The file must already exist in these modes. </exception>
        /// <exception cref="IOException">An I/O error, such as specifying FileMode.CreateNew when the file specified by <paramref name="path" /> already exists, occurred. -or-The stream has been closed.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive. </exception>
        /// <exception cref="UnauthorizedAccessException">The requested is not permitted by the operating system for the specified <paramref name="path" />, such as when <see cref="FileAccess"/> is Write or ReadWrite and the file or directory is set for read-only access. </exception>
        /// <exception cref="OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters. -or-<paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="path" /> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <see cref="System.IO.FileMode"/> contains an invalid value. </exception>
        public static string GetFileContents(string path)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(fs);

            return reader.ReadToEnd();
        }
    }
}