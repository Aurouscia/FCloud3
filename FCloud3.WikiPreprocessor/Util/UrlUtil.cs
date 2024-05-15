using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Util
{
    public static class UrlUtil
    {
        private readonly static List<string> imageExts = new()
        {
            ".png",
            ".jpg",".jpeg",
            ".gif",".webp",
            ".svg",".svgz"
        };
        private readonly static List<string> audioExts = new()
        {
            ".mp3", ".ogg", ".aac"
        };
        private readonly static List<string> videoExts = new()
        {
            ".mp4", ".webm"
        };
        public static bool IsObject(string? fileName)
        {
            return IsImage(fileName) || IsAudio(fileName) || IsVideo(fileName);
        }
        public static bool IsImage(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            string trimmed = fileName.Trim();
            if (trimmed.Length <= 4) return false;
            return imageExts.Any(trimmed.EndsWith);
        }
        public static bool IsAudio(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            string trimmed = fileName.Trim();
            if (trimmed.Length <= 4) return false;
            return audioExts.Any(trimmed.EndsWith);
        }
        public static bool IsVideo(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            string trimmed = fileName.Trim();
            if (trimmed.Length <= 4) return false;
            return videoExts.Any(trimmed.EndsWith);
        }
        public static bool IsUrl(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            string trimmed = fileName.Trim();
            return trimmed.StartsWith("http") || trimmed.StartsWith("/");
        }
    }
}
