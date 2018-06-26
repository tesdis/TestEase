using System.IO;

namespace TestEase.Helpers
{
    public static class FileHelper
    {
        public static string GetFileContents(string path)
        {
            if (!File.Exists(path)) return null;

            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(fs);

            return reader.ReadToEnd();

        }
    }
}
