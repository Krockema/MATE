using Master40.DB.Repository.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IArticleRepository Articles { get; }
        IArticleBomRepository ArticleBoms { get; }
        int Complete();
    }
}
