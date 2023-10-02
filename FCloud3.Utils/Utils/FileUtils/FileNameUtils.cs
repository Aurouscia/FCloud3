namespace FCloud3.Utils.Utils.FileUtils
{
    public static class FileNameUtils
    {
        public static string WithoutSuffix(this string fileName)
        {
            int lastDot = fileName.LastIndexOf('.');
            if (lastDot != -1)
            {
                return fileName.Substring(0,lastDot);
            }
            return fileName;
        } 
        public static string GetSuffix(this string fileName)
        {
            int lastDot = fileName.LastIndexOf('.');
            if (lastDot != -1)
            {
                return fileName.Substring(lastDot + 1);
            }
            return string.Empty;
        }
        public static string RandRenameFor(string fileName)
        {
            string suffix = fileName.GetSuffix();
            string name = Path.GetRandomFileName().WithoutSuffix();
            return $"{name}.{suffix}";
        }
        public static readonly string[] ImgsSuffixes = new string[]
        {
            "jpg","jpeg","png","bmp","gif"
        };
        public static bool IsImg(string fileName)
        {
            string suffix = fileName.GetSuffix();
            return ImgsSuffixes.Contains(suffix);
        }
    }
}
