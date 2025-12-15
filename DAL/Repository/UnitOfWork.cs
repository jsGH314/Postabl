using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Repository.IRepository;
using Models;

namespace DAL.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        //private readonly UserManager<ApplicationUser> _userManager;
        public IBlogPostRepository BlogPost { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IProfileRepository Profile { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            ApplicationUser = new ApplicationUserRepository(_db);
            BlogPost = new BlogPostRepository(_db);
            Profile = new ProfileRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
