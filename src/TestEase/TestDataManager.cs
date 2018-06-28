namespace TestEase
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using TestEase.LibraryItemDictionaries;
    using TestEase.LibraryItems;

    /// <summary>
    /// Coordinates the setup and retrieval of item dictionaries
    /// </summary>
    public class TestDataManager
    {
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
        /// Dictionaries that are configured
        /// </summary>
        private readonly ItemDictionaryCollection dictionaries = new ItemDictionaryCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataManager"/> class.
        /// </summary>
        /// <param name="pathsToSearch">
        /// The paths to search.
        /// </param>
        /// <exception cref="AppDomainUnloadedException">The operation is attempted on an unloaded application domain. </exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission. </exception>
        /// <exception cref="DirectoryNotFoundException">The path is invalid (for example, it is on an unmapped drive). </exception>
        public TestDataManager(IList<string> pathsToSearch = null)
        {
            if (AppDomain.CurrentDomain.GetData(DomainKey) == null)
            {
                AppDomain.CurrentDomain.SetData(DomainKey, this);
            }

            this.libraryFolderKey = ConfigurationManager.AppSettings["libraryFolderName"] ?? "_TestDataLibrary";

            this.InitItemDictionaries();

            if (pathsToSearch != null)
            {
                foreach (var libraryPath in pathsToSearch)
                {
                    if (!this.libraryPaths.Contains(libraryPath))
                    {
                        this.libraryPaths.Add(libraryPath);
                    }
                }
            }

            var sharedPaths = ConfigurationManager.AppSettings["sharedPaths"];

            if (!string.IsNullOrWhiteSpace(sharedPaths))
            {
                foreach (var s in sharedPaths.Split(','))
                {
                    if (!this.libraryPaths.Contains(s))
                    {
                        this.libraryPaths.Add(s);
                    }
                }
            }

            var libraryFolderPaths = this.GetTestLibraryFolders();

            foreach (var s in libraryFolderPaths)
            {
                if (!this.libraryPaths.Contains(s))
                {
                    this.libraryPaths.Add(s);
                }
            }

            var validExtensions = this.dictionaries.Values.Select(itemDic => itemDic.FileExtension).ToList();
            var files = new List<FileInfo>();

            foreach (var path in this.libraryPaths)
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

            this.SetupLibraryDictionaries(files);
        }

        /// <summary>
        /// Json item dictionary helper
        /// </summary>
        public JsonItemDictionary Json =>
            (JsonItemDictionary)this.dictionaries[this.dictionaries.ExtensionMappings[ItemFileType.Json]];

        /// <summary>
        /// Sql item dictionary helper
        /// </summary>
        public SqlItemDictionary Sql =>
            (SqlItemDictionary)this.dictionaries[this.dictionaries.ExtensionMappings[ItemFileType.Sql]];

        /// <summary>
        /// Text item dictionary helper
        /// </summary>
        public TextItemDictionary Text =>
            (TextItemDictionary)this.dictionaries[this.dictionaries.ExtensionMappings[ItemFileType.Text]];

        /// <summary>
        /// Xml item dictionary helper
        /// </summary>
        public XmlItemDictionary Xml =>
            (XmlItemDictionary)this.dictionaries[this.dictionaries.ExtensionMappings[ItemFileType.Xml]];

        /// <summary>
        /// Searches for library item folders. Searches up to five levels by default
        /// </summary>
        /// <param name="depthLimit">
        /// Optional depth limit to search up from the working folder
        /// </param>
        /// <returns>
        /// Collection of string library paths
        /// </returns>
        private IEnumerable<string> GetTestLibraryFolders(int depthLimit = 5)
        {
            var projectPath = new DirectoryInfo(
                Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)
                ?? throw new InvalidOperationException());

            var returnPaths = new List<string>();
            var rootPath = projectPath.FullName;

            for (var i = 0; i < depthLimit; i++)
            {
                var currentDi = new DirectoryInfo(rootPath);
                var dirs = Directory.GetDirectories(rootPath);

                returnPaths.AddRange(
                    dirs.Where(dir => dir.Substring(dir.LastIndexOf('\\') + 1) == this.libraryFolderKey));

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
            this.dictionaries.Register<SqlItemDictionary>();
            this.dictionaries.Register<XmlItemDictionary>();
            this.dictionaries.Register<JsonItemDictionary>();
            this.dictionaries.Register<TextItemDictionary>();
        }

        /// <summary>
        /// Adds the provided files to the appropriate dictionary
        /// </summary>
        /// <param name="files">
        /// Files to be filtered and added to dictionaries
        /// </param>
        private void SetupLibraryDictionaries(List<FileInfo> files)
        {
            files.ForEach(
                f =>
                    {
                        if (this.dictionaries.ContainsKey(f.Extension))
                        {
                            this.dictionaries[f.Extension].AddFileInfo(f);
                        }
                    });
        }
    }
}