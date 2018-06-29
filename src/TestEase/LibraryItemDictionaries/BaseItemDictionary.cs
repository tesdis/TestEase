namespace TestEase.LibraryItemDictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Security;

    using TestEase.LibraryItems;

    /// <summary>
    /// Wrapper for item dictionaries containing common setup logic.
    /// </summary>
    public abstract class BaseItemDictionary : Dictionary<string, LibraryItem>, IItemDictionary
    {
        /// <inheritdoc />
        public abstract string FileExtension { get; }

        /// <inheritdoc />
        public abstract ItemFileType FileType { get; }

        /// <summary>
        /// Using the file path, the item is keyed based on folder directory (dot notation starting from the root test data library folder)
        /// </summary>
        /// <param name="file">
        /// The <see cref="FileInfo" /> file info object to set base information about the item/>
        /// </param>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        public void AddFileInfo(FileInfo file)
        {
            var libraryFolderName = ConfigurationManager.AppSettings["libraryFolderName"] ?? "_TestDataLibrary";

            var key = file.FullName.Substring(file.FullName.IndexOf(libraryFolderName, StringComparison.Ordinal));
            key = key.Replace($"{libraryFolderName}\\", string.Empty);
            key = key.Replace("\\", ".");
            key = key.Replace(file.Extension, string.Empty);

            if (!this.ContainsKey(key))
            {
                this.Add(key, new LibraryItem(file));
            }
            else
            {
                // TODO Should log that we ignore items already added
            }
        }

        /// <summary>
        /// Returns the item text of a library item
        /// </summary>
        /// <param name="key">
        /// Library item key
        /// </param>
        /// <returns>
        /// The library item text as a <see cref="string"/> .
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Will be thrown if the file was not discovered and is not stored in the dictionary
        /// </exception>
        public string Get(string key)
        {
            if (this.ContainsKey(key))
            {
                return this[key].LibraryItemText;
            }

            throw new ArgumentException($"File does not exist in the library for key {key}");
        }
    }
}