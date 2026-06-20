using System.Security.Cryptography;
using System.Text;
using FCloud3.Services.Ai;

namespace FCloud3.App.Services.Utils
{
    /// <summary>
    /// 使用 AES-256-CBC 加密 AI 实例 API Key。
    /// 密钥从配置项 Ai:ApiKeyEncryptionKey 读取，任意长度字符串会经 SHA256 派生为 32 字节密钥。
    /// 密文格式：enc:Base64(IV):Base64(CipherText)
    /// </summary>
    public class AiApiKeyEncryption(IConfiguration config) : IAiApiKeyEncryption
    {
        private const string Prefix = "enc:";
        private readonly byte[] _key = DeriveKey(config["Ai:ApiKeyEncryptionKey"]);

        private static byte[] DeriveKey(string? keyString)
        {
            if (string.IsNullOrWhiteSpace(keyString))
                throw new Exception("未找到配置项 Ai:ApiKeyEncryptionKey");
            return SHA256.HashData(Encoding.UTF8.GetBytes(keyString));
        }

        public string Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText) || IsEncrypted(plainText))
                return plainText ?? string.Empty;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            using var encryptor = aes.CreateEncryptor();
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return $"{Prefix}{Convert.ToBase64String(aes.IV)}:{Convert.ToBase64String(cipherBytes)}";
        }

        public string Decrypt(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText) || !IsEncrypted(cipherText))
                return cipherText ?? string.Empty;

            var parts = cipherText.Split(':', 3);
            if (parts.Length != 3)
                return cipherText;

            var iv = Convert.FromBase64String(parts[1]);
            var cipherBytes = Convert.FromBase64String(parts[2]);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        public bool IsEncrypted(string? value)
            => !string.IsNullOrEmpty(value) && value.StartsWith(Prefix, StringComparison.Ordinal);
    }
}
