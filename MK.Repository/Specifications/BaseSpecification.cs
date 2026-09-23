using MK.Core.HelperModels;
using MK.Core.Specifications;
using System.Linq.Expressions;

namespace MK.Repository.Specifications
{
    public class BaseSpecification<T> : ISpecification<T> where T : ModelBase
    {
        public Expression<Func<T, bool>> critria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; } = new List<Expression<Func<T, object>>>();
        public List<string> IncludeStrings { get; set; } = new List<string>();
        public Expression<Func<T, object>> OrderBy { get; set; }
        public Expression<Func<T, object>> OrderByDes { get; set; }
        public Expression<Func<T, object>> ThenOrder { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }
        public bool IsPaginationEnabled { get; set; }
        public bool IsSplitQueryEnabled { get; set; }
        public BaseSpecification()
        {

        }
        public BaseSpecification(Expression<Func<T, bool>> critriaExpression)
        {
            critria = critriaExpression;
        }
        public void AddOrderBy(Expression<Func<T, object>> orderBy)
        {
            OrderBy = orderBy;
        }
        public void AddOrderByDes(Expression<Func<T, object>> orderByDes)
        {
            OrderByDes = orderByDes;
        }
        public void AddSecondOrderBy(Expression<Func<T, object>> thenOrder)
        {
            ThenOrder = thenOrder;
        }
        public void ApplyPagination(int skip, int take)
        {
            IsPaginationEnabled = true;
            Skip = skip;
            Take = take;
        }
        public void EnableSplitQuery()
        {
            IsSplitQueryEnabled = true;
        }
        public void AddInclude(Expression<Func<T, object>> include)
        {
            Includes.Add(include);
        }
        public void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }
    }
}
