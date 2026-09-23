using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MK.Repository.Specifications
{
    public class ProductWithTranslationsSpecification : BaseSpecification<Product>
    {
        public ProductWithTranslationsSpecification(Expression<Func<Product, bool>> criteria) : base(criteria)
        {
            AddInclude(x => x.Translations);

        }
    }
}
