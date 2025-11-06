using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Repository.IRepository;

namespace DAL.Repository
{
    public class BlogPostRepository : Repository<BlogPost>, IBlogPostRepository
    {
        private readonly ApplicationDbContext _db;
        public BlogPostRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(BlogPost obj)
        {
            var objFromDb = _db.BlogPosts.FirstOrDefault(s => s.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Title = obj.Title;
                objFromDb.Content = obj.Content;
                objFromDb.PublishedDate = obj.PublishedDate;
                objFromDb.Author = obj.Author;
                objFromDb.IsPublic = obj.IsPublic;
                objFromDb.Likes = obj.Likes;
                objFromDb.ApplicationUserId = obj.ApplicationUserId;
            }
        }
    }
}
