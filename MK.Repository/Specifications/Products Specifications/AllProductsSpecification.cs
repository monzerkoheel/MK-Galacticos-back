using MK.Core.DTOs.Products;
using MK.Core.HelperModels;
using MK.Core.Specifications.SpecificationParameters;

namespace MK.Repository.Specifications
{
    public class AllProductsSpecification : ProductWithAllDataSpecification
    {
        public AllProductsSpecification(ProductSpecPrams specPrams)
            : base(x =>
                (string.IsNullOrEmpty(specPrams.Search) ||
                    x.Translations.Any(t => t.Language == Language.English && t.Name.ToLower().Contains(specPrams.Search.ToLower())) ||
                    x.SKU.ToLower().Contains(specPrams.Search.ToLower()))
            &&  (!specPrams.CollectionId.HasValue || 
                    x.ProductCollections.Any(x => x.CollectionId == specPrams.CollectionId.Value))
            &&  (!specPrams.Gender.HasValue || x.AvailableFor == specPrams.Gender.Value)
            &&  (!specPrams.Status.HasValue || x.Visibility == specPrams.Status.Value)
            &&  (!specPrams.PrintType.HasValue || 
                    (specPrams.PrintType == ProductPrintType.None && !x.IsCustomPrintAvailable && !x.Prints.Any()) ||
                    (specPrams.PrintType == ProductPrintType.Custom && x.IsCustomPrintAvailable) ||
                    (specPrams.PrintType == ProductPrintType.Players && x.Prints.Any()))
            &&  (!specPrams.StockFilter.HasValue || 
                    (specPrams.StockFilter == StockFilter.InStock && x.Stocks.Sum(x => x.StockQuantity) > 0) ||
                    (specPrams.StockFilter == StockFilter.LowStock && x.Stocks.Sum(x => x.StockQuantity) <= x.LowStockThreshold && x.Stocks.Sum(x => x.StockQuantity) > 0) ||
                    (specPrams.StockFilter == StockFilter.OutOfStock && x.Stocks.Sum(x => x.StockQuantity) <= 0))
            )
        {
            if (specPrams.Sort.HasValue && !specPrams.DesSort.HasValue)
            {
                if (specPrams.Sort == ProductsSort.Price)
                    AddOrderBy(x => x.Price);
                else if (specPrams.Sort == ProductsSort.Stock)
                    AddOrderBy(x => x.Stocks.Sum(x => x.StockQuantity));
            }

            if (specPrams.DesSort.HasValue && !specPrams.Sort.HasValue)
            {
                if (specPrams.DesSort == ProductsSort.Price)
                    AddOrderByDes(x => x.Price);
                else if (specPrams.DesSort == ProductsSort.Stock)
                    AddOrderByDes(x => x.Stocks.Sum(x => x.StockQuantity));
            }
            if (!specPrams.Sort.HasValue && !specPrams.DesSort.HasValue)
            {
                AddOrderBy(x => x.Id);
            }

            if (specPrams.PageIndex != 0 && specPrams.PageSize != 0)
                ApplyPagination((specPrams.PageIndex - 1) * specPrams.PageSize, specPrams.PageSize);
        }
    }
}
