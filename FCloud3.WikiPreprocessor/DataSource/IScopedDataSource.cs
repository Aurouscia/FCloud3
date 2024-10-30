﻿using FCloud3.WikiPreprocessor.DataSource.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.DataSource
{
    /// <summary>
    /// 每次调用解析器时传入的数据源，解析时实时查询，避免解析器被缓存时保留大量垃圾
    /// </summary>
    public interface IScopedDataSource
    {
        /// <summary>
        /// 试图将[xxx]或[aaa](xxx)中的xxx找到对应的链接
        /// </summary>
        /// <param name="linkSpan">xxx</param>
        /// <returns>链接信息</returns>
        public LinkItem? Link(string linkSpan);
    }
}
