namespace TestEase.LibraryItemDictionaries
{
    using System;
    using System.Collections.Generic;

    using LibraryItems;

    /// <inheritdoc />
    /// <summary>
    /// Collection of item dictionaries and helper for registering
    /// </summary>
    public class ItemDictionaryCollection : Dictionary<string, IItemDictionary>
    {
        /// <summary>
        /// Maps <see cref="ItemFileType" /> to file extensions
        /// </summary>
        public readonly IDictionary<ItemFileType, string> ExtensionMappings = new Dictionary<ItemFileType, string>();

        /// <summary>
        /// Registers a dictionary type and maps file extensions to that dictionary
        /// </summary>
        /// <param name="dictionary">
        ///  An instance of the dictionary to set
        /// </param>
        /// <param name="overrideRegistration">
        /// Whether or not to overwrite an existing dictionary with the same file extension key
        /// </param>
        /// <typeparam name="T">
        /// <see cref="IItemDictionary"/> Dictionary implementation to register
        /// </typeparam>
        /// <exception cref="ArgumentException">Dictionary already registered and override is disabled</exception>
        public void Register<T>(
            IItemDictionary dictionary = null,
            bool overrideRegistration = false)
            where T : IItemDictionary, new()
        {
            var instance = dictionary ?? new T();

            if (ContainsKey(instance.FileExtension))
            {
                if (!overrideRegistration)
                {
                    throw new ArgumentException($"Dictionary already registered and override is disabled. Dictionary type: {instance.FileExtension}");
                }

                this[instance.FileExtension] = instance;
            }
            else
            {
                Add(instance.FileExtension, instance);

                if (!ExtensionMappings.ContainsKey(instance.FileType))
                {
                    ExtensionMappings.Add(instance.FileType, instance.FileExtension);
                }
            }
        }
    }
}