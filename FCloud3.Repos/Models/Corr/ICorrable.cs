using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Models.Cor
{
    public interface ICorrable
    {
        public bool MatchedCorr(Corr corr);
    }
}
