using MK.Core.Models;
using System.Linq.Expressions;


namespace MK.Repository.Specifications
{
    public class ProductWithAllDataSpecification : BaseSpecification<Product>
    {
        public ProductWithAllDataSpecification(Expression<Func<Product, bool>> criteria) : base(criteria)
        {
            EnableSplitQuery();
            AddInclude(x => x.Translations);
            AddInclude(x => x.ProductCollections);
            AddInclude("ProductCollections.Collection");
            AddInclude("ProductCollections.Collection.Translations");
            AddInclude(x => x.Images);
            AddInclude(x => x.Stocks);
            AddInclude(x => x.Prints);
        }
    }
}
