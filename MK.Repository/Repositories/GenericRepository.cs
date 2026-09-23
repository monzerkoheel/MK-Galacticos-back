using Microsoft.EntityFrameworkCore;
using MK.Core.Contracts.RepositoriesContracts;
using MK.Core.HelperModels;
using MK.Core.Specifications;
using MK.Repository.Data;
using MK.Repository.Specifications;

namespace MK.Repository.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : ModelBase
    {
        private readonly MKGalacticosDbContext _dbcontext;

        public GenericRepository(MKGalacticosDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbcontext.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbcontext.Set<T>().ToListAsync();
        }

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification)
        {
            return await ApplySpecification(specification).ToListAsync();
        }

        public async Task<T?> GetFirstOrDefaultAsync(ISpecification<T> specification)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync();
        }

        public async Task<int> CountAsync(ISpecification<T> specification)
        {
            return await ApplySpecification(specification).CountAsync();
        }

        public async Task<bool> AnyAsync(ISpecification<T> specification)
        {
            return await ApplySpecification(specification).AnyAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbcontext.Set<T>().AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbcontext.Set<T>().AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _dbcontext.Set<T>().Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbcontext.Set<T>().UpdateRange(entities);
        }

        public void Delete(T entity)
        {
            _dbcontext.Set<T>().Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbcontext.Set<T>().RemoveRange(entities);
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> specification)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbcontext.Set<T>().AsQueryable(),specification);
        }
    }
}
