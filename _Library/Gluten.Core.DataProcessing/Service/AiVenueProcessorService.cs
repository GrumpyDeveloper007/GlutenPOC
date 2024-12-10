using Gluten.Core.DataProcessing.Helper;
using Gluten.Core.Helper;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Processes the AIVenue data type
    /// </summary>
    public class AiVenueProcessorService(MapPinService mapPinService,
        MappingService mappingService,
        IConsole Console)
    {
        private readonly MapPinService _mapPinService = mapPinService;
        private readonly MappingService _mappingService = mappingService;

        /// <summary>
        /// Returns a list of urls if the placename search results in multiple results,
        /// Filters returned results by placename, so if a search is very general, 
        /// for example 'Store', we don't just return lots of random locations
        /// </summary>
        public bool IsPlaceNameAChain(AiVenue venue, List<string> chainUrls)
        {
            if (!AiDataFilterHelper.IsInPlaceNameSkipList(venue.PlaceName)
            && venue.Pin == null
                && !string.IsNullOrWhiteSpace(venue.PlaceName))
            {
                //_mapPinService.GetMapUrl(venue.PlaceName + $", {fBGroupService.GetCountryName(groupId)}");
                var placeNames = _mapPinService.GetMapPlaceNames();
                var mapUrls = _mapPinService.GetMapUrls();

                // Try to filter general searches
                var matchingPlaces = new List<string>();
                var searchString = RemoveSuffixes(StringHelper.RemoveDiacritics(venue.PlaceName).ToUpper());
                for (int t = 0; t < placeNames.Count; t++)
                {
                    string? i = placeNames[t];
                    var processedString = StringHelper.RemoveDiacritics(i).ToUpper();
                    if (processedString.StartsWith(searchString, StringComparison.InvariantCulture))
                    {
                        matchingPlaces.Add(mapUrls[t]);
                    }
                }
                chainUrls.AddRange(matchingPlaces);

                return matchingPlaces.Count > 0;
            }
            return false;
        }



        /// <summary>
        /// Tries to generate a pin based on the venue place name 
        /// </summary>
        public void GetMapPinForPlaceName(AiVenue venue, string country, string? city, List<string> chainUrls)
        {
            string? searchString;
            TopicPin? newPin;
            if (venue == null) return;
            // Don't process if it is in the skip list
            if (AiDataFilterHelper.IsInPlaceNameSkipList(venue.PlaceName)
                || string.IsNullOrWhiteSpace(venue.PlaceName)) return;

            // search with city/country
            if (!string.IsNullOrWhiteSpace(city))
            {
                searchString = $"{venue.PlaceName}, {city}, {country}";
                newPin = SearchForPin(searchString, venue.PlaceName);
                if (newPin != null)
                {
                    venue.Pin = newPin;
                    return;
                }

                // If we are unable to get a specific pin, generate chain urls, to add later
                if (IsPlaceNameAChain(venue, chainUrls))
                {
                    Console.WriteLine($"Removing chain pin");
                    venue.PlaceName = null;
                    venue.Address = null;
                    return;
                }

            }


            searchString = venue.PlaceName + $", {country}";
            newPin = SearchForPin(searchString, venue.PlaceName);
            if (newPin != null)
            {
                venue.Pin = newPin;
                return;
            }

            // If we are unable to get a specific pin, generate chain urls, to add later
            if (IsPlaceNameAChain(venue, chainUrls))
            {
                Console.WriteLine($"Removing chain pin");
                venue.PlaceName = null;
                venue.Address = null;
                return;
            }

            // pin not found - try with address string
            var address = AiDataFilterHelper.FilterAddress(venue.Address);
            venue.Address = address;
            if (!string.IsNullOrWhiteSpace(address))
            {
                searchString = venue.PlaceName + " " + address + $", {country}";
                newPin = SearchForPin(searchString, venue.PlaceName);
                if (newPin != null)
                {
                    venue.Pin = newPin;
                }
                else
                {
                    Console.WriteLine($"Still unable to process :{venue.PlaceName}");
                }
            }
            else
            {
                Console.WriteLine($"Still unable to process, no address :{venue.PlaceName}, address : {venue.Address}");
            }
        }

        private TopicPin? SearchForPin(string searchString, string placeName)
        {
            Console.WriteLine($"Searching for {searchString}");
            _mapPinService.GetMapUrl(searchString);
            var pin = _mapPinService.GetPinFromCurrentUrl(placeName);
            if (pin != null)
            {
                return _mappingService.Map<TopicPin, TopicPinCache>(pin);
            }
            return null;
        }

        private static string RemoveSuffixes(string placeName)
        {
            return placeName.Replace("Restaurant", "", StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
