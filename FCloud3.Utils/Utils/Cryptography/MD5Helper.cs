using System.Security.Cryptography;
using System.Text;

namespace FCloud3.Utils.Utils.Cryptography
{
    public static class MD5Helper
    {
        public static string? GetMD5Of(string? input)
        {
            if (input is null)
                return null;

            // 把输入的字符串转换为字节数组并计算哈希值
            byte[] data = MD5.HashData(Encoding.UTF8.GetBytes(input));


            // 新建一个Sb对象来收集结果
            StringBuilder sBuilder = new();


            // 对于上述结果的每一个byte
            // 把byte转换为16进制字符串
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }


            // 返回“16进制”字符串
            return sBuilder.ToString();
        }

        //该函数从CSDN复制来
        //原文链接：https://blog.csdn.net/u014627020/article/details/123946000
    }
}
