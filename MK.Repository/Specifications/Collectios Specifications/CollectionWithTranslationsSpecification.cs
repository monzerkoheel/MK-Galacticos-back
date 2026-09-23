using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MK.Repository.Specifications
{
    public class CollectionWithTranslationsSpecification : BaseSpecification<Collection>
    {
        public CollectionWithTranslationsSpecification(Expression<Func<Collection, bool>> criteria) : base(criteria)
        {
            AddInclude(x => x.Translations);
            AddOrderBy(x => x.DisplayOrder);
        }
    }
}
