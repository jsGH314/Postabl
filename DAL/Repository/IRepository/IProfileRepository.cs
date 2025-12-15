using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.IRepository
{
    public interface IProfileRepository : IRepository<Profile>
    {
        void Update(Profile obj);
    }
}
