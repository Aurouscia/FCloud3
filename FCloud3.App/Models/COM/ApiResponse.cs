using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
        public ContentResult BuildResult()
        {
            return new ContentResult()
            {
                StatusCode = 200,
                Content = JsonConvert.SerializeObject(this),
                ContentType = Application.Json
            };
        }
    }

    public static class ApiResponseCodes
    {
        public const int Normal = 0;
        public const int NoTourist = 70827;
    }
}
