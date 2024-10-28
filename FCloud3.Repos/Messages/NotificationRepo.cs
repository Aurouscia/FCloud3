using FCloud3.DbContexts;
using FCloud3.Entities.Messages;
using FCloud3.Repos.Etc;
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
            => Mine.OrderByDescending(x=>x.Created).Skip(skip).Take(take);
        public void MarkRead(int id) {
            Mine.Where(x => x.Id == id).ExecuteUpdate(x => x
                .SetProperty(n => n.Read, true)
                .SetProperty(n => n.Updated, DateTime.Now));
            AfterDataChange();
        }
        public void MarkAllRead()
        {
            Mine.ExecuteUpdate(x => x
                .SetProperty(n => n.Read, true)
                .SetProperty(n => n.Updated, DateTime.Now));
            AfterDataChange();
        }

        public IQueryable<Notification> Mine => Existing.Where(x => x.Receiver == _userIdProvider.Get());
        public IQueryable<Notification> MineUnread => Mine.Where(x => !x.Read);

        public new void Add(Notification notification) => base.Add(notification);
    }
}
