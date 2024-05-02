using FCloud3.DbContexts;
using FCloud3.Entities.Messages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Messages
{
    public class NotificationRepo(
        FCloudContext context,
        ICommitingUserIdProvider userIdProvider) 
        : RepoBase<Notification>(context, userIdProvider)
    {
        public IQueryable<Notification> TakeRange(int skip, int take)
        {
            return Existing.OrderByDescending(x=>x.Created).Skip(skip).Take(take);
        }
    }
}
