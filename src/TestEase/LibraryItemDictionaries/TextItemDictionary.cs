namespace TestEase.LibraryItemDictionaries
{
    using TestEase.LibraryItems;

    /// <summary>
    /// The text item dictionary.
    /// </summary>
    public class TextItemDictionary : BaseItemDictionary
    {
        /// <inheritdoc />
        public override string FileExtension => ".txt";

        /// <inheritdoc />
        public override ItemFileType FileType => ItemFileType.Text;
    }
}