using Microsoft.EntityFrameworkCore;
using MK.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MK.Repository.Data
{
    public static class DbContextSeeding
    {
        public async static Task SeedAsync(MKGalacticosDbContext _dbContext)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };

            var countryData = File.ReadAllText("../MK.Repository/Data/DataSeeding/countries_seed.json");
            var countries = JsonSerializer.Deserialize<List<Country>>(countryData);
            if (countries?.Count > 0)
            {
                if (_dbContext.Countries.Count() == 0)
                {
                    _dbContext.Countries.AddRange(countries);
                    await _dbContext.SaveChangesAsync();
                }
            }

            var governorateData = File.ReadAllText("../MK.Repository/Data/DataSeeding/governorates_seed.json");
            var governorates = JsonSerializer.Deserialize<List<Governorate>>(governorateData);
            if (governorates?.Count > 0)
            {
                if (_dbContext.Governorates.Count() == 0)
                {
                    _dbContext.Governorates.AddRange(governorates);
                    await _dbContext.SaveChangesAsync();
                }
            }

            var couponData = File.ReadAllText("../MK.Repository/Data/DataSeeding/coupon_seed.json");
            var coupons = JsonSerializer.Deserialize<List<Coupon>>(couponData, options);
            if (coupons?.Count > 0 && !await _dbContext.Coupons.AnyAsync())
            {
                _dbContext.Coupons.AddRange(coupons);

                await _dbContext.SaveChangesAsync();
            }

            var collectionData = File.ReadAllText("../MK.Repository/Data/DataSeeding/collection_seed.json");
            var collections = JsonSerializer.Deserialize<List<Collection>>(collectionData, options);
            if (collections?.Count > 0 && !await _dbContext.Collections.AnyAsync())
            {
                _dbContext.Collections.AddRange(collections);

                await _dbContext.SaveChangesAsync();
            }

            var cardsData = File.ReadAllText("../MK.Repository/Data/DataSeeding/legend_cards_seed.json");
            var cards = JsonSerializer.Deserialize<List<LegendCard>>(cardsData, options);
            if (cards?.Count > 0 && !await _dbContext.LegendCards.AnyAsync())
            {
                _dbContext.LegendCards.AddRange(cards);

                await _dbContext.SaveChangesAsync();
            }

            var shippingMethodsData = File.ReadAllText("../MK.Repository/Data/DataSeeding/shipping_methods_seed.json");
            var shippingMethods = JsonSerializer.Deserialize<List<ShippingMethod>>(shippingMethodsData, options);
            if (shippingMethods?.Count > 0 && !await _dbContext.ShippingMethods.AnyAsync())
            {
                _dbContext.ShippingMethods.AddRange(shippingMethods);

                await _dbContext.SaveChangesAsync();
            }

            var paymentMethodsData = File.ReadAllText("../MK.Repository/Data/DataSeeding/payment_methods_seed.json");
            var paymentMethods = JsonSerializer.Deserialize<List<PaymentMethod>>(paymentMethodsData, options);
            if (paymentMethods?.Count > 0 && !await _dbContext.PaymentMethods.AnyAsync())
            {
                _dbContext.PaymentMethods.AddRange(paymentMethods);

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
