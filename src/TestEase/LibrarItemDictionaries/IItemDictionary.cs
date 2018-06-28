using System.Collections.Generic;
using System.IO;
using TestEase.LibraryItems;

namespace TestEase.LibrarItemDictionaries
{
    public interface IItemDictionary
    {
        ItemFileType FileType { get; }
        string FileExtension { get; }
        void AddFileInfo(FileInfo file);
    }
}
