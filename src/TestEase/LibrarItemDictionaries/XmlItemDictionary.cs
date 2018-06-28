using TestEase.LibraryItems;

namespace TestEase.LibrarItemDictionaries
{
    public class XmlItemDictionary : BaseItemDictionary, IItemDictionary
    {
        public ItemFileType FileType => ItemFileType.Xml;

        public string FileExtension => ".xml";
    }
}
