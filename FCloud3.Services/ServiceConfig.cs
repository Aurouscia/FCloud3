using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services
{
    public interface IServiceConfig
    {
        public int WikiTitleMaxLength { get; }
        public Func<DateTime,string> TimeFormatFull { get; }
        public Func<DateTime, string> TimeFormatShort { get; }
    }
}
