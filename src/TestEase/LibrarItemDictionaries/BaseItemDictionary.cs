using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestEase.LibraryItems;

namespace TestEase.LibrarItemDictionaries
{
    public class BaseItemDictionary: Dictionary<string, LibraryItem>
    {
        public void AddFileInfo(FileInfo file)
        {
            var libraryFolderName = ConfigurationManager.AppSettings["libraryFolderName"] ?? "_TestDataLibrary";

            var key = file.FullName.Substring(file.FullName.IndexOf(libraryFolderName, StringComparison.Ordinal));
            key = key.Replace($"{libraryFolderName}\\", string.Empty);
            key = key.Replace("\\", ".");
            key = key.Replace(file.Extension, string.Empty);

            Add(key, new LibraryItem(file));
        }
    }
}
