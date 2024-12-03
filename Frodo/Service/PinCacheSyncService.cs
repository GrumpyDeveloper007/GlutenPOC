using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.LocationProcessing.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Frodo.Service
{
    internal class PinCacheSyncService(MapPinService _mapPinService,
        DatabaseLoaderService _databaseLoaderService,
        GeoService _geoService,
        MapPinCache _mapPinCache
        )
    {
        private readonly MapsMetaExtractorService _mapsMetaExtractorService = new();


        public void ExtractMetaInfoFromPinCache()
        {
            int i = 0;
            var cache = _mapPinCache.GetCache();
            _mapsMetaExtractorService.ClearRestaurantType();
            foreach (var item in cache)
            {
                if (string.IsNullOrWhiteSpace(item.Value.Country))
                {
                    item.Value.Country = _geoService.GetCountryPin(item.Value);
                }
                Console.WriteLine($"Processing pin meta {i} of {cache.Count}");

                if (!item.Value.MetaProcessed)
                {
                    if (string.IsNullOrWhiteSpace(item.Value.MetaHtml) && item.Value.MapsUrl != null)
                    {
                        // load meta if missing
                        var url2 = _mapPinService.GoAndWaitForUrlChange(item.Value.MapsUrl);
                        url2 = _mapPinService.GetCurrentUrl();
                        url2 = HttpUtility.UrlDecode(url2);

                        var newPin = PinHelper.GenerateMapPin(url2, item.Value.Label, "");
                        if (newPin != null)
                            item.Value.Label = newPin.Label;

                        item.Value.MetaHtml = _mapPinService.GetMeta(item.Value.Label);
                        if (string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                        {
                            var placeNames = _mapPinService.GetMapPlaceNames();
                            if (placeNames.Count > 0)
                            {
                                cache.Remove(item.Key);
                                Console.WriteLine($"Cache pin returns multiple results {item.Value.Label}");
                                continue;
                            }
                            if (placeNames.Count == 0)
                            {
                                cache.Remove(item.Key);
                                Console.WriteLine($"0 results {item.Value.Label}");
                            }

                        }

                        if (string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                        {
                            var url = _mapPinService.GetMapUrl(item.Value.Label ?? "");
                            var placeNames = _mapPinService.GetMapPlaceNames();
                            if (placeNames.Count > 0)
                            {
                                cache.Remove(item.Key);
                                Console.WriteLine($"Multiple results {item.Value.Label}");
                            }
                            if (placeNames.Count == 0)
                            {
                                //cache.Remove(item.Key);
                                Console.WriteLine($"0 results {item.Value.Label}");
                            }

                            item.Value.MapsUrl = url;
                            var pin = PinHelper.GenerateMapPin(url, "", "");
                            item.Value.MetaHtml = _mapPinService.GetMeta(item.Value.Label);
                            if (pin != null && string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                            {
                                item.Value.MetaHtml = _mapPinService.GetMeta(pin.Label);
                            }
                            if (string.IsNullOrWhiteSpace(item.Value.MetaHtml))
                            {
                                Console.WriteLine($"Unable to get meta for {item.Value.Label}");
                            }
                        }

                    }

                    if (item.Value.MetaData == null || string.IsNullOrWhiteSpace(item.Value.MetaData.RestaurantType))
                    {
                        item.Value.MetaData = _mapsMetaExtractorService.ExtractMeta(item.Value.MetaHtml);
                    }

                    if (item.Value.MetaData != null)
                    {
                        _mapsMetaExtractorService.AddRestaurantType(item.Value.MetaData.RestaurantType);
                    }
                    else
                    {
                        Console.WriteLine($"Unable to get meta for {item.Value.Label}");
                    }
                    item.Value.MetaProcessed = true;
                }
                i++;
            }

            var restaurants = _mapsMetaExtractorService.GetRestaurantTypes();
            _databaseLoaderService.SaveRestaurantList(restaurants);
            _databaseLoaderService.SavePinDB();
        }

    }
}
