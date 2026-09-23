using MK.Core.HelperModels;
using MK.Core.Specifications;

namespace MK.Core.Contracts.RepositoriesContracts
{
    public interface IGenericRepository<T> where T : ModelBase
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification);
        Task<T?> GetFirstOrDefaultAsync(ISpecification<T> specification);

        Task<int> CountAsync(ISpecification<T> specification);
        Task<bool> AnyAsync(ISpecification<T> specification);

        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);

        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }
}
