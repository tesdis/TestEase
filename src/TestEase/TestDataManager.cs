using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using TestEase.LibrarItemDictionaries;
using TestEase.LibraryItems;

namespace TestEase
{
    public class TestDataManager
    {
        private const string DomainKey = "TestDataManager";
        private readonly string _libraryFolderKey;
        private readonly IList<string> _libraryPaths = new List<string>();
        private IDictionary<string, IItemDictionary> _dictionaries = new Dictionary<string, IItemDictionary>();
        private readonly IDictionary<ItemFileType,string> _extensionMappings = new Dictionary<ItemFileType, string>();

        public TestDataManager(IList<string> pathsToSearch = null)
        {
            if (AppDomain.CurrentDomain.GetData(DomainKey) == null)
            {
                AppDomain.CurrentDomain.SetData(DomainKey, this);
            }

            _libraryFolderKey = ConfigurationManager.AppSettings["libraryFolderName"] ?? "_TestDataLibrary";

            InitItemDictionaries();

            if (pathsToSearch != null)
            {
                foreach (var libraryPath in pathsToSearch)
                {
                    if (!_libraryPaths.Contains(libraryPath)) _libraryPaths.Add(libraryPath);
                }
            }

            var sharedPaths = ConfigurationManager.AppSettings["sharedPaths"];

            if (!string.IsNullOrWhiteSpace(sharedPaths))
            {
                foreach (var s in sharedPaths.Split(','))
                {
                    if (!_libraryPaths.Contains(s)) _libraryPaths.Add(s);
                }
            }

            var libraryFolderPaths = GetTestLibraryFolders();

            foreach (var s in libraryFolderPaths)
            {
                if (!_libraryPaths.Contains(s)) _libraryPaths.Add(s);
            }

            var validExtensions = _dictionaries.Values.Select(itemDic => itemDic.FileExtension).ToList();
            var files = new List<FileInfo>();

            foreach (var path in _libraryPaths)
            {
                if (!Directory.Exists(path)) continue;

                var pathDi = new DirectoryInfo(path);
                var matchedFiles = pathDi.GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(f => validExtensions.Contains(f.Extension)).ToList();

                files.AddRange(matchedFiles);
            }

            SetupLibraryDictionaries(files);
        }

        private void SetupLibraryDictionaries(List<FileInfo> files)
        {
            files.ForEach(f =>
            {
                if (_dictionaries.ContainsKey(f.Extension))
                {
                    _dictionaries[f.Extension].AddFileInfo(f);
                }
            });
        }

        private IEnumerable<string> GetTestLibraryFolders(int depthLimit = 5)
        {
            var projectPath =
                new DirectoryInfo(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) ?? throw new InvalidOperationException());

            var returnPaths = new List<string>();
            var rootPath = projectPath.FullName;

            for (var i = 0; i < depthLimit; i++)
            {
                var currentDi = new DirectoryInfo(rootPath);
                var dirs = Directory.GetDirectories(rootPath);

                returnPaths.AddRange(dirs.Where(dir => dir.Substring(dir.LastIndexOf('\\') + 1) == _libraryFolderKey));

                if (currentDi.Parent == null) break;

                rootPath = currentDi.Parent.FullName;
            }

            return returnPaths;
        }

        private void InitItemDictionaries()
        {
            _dictionaries = new Dictionary<string, IItemDictionary>();

            var sqlDic = new SqlItemDictionary();
            var xmlDic = new XmlItemDictionary();
            var jsonDic = new JsonItemDictionary();

            _dictionaries.Add(sqlDic.FileExtension, sqlDic);
            _dictionaries.Add(xmlDic.FileExtension, xmlDic);
            _dictionaries.Add(jsonDic.FileExtension, jsonDic);
            _extensionMappings.Add(sqlDic.FileType,sqlDic.FileExtension);
            _extensionMappings.Add(xmlDic.FileType, xmlDic.FileExtension);
            _extensionMappings.Add(jsonDic.FileType, jsonDic.FileExtension);
        }

        public SqlItemDictionary Sql => (SqlItemDictionary) _dictionaries[_extensionMappings[ItemFileType.Sql]];
        public XmlItemDictionary Xml => (XmlItemDictionary)_dictionaries[_extensionMappings[ItemFileType.Xml]];
        public JsonItemDictionary Json => (JsonItemDictionary)_dictionaries[_extensionMappings[ItemFileType.Json]];
    }
}



