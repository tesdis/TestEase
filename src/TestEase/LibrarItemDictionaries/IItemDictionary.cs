using System.IO;
using TestEase.LibraryItems;

namespace TestEase.LibrarItemDictionaries
{
    public interface IItemDictionary
    {
        ItemFileType GetFileType();
        string GetFileExtension { get; }
        void AddFileInfo(FileInfo file);
    }
}
