

namespace HrukniNunitTest.ServicesForTesting
{
    public static class TestFilesService
    {
        public static string LoadFile(string filePath)
        {
            if(File.Exists(filePath))
                return File.ReadAllText(filePath);
            else
                return string.Empty;
        }
    }
}
