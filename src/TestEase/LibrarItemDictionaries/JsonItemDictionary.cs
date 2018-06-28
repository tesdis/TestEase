using TestEase.LibraryItems;

namespace TestEase.LibrarItemDictionaries
{
    public class JsonItemDictionary : BaseItemDictionary, IItemDictionary
    {
        public ItemFileType FileType => ItemFileType.Json;

        public string FileExtension => ".json";
    }
}
