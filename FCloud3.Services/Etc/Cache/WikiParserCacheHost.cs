using FCloud3.Entities.Sys;
using FCloud3.Repos.Sys;
using FCloud3.Services.Etc.Cache.Base;
using FCloud3.WikiPreprocessor.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Etc.Cache
{
    public class WikiParserCacheHost(LastUpdateRepo lastUpdateRepo)
        : NewestEnsuredCacheHost<Parser>(lastUpdateRepo)
    {
        protected override LastUpdateType[] LuTypes => [
                LastUpdateType.Material,
                LastUpdateType.WikiItemRefedProps,
                LastUpdateType.WikiTitleContain
            ];
    }
}
