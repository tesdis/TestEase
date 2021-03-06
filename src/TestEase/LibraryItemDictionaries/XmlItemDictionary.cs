﻿namespace TestEase.LibraryItemDictionaries
{
    using LibraryItems;

    /// <inheritdoc />
    /// <summary>
    /// The xml item dictionary.
    /// </summary>
    public class XmlItemDictionary : BaseItemDictionary
    {
        /// <inheritdoc />
        public override string FileExtension => ".xml";

        /// <inheritdoc />
        public override ItemFileType FileType => ItemFileType.Xml;
    }
}