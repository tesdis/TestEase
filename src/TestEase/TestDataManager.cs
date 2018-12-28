namespace TestEase
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using LibraryItemDictionaries;
    using LibraryItems;

    /// <summary>
    /// Coordinates the setup and retrieval of item dictionaries
    /// </summary>
    public class TestDataManager
    {
        /// <summary>
        /// Dictionaries that are configured
        /// </summary>
        public readonly ItemDictionaryCollection Dictionaries = new ItemDictionaryCollection();

        /// <summary>
        /// Domain key to be used for global library
        /// </summary>
        private const string DomainKey = "TestDataManager";

        /// <summary>
        /// Name of the folder the signifies a test data library folder
        /// </summary>
        private readonly string libraryFolderKey;

        /// <summary>
        /// Paths of known libraries to find items
        /// </summary>
        private readonly IList<string> libraryPaths = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataManager"/> class.
        /// </summary>
        /// <exception cref="AppDomainUnloadedException">The operation is attempted on an unloaded application domain. </exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="DirectoryNotFoundException">The path is invalid (for example, it is on an unmapped drive). </exception>
        public TestDataManager()
        {
            if (AppDomain.CurrentDomain.GetData(DomainKey) == null)
            {
                AppDomain.CurrentDomain.SetData(DomainKey, this);
            }

            libraryFolderKey = ConfigurationManager.AppSettings["libraryFolderName"] ?? "_TestDataLibrary";

            InitItemDictionaries();

            var sharedPaths = ConfigurationManager.AppSettings["sharedPaths"];

            if (!string.IsNullOrWhiteSpace(sharedPaths))
            {
                foreach (var s in sharedPaths.Split(','))
                {
                    if (!libraryPaths.Contains(s))
                    {
                        libraryPaths.Add(s);
                    }
                }
            }

            var libraryFolderPaths = GetTestLibraryFolders();

            foreach (var s in libraryFolderPaths)
            {
                if (!libraryPaths.Contains(s))
                {
                    libraryPaths.Add(s);
                }
            }

            var validExtensions = Dictionaries.Values.Select(itemDic => itemDic.FileExtension).ToList();
            var files = new List<FileInfo>();

            foreach (var path in libraryPaths)
            {
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var pathDi = new DirectoryInfo(path);
                var matchedFiles = pathDi.GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(f => validExtensions.Contains(f.Extension)).ToList();

                files.AddRange(matchedFiles);
            }

            SetupLibraryDictionaries(files);
        }

        /// <summary>
        /// Json item dictionary helper
        /// </summary>
        public JsonItemDictionary Json =>
            (JsonItemDictionary)Dictionaries[Dictionaries.ExtensionMappings[ItemFileType.Json]];

        /// <summary>
        /// Sql item dictionary helper
        /// </summary>
        public SqlItemDictionary Sql => (SqlItemDictionary)Dictionaries[Dictionaries.ExtensionMappings[ItemFileType.Sql]];

        /// <summary>
        /// Text item dictionary helper
        /// </summary>
        public TextItemDictionary Text =>
            (TextItemDictionary)Dictionaries[Dictionaries.ExtensionMappings[ItemFileType.Text]];

        /// <summary>
        /// Xml item dictionary helper
        /// </summary>
        public XmlItemDictionary Xml =>
            (XmlItemDictionary)Dictionaries[Dictionaries.ExtensionMappings[ItemFileType.Xml]];

        /// <summary>
        /// Searches for library item folders. Searches up to five levels by default
        /// </summary>
        /// <returns>
        /// Collection of string library paths
        /// </returns>
        private IEnumerable<string> GetTestLibraryFolders()
        {
            var projectPath = new DirectoryInfo(
                Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ?? throw new InvalidOperationException());

            var returnPaths = new List<string>();
            var rootPath = projectPath.FullName;

            while (true)
            {
                var currentDi = new DirectoryInfo(rootPath);
                var dirs = Directory.GetDirectories(rootPath);

                returnPaths.AddRange(
                    dirs.Where(dir => dir.Substring(dir.LastIndexOf('\\') + 1) == libraryFolderKey));

                if (currentDi.Parent == null)
                {
                    break;
                }

                rootPath = currentDi.Parent.FullName;
            }

            return returnPaths;
        }

        /// <summary>
        /// Registers default dictionaries
        /// </summary>
        private void InitItemDictionaries()
        {
            Dictionaries.Register<SqlItemDictionary>();
            Dictionaries.Register<XmlItemDictionary>();
            Dictionaries.Register<JsonItemDictionary>();
            Dictionaries.Register<TextItemDictionary>();
        }

        /// <summary>
        /// Adds the provided files to the appropriate dictionary
        /// </summary>
        /// <param name="files">
        /// Files to be filtered and added to dictionaries
        /// </param>
        private void SetupLibraryDictionaries(IEnumerable<FileInfo> files)
        {
            foreach (var f in files)
            {
                if (Dictionaries.ContainsKey(f.Extension))
                {
                    Dictionaries[f.Extension].AddFileInfo(f);
                }
            }
        }
    }
}