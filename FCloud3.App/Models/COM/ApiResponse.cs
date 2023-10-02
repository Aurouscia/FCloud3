using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.App.Models.COM
{
    public class ApiResponse
    {
        public bool success { get; set; } = true;
        public int code { get; set; }
        public object? data { get; set; }
        public string? errmsg { get; set; }
        public ApiResponse(object? obj, bool success = true, string? errmsg = null, int code = 0)
        {
            data = obj;
            this.success = success;
            this.errmsg = errmsg;
            if (!success && errmsg is null)
                this.errmsg = "服务器内部错误";
            this.code = code;
        }
    }
}
