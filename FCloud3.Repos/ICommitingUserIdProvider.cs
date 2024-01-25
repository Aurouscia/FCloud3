using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos
{
    public interface ICommitingUserIdProvider
    {
        public int Get();
    }
}
