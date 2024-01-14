using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Wiki
{
    public class WikiParaRepo : RepoBase<WikiPara>
    {
        public WikiParaRepo(FCloudContext context) : base(context)
        {
        }
    }
}
