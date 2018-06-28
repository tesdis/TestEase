using System.IO;
using TestEase.Helpers;

namespace TestEase.LibraryItems
{
    public class LibraryItem
    {
        private string _libraryItemText;

        public LibraryItem(FileSystemInfo fi)
        {
            ItemFilePath = fi.FullName;
        }

        public string ItemFilePath { get; set; }

        public string LibraryItemText => _libraryItemText ?? (_libraryItemText = FileHelper.GetFileContents(ItemFilePath));
    }
}