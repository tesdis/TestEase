namespace TestEase.LibraryItemDictionaries
{
    using System.IO;

    using TestEase.LibraryItems;

    /// <summary>
    /// Maintains a collection of library items
    /// </summary>
    public interface IItemDictionary
    {
        /// <summary>
        /// Gets the file extension, including the period
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Gets the <see cref="ItemFileType"/>
        /// </summary>
        ItemFileType FileType { get; }

        /// <summary>
        /// FileInfo that will be used to set backing properties to library item
        /// </summary>
        /// <param name="file">
        /// <see cref="FileInfo"/> to be used
        /// </param>
        void AddFileInfo(FileInfo file);
    }
}