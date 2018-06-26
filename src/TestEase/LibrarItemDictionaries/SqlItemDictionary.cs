using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using TestEase.LibraryItems;

namespace TestEase.LibrarItemDictionaries
{
    public class SqlItemDictionary : Dictionary<string, LibraryItem>, IItemDictionary
    {
        public Dictionary<string, StringBuilder> QueuedSql;

        public SqlItemDictionary()
        {

        }

        public ItemFileType GetFileType()
        {
            return ItemFileType.Sql;
        }

        public string GetFileExtension => ".sql";

        public void AddFileInfo( FileInfo file)
        {
            var libraryFolderName = ConfigurationManager.AppSettings["libraryFolderName"] ?? "_TestDataLibrary";

            var key = file.FullName.Substring(file.FullName.IndexOf(libraryFolderName, StringComparison.Ordinal));
            key = key.Replace($"{libraryFolderName}\\",string.Empty);
            key = key.Replace("\\", ".");
            key = key.Replace(file.Extension, string.Empty);

            this.Add(key, new LibraryItem(file));
        }

        public void SetupConnections(IDictionary<string, string> connections)
        {

        }

        public void QueueSql(IDictionary<string, object> ReplacementValues = null)
        {

        }
    }
}