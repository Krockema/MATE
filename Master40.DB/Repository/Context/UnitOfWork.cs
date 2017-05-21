using Master40.DB.Repository.Data;
using Master40.DB.Repository.Domains;
using Master40.DB.Repository.Interfaces;

namespace Master40.DB.Repository.Context
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MasterDBContext _context;

        public UnitOfWork(MasterDBContext context)
        {
            _context = context;
            Articles = new ArticleRepository(_context);
            ArticleBoms = new ArticleBomRepository(_context);
        }

        public IArticleRepository Articles { get; private set; }
        public IArticleBomRepository ArticleBoms { get; private set; }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
