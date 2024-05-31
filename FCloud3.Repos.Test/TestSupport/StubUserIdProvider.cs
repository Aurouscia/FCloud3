using FCloud3.Repos.Etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Test.TestSupport
{
    internal class StubUserIdProvider : ICommitingUserIdProvider
    {
        public StubUserIdProvider(int userId) 
        {
            UserId = userId;
        }
        public int UserId { get; set; }
        public int Get() => UserId;
    }
}
