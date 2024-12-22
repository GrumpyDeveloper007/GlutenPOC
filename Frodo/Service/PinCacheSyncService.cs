using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Service;
using NetTopologySuite.Index.HPRtree;
using System.Diagnostics;
using System.Web;

namespace Frodo.Service
{
    /// <summary>
    /// Data check function for the pin cache database
    /// </summary>
    internal class PinCacheSyncService(MapPinService _mapPinService,
        DatabaseLoaderService _databaseLoaderService,
        GeoService _geoService,
        MapPinCache _mapPinCache,
        RestaurantTypeService _restaurantTypeService,
        IConsole Console
        )
    {
        private readonly MapsMetaExtractorService _mapsMetaExtractorService = new(Console);


        public void ClearSearchStrings()
        {
            var cache = _mapPinCache.GetCache();
            foreach (var item in cache)
            {
                if (item.Value == null) continue;
                item.Value.SearchStrings = new();
            }

            _databaseLoaderService.SavePinDB();
        }

        public void MakeSureIndexIsInSearchString()
        {
            var cache = _mapPinCache.GetCache();
            foreach (var item in cache)
            {
                if (item.Value == null) continue;
                if (item.Value.SearchStrings.Contains(item.Key)) continue;
                item.Value.SearchStrings.Add(item.Key);
            }

            _databaseLoaderService.SavePinDB();
        }


        public void CheckRestaurantTypes()
        {
            var cache = _mapPinCache.GetCache();
            _restaurantTypeService.ClearRestaurantType();
            foreach (var item in cache)
            {
                if (item.Value == null) continue;
                if (item.Value?.MetaData?.RestaurantType != null && !string.IsNullOrWhiteSpace(item.Value.MetaData.RestaurantType))
                {
                    _restaurantTypeService.AddRestaurantType(item.Value.MetaData.RestaurantType);
                }
            }

            var restaurants = _restaurantTypeService.GetRestaurantTypes();
            _databaseLoaderService.SaveRestaurantList(restaurants);
            _databaseLoaderService.SavePinDB();
        }


        public void CheckPriceExtraction()
        {
            List<string> prices = new();
            var cache = _mapPinCache.GetCache();
            _restaurantTypeService.ClearRestaurantType();
            foreach (var item in cache)
            {
                if (item.Value == null) continue;
                if (item.Value?.MetaData?.RestaurantType != null &&
                    !string.IsNullOrWhiteSpace(item.Value.MetaData.RestaurantType))
                {
                    if (!string.IsNullOrWhiteSpace(item.Value.MetaData.Price) && !prices.Contains(item.Value.MetaData.Price))
                    {
                        prices.Add(item.Value.MetaData.Price);
                    }
                }
            }

            prices.Sort();
            foreach (var item in prices)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine($"Total price string types : {prices.Count}");
        }



        /// <summary>
        /// Checks that the pin cache database is valid and contains all needed info
        /// </summary>
        public void ExtractMetaInfoFromPinCache()
        {
            int i = 0;
            var cache = _mapPinCache.GetCache();
            var updateCount = 0;
            _restaurantTypeService.ClearRestaurantType();
            foreach (var item in cache)
            {
                i++;
                if (item.Value.MetaData != null) continue;

                if (string.IsNullOrWhiteSpace(item.Value.Country))
                {
                    item.Value.Country = _geoService.GetCountryPin(item.Value);
                }

                if (updateCount > 5)
                {
                    _databaseLoaderService.SavePinDB();
                    updateCount = 0;
                }

                Console.WriteLine($"Processing pin meta {i} of {cache.Count}");
                var metaHtml = _mapPinCache.GetMetaHtml(item.Value.GeoLatitude, item.Value.GeoLongitude);
                if (string.IsNullOrWhiteSpace(metaHtml)
                    && item.Value.MapsUrl != null
                    && !item.Value.MetaProcessed)
                {
                    // load meta if missing
                    var url2 = _mapPinService.GoAndWaitForUrlChange(item.Value.MapsUrl);
                    url2 = _mapPinService.GetCurrentUrl();
                    url2 = HttpUtility.UrlDecode(url2);
                    updateCount++;

                    var newPin = PinHelper.GenerateMapPin(url2, item.Value.Label, "");
                    if (newPin != null)
                        item.Value.Label = newPin.Label;

                    metaHtml = _mapPinService.GetMeta(item.Value.Label);
                    _mapPinCache.AddUpdateMetaHtml(metaHtml, item.Value.GeoLatitude, item.Value.GeoLongitude);

                    if (string.IsNullOrWhiteSpace(metaHtml))
                    {
                        var url = _mapPinService.GetMapUrl(item.Value.Label ?? "");
                        var placeNames = _mapPinService.GetMapPlaceNames();
                        if (placeNames.Count > 0)
                        {
                            Console.WriteLine($"Multiple results {item.Value.Label}");
                            item.Value.MetaProcessed = true;
                            continue;
                        }
                        if (placeNames.Count == 0)
                        {
                            Console.WriteLine($"0 results {item.Value.Label}");
                        }

                        item.Value.MapsUrl = url;
                        var pin = PinHelper.GenerateMapPin(url, "", "");
                        metaHtml = _mapPinService.GetMeta(item.Value.Label);

                        if (pin != null && string.IsNullOrWhiteSpace(metaHtml))
                        {
                            metaHtml = _mapPinService.GetMeta(pin.Label);
                        }
                        if (string.IsNullOrWhiteSpace(metaHtml))
                        {
                            Console.WriteLine($"Unable to get meta for {item.Value.Label}");
                            cache.Remove(item.Key);
                        }
                    }
                }

                if (!item.Value.MetaProcessed)
                {
                    item.Value.MetaData = _mapsMetaExtractorService.ExtractMeta(metaHtml);
                }

                if (item.Value.MetaData == null)
                {
                    Console.WriteLine($"Unable to get meta for {item.Value.Label}");
                }

                item.Value.MetaProcessed = true;
            }

            _databaseLoaderService.SavePinDB();
            _databaseLoaderService.SavePinHtmlDB();
        }

    }
}
