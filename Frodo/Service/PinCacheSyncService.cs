using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.PinCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        /// <summary>
        /// Checks that the pin cache database is valid and contains all needed info
        /// </summary>
        public void ExtractMetaInfoFromPinCache()
        {
            int i = 0;
            var cache = _mapPinCache.GetCache();
            List<string> toDelete = new();
            List<string> prices = new();
            var updateCount = 0;
            _restaurantTypeService.ClearRestaurantType();
            foreach (var item in cache)
            {
                if (string.IsNullOrWhiteSpace(item.Value.Country))
                {
                    item.Value.Country = _geoService.GetCountryPin(item.Value);
                }

                if (updateCount > 20)
                {
                    _databaseLoaderService.SavePinDB();
                    updateCount = 0;
                }

                Console.WriteLine($"Processing pin meta {i} of {cache.Count}");
                if (string.IsNullOrWhiteSpace(item.Value.MetaHtml)
                    && item.Value.MapsUrl != null
                    && !item.Value.MetaProcessed)
                {
                    // load meta if missing
                    var url2 = _mapPinService.GoAndWaitForUrlChange(item.Value.MapsUrl);
                    url2 = _mapPinService.GetCurrentUrl();
                    url2 = HttpUtility.UrlDecode(url2);

                    var newPin = PinHelper.GenerateMapPin(url2, item.Value.Label, "");
                    if (newPin != null)
                        item.Value.Label = newPin.Label;

                    item.Value.MetaHtml = _mapPinService.GetMeta(item.Value.Label);
                    // sync separate html data store
                    if (item.Value.MetaHtml != null && item.Value.GeoLatitude != null && item.Value.GeoLongitude != null)
                        _mapPinCache.AddUpdateMetaHtml(item.Value.MetaHtml, item.Value.GeoLatitude, item.Value.GeoLongitude);

                    if (string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                    {
                        var placeNames = _mapPinService.GetMapPlaceNames();
                        if (placeNames.Count > 0)
                        {
                            Console.WriteLine($"Cache pin returns multiple results {item.Value.Label}");
                            continue;
                        }
                        if (placeNames.Count == 0)
                        {
                            Console.WriteLine($"0 results {item.Value.Label}");
                        }

                    }

                    if (string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                    {
                        updateCount++;
                        var url = _mapPinService.GetMapUrl(item.Value.Label ?? "");
                        var placeNames = _mapPinService.GetMapPlaceNames();
                        if (placeNames.Count > 0)
                        {
                            Console.WriteLine($"Multiple results {item.Value.Label}");
                        }
                        if (placeNames.Count == 0)
                        {
                            Console.WriteLine($"0 results {item.Value.Label}");
                        }

                        item.Value.MapsUrl = url;
                        var pin = PinHelper.GenerateMapPin(url, "", "");
                        item.Value.MetaHtml = _mapPinService.GetMeta(item.Value.Label);

                        // sync separate html data store
                        if (item.Value.MetaHtml != null && item.Value.GeoLatitude != null && item.Value.GeoLongitude != null)
                            _mapPinCache.AddUpdateMetaHtml(item.Value.MetaHtml, item.Value.GeoLatitude, item.Value.GeoLongitude);

                        if (pin != null && string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                        {
                            item.Value.MetaHtml = _mapPinService.GetMeta(pin.Label);
                        }
                        if (string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                        {
                            Console.WriteLine($"Unable to get meta for {item.Value.Label}");
                            cache.Remove(item.Key);
                        }
                    }

                }

                if (!item.Value.MetaProcessed)
                {
                    item.Value.MetaData = _mapsMetaExtractorService.ExtractMeta(item.Value.MetaHtml);
                }

                if (!string.IsNullOrWhiteSpace(item.Value.MetaData.RestaurantType))
                {
                    _restaurantTypeService.AddRestaurantType(item.Value.MetaData.RestaurantType);
                    if (!string.IsNullOrWhiteSpace(item.Value.MetaData.Price) && !prices.Contains(item.Value.MetaData.Price))
                    {
                        prices.Add(item.Value.MetaData.Price);
                    }
                }


                if (item.Value.MetaData == null)
                {
                    Console.WriteLine($"Unable to get meta for {item.Value.Label}");
                }
                else
                {

                }

                item.Value.MetaProcessed = true;

                i++;
            }

            Console.WriteLine($"max restaurant match : {_mapsMetaExtractorService.maxMatchIndex}");
            var restaurants = _restaurantTypeService.GetRestaurantTypes();
            _databaseLoaderService.SaveRestaurantList(restaurants);
            _databaseLoaderService.SavePinDB();
        }

    }
}
