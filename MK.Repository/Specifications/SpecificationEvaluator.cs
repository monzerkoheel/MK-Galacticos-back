using Microsoft.EntityFrameworkCore;
using MK.Core.HelperModels;
using MK.Core.Specifications;

namespace MK.Repository.Specifications
{
    internal static class SpecificationEvaluator<T> where T : ModelBase
    {
        public static IQueryable<T> GetQuery(IQueryable<T> innerQuery, ISpecification<T> spec)
        {
            var query = innerQuery;

            if (spec.critria is not null)
            {
                query = query.Where(spec.critria);
            }

            if (spec.IsSplitQueryEnabled)
            {
                query = query.AsSplitQuery();
            }

            if (spec.OrderBy is not null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDes is not null)
            {
                query = query.OrderByDescending(spec.OrderByDes);
            }

            if (spec.ThenOrder is not null && (spec.OrderBy is not null || spec.OrderByDes is not null))
            {
                query = ((IOrderedQueryable<T>)query).ThenBy(spec.ThenOrder);
            }


            if (spec.IsPaginationEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }


            query = spec.Includes.Aggregate(query, (currentquery, IncludeExpression) => currentquery.Include(IncludeExpression));

            query = spec.IncludeStrings.Aggregate(query,(currentquery, IncludeString) => currentquery.Include(IncludeString));
            return query;
        }
    }
}
