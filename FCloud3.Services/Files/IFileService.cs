using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Files
{
    public interface IFileService
    {
        /// <summary>
        /// 存储新添加的文件
        /// </summary>
        /// <param name="stream">读取的Stream</param>
        /// <param name="displayName">显示名称，例如index.html，试图上传同名的可以同时存在</param>
        /// <param name="storePathName">存储路径+名称，例如/upload/index.html，试图上传同名的会覆盖老的</param>
        /// <param name="errmsg">错误信息</param>
        /// <returns>新存入的文件Id，失败返回0</returns>
        public int Save(Stream stream, int byteCount, string displayName, string storePath,string? storeName, out string? errmsg);
        public bool ExistDisplayName(string displayName);
        public bool ExistStorePathName(string storePathName);
        public string Url(int id);
        public string Url(string displayName);
        public bool Delete(int id, out string? errmsg);
    }
}
