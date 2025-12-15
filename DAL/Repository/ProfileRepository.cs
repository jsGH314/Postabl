using DAL.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class ProfileRepository : Repository<Profile>, IProfileRepository
    {
        private readonly ApplicationDbContext _db;
        public ProfileRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Profile obj)
        {
            _db.Profiles.Update(obj);
        }
    }
}
