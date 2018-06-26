using System.IO;
using TestEase.Helpers;

namespace TestEase.LibraryItems
{
    public class LibraryItem
    {
        private string libraryItemText = null;

        public LibraryItem(FileSystemInfo fi)
        {
            ItemFilePath = fi.FullName;
        }

        public string ItemFilePath { get; set; }

        public string LibraryItemText => libraryItemText ?? (libraryItemText = FileHelper.GetFileContents(ItemFilePath));
    }
}