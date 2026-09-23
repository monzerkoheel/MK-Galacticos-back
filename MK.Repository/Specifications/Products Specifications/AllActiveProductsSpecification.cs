using MK.Core.HelperModels;
using MK.Core.Models;
using MK.Core.Specifications.SpecificationParameters;


namespace MK.Repository.Specifications.Products_Specifications
{
    public class AllActiveProductsSpecification : ProductWithAllDataSpecification
    {
        public AllActiveProductsSpecification(ActiveProductsSpecPrams specPrams, Language language)
            : base(x =>
               x.Visibility == Visibility.Active
            && (string.IsNullOrEmpty(specPrams.Search) ||
                    x.Translations.Any(t => t.Language == language &&  t.Name.ToLower().Contains(specPrams.Search.ToLower())) ||
                    x.Prints.Any(x => x.Name.ToLower().Contains(specPrams.Search.ToLower())))
            && (specPrams.CollectionIds == null || !specPrams.CollectionIds.Any() ||
                    x.ProductCollections.Any(pc => specPrams.CollectionIds.Contains(pc.CollectionId)))
            && (!specPrams.Gender.HasValue || x.AvailableFor == specPrams.Gender.Value)
            && (specPrams.ProductSizes == null || !specPrams.ProductSizes.Any() ||
                    x.Stocks.Any(s => specPrams.ProductSizes.Contains(s.Size) && s.StockQuantity > 0))
            && (specPrams.MinPrice == null || x.Price >= specPrams.MinPrice.Value)
            && (specPrams.MaxPrice == null || x.Price <= specPrams.MaxPrice.Value)
            )
        {

            if (specPrams.Sort.HasValue && !specPrams.DesSort.HasValue)
            {
                if (specPrams.Sort == ActiveProductsSort.Price)
                    AddOrderBy(x => x.Price);
                else if (specPrams.Sort == ActiveProductsSort.Id)
                    AddOrderBy(x => x.Id);
            }

            if (specPrams.DesSort.HasValue && !specPrams.Sort.HasValue)
            {
                if (specPrams.DesSort == ActiveProductsSort.Price)
                    AddOrderByDes(x => x.Price);
                else if (specPrams.DesSort == ActiveProductsSort.Id)
                    AddOrderByDes(x => x.Id);
            }
            if (!specPrams.DesSort.HasValue && !specPrams.Sort.HasValue)
                AddOrderByDes(x => x.Id);

            if (specPrams.PageIndex != 0 && specPrams.PageSize != 0)
                ApplyPagination((specPrams.PageIndex - 1) * specPrams.PageSize, specPrams.PageSize);
        }
    }
}
