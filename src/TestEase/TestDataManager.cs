using System;
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

            var validExtensions = _dictionaries.Values.Select(itemDic => itemDic.GetFileExtension).ToList();
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

            _dictionaries.Add(sqlDic.GetFileExtension, sqlDic);
        }

        public SqlItemDictionary Sql { get; }
    }
}
