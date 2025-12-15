using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IApplicationUserRepository ApplicationUser { get; }
        IBlogPostRepository BlogPost { get; }
        IProfileRepository Profile { get; }
        void Save();
    }
}
