namespace FCloud3.Services.Ai
{
    /// <summary>
    /// AI 实例 API Key 加解密服务
    /// </summary>
    public interface IAiApiKeyEncryption
    {
        /// <summary>加密明文 Key，空值原样返回</summary>
        string Encrypt(string? plainText);
        /// <summary>解密密文 Key，空值或明文原样返回</summary>
        string Decrypt(string? cipherText);
        /// <summary>判断值是否为已加密格式</summary>
        bool IsEncrypted(string? value);
    }
}
