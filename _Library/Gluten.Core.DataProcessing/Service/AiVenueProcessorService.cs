using Gluten.Core.DataProcessing.Helper;
using Gluten.Core.Helper;
using Gluten.Core.Interface;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;
using System;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using System.Web;

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
        public bool IsPlaceNameAChain(List<string> chainUrls, string placeName)
        {
            if (!PlaceNameFilterHelper.StartsWithPlaceNameSkipList(placeName)
                && !string.IsNullOrWhiteSpace(placeName))
            {
                //_mapPinService.GetMapUrl(placeName + $", {fBGroupService.GetCountryName(groupId)}");
                var placeNames = _mapPinService.GetMapPlaceNames();
                var mapUrls = _mapPinService.GetMapUrls();

                // Try to filter general searches
                var matchingPlaces = new List<string>();
                var searchString = RemoveSuffixes(StringHelper.RemoveDiacritics(placeName).ToUpper());
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

        public string FilterPlaceName(string placeName, string country)
        {
            // TODO: Add parsing of name for spelling errors?
            // TODO: Remove restaurant types from place name?
            placeName = RemoveTextInBrackets(placeName);
            placeName = PlaceNameAdjusterHelper.FixUserErrorsInPlaceNames(placeName, country);
            return placeName;
        }


        /// <summary>
        /// Tries to generate a pin based on the venue place name 
        /// </summary>
        public bool GetMapPinForPlaceName(AiVenue venue, string country, string? city, List<string> chainUrls, bool enableChainMatch)
        {
            string? searchString;
            string placeName = venue.PlaceName ?? "";

            // Don't process if it is in the skip list
            if (PlaceNameFilterHelper.StartsWithPlaceNameSkipList(placeName)
                || string.IsNullOrWhiteSpace(placeName)) return false;

            // TODO: Add parsing of name for spelling errors?
            // TODO: Remove restaurant types from place name?
            placeName = RemoveTextInBrackets(placeName);
            placeName = PlaceNameAdjusterHelper.FixUserErrorsInPlaceNames(placeName, country);

            if (venue == null) return false;

            // pin not found - try with address string
            var address = AddressFilterHelper.FilterAddress(venue.Address);
            venue.Address = address;

            if (!string.IsNullOrWhiteSpace(address))
            {
                searchString = $"{placeName} {address}, {country}";
                if (SearchForPlace(venue, searchString, placeName, chainUrls, $" {address}, {country}", enableChainMatch)) return true;
            }
            // search with city/country
            else if (!string.IsNullOrWhiteSpace(city))
            {
                searchString = $"{placeName}, {city}, {country}";
                if (SearchForPlace(venue, searchString, placeName, chainUrls, $" {city}, {country}", enableChainMatch)) return true;
            }
            else
            {
                searchString = $"{placeName}, {country}";
                if (SearchForPlace(venue, searchString, placeName, chainUrls, $", {country}", enableChainMatch)) return true;
            }

            if (venue.Pin == null)
            {
                Console.WriteLine($"Still unable to process, no address :{placeName}, address : {venue.Address}");
            }
            return false;
        }

        private bool SearchForPlace(AiVenue venue, string searchString, string placeName, List<string> chainUrls, string suffix, bool enableChainMatch)
        {
            var newPin = SearchForPin(searchString, placeName, venue.PlaceName);
            if (newPin != null)
            {
                //Console.WriteLine($"Found pin :{searchString}");
                venue.Pin = newPin;
                venue.IsChain = false;
                return true;
            }
            venue.PinsFound = _mapPinService.GetMapUrls().Count;

            if (!enableChainMatch) return false;

            // If we are unable to get a specific pin, generate chain urls, to add later
            if (IsPlaceNameAChain(chainUrls, placeName))
            {
                return true;
            }

            var url = _mapPinService.GetCurrentUrl();
            var mapPin = HttpUtility.UrlDecode(PinHelper.GetPlaceNameFromSearchUrl(url));
            mapPin = mapPin.Replace(suffix, "");
            if (IsPlaceNameAChain(chainUrls, mapPin))
            {
                return true;
            }
            return false;
        }

        private TopicPin? SearchForPin(string searchString, string placeName, string originalPlaceName)
        {
            searchString = searchString.Replace("_", " ");
            //Console.WriteLine($"Searching for {searchString}");
            _mapPinService.GetMapUrl(searchString);
            var pin = _mapPinService.GetPinFromCurrentUrl(placeName, originalPlaceName);

            if (pin != null)
            {
                return _mappingService.Map<TopicPin, TopicPinCache>(pin);
            }
            return null;
        }

        private static string RemoveSuffixes(string placeName)
        {
            if (placeName.EndsWith("Restaurant"))
                return placeName.Replace("Restaurant", "", StringComparison.InvariantCultureIgnoreCase);
            return placeName;
        }

        private string RemoveTextInBrackets(string text)
        {
            string result = Regex.Replace(text, @"\s*\(.*?\)\s*", " ");

            // Trim any extra spaces
            return result.Trim();
        }

    }
}
