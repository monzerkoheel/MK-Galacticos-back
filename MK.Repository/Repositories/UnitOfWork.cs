using MK.Repository.Data;
using System.Collections;
using MK.Core.HelperModels;
using MK.Core.Contracts.RepositoriesContracts;


namespace MK.Repository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MKGalacticosDbContext _dbContext;
        private readonly Hashtable _repositories;
        public UnitOfWork(MKGalacticosDbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Hashtable();
        }
        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _dbContext.DisposeAsync();
        }

        public IGenericRepository<T> Repository<T>() where T : ModelBase
        {
            var key = typeof(T).Name;
            if (!_repositories.ContainsKey(key))
            {
                var repo = new GenericRepository<T>(_dbContext);
                _repositories.Add(key, repo);
            }
            return _repositories[key] as IGenericRepository<T>;
        }
        public async Task BeginTransactionAsync()
            => await _dbContext.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
            => await _dbContext.Database.CommitTransactionAsync();

        public async Task RollbackTransactionAsync()
            => await _dbContext.Database.RollbackTransactionAsync();
    }
}
