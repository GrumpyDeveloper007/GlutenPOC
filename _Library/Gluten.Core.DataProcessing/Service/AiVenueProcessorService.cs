using Gluten.Core.Helper;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Processes the AIVenue data type
    /// </summary>
    public class AiVenueProcessorService(MapPinService aIProcessingService, MappingService mappingService,
        FBGroupService fBGroupService)
    {
        private readonly MapPinService _mapPinService = aIProcessingService;
        private readonly MappingService _mappingService = mappingService;

        private readonly List<string> _addressFilters = [
            "( exact location not specified)",
            "(No specific address mentioned)",
            "(no address provided)",
            "(no specific address given)",
            "(no street address provided in the text)",
            "<No specific address provided in the given text>",
            "<no address provided in the original text>",
            "<address not provided in original text>",
            "<Not provided in original text>",
            "<insert address here>",
            "<insert address>",
            "<unknown>",
            "<not provided>",
            "no specific address provided",
            "Not specified",
            "Google Maps link",
        ];

        private readonly List<string> _nameFilters = [
            "Groceries",
            "Train Station",
            "Tokyo Station",
            "Supermarket",
            "Unknown Restaurant",
            "Tokyo shops",
            "Google Drive",
            "Christmas Cafe",
            "<insert place name>",
            "Domino’s",
            "GFTs",
            "Aomi",
            "Find Me Gluten Free",
            "Their website",
            "Sheraton",
            "Disney hotels",
            "Universal Studios",
            "Gate Building",
            "Volcano",
            "mochi place",
            "sumo wrestling",
            "Kyoto Tower GF treats store",
            "Backup restaurant name",
            "Conveyor belt sushi place",
            "GF pizza place",
            "Other sushi restaurant"
             ];

        /// <summary>
        /// Returns a list of urls if the placename search results in multiple results,
        /// Filters returned results by placename, so if a search is very general, 
        /// for example 'Store', we don't just return lots of random locations
        /// </summary>
        public bool IsPlaceNameAChain(AiVenue venue, List<string> chainUrls, string groupId)
        {
            if (!IsInPlaceNameSkipList(venue.PlaceName)
            && venue.Pin == null
                && !string.IsNullOrWhiteSpace(venue.PlaceName))
            {
                _mapPinService.GetMapUrl(venue.PlaceName + $", {fBGroupService.GetCountryName(groupId)}");
                var mapUrls = _mapPinService.GetMapUrls();
                var placeNames = _mapPinService.GetMapPlaceNames();

                // Try to filter general searches
                var matchingPlaces = new List<string>();
                var searchString = StringHelper.RemoveDiacritics(venue.PlaceName).ToUpper();
                for (int t = 0; t < placeNames.Count; t++)
                {
                    string? i = placeNames[t];
                    var processedString = StringHelper.RemoveDiacritics(i).ToUpper();
                    if (processedString.Contains(searchString, StringComparison.InvariantCulture))
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
        public void GetMapPinForPlaceName(AiVenue venue, string groupId)
        {
            if (venue == null) return;
            // Don't process if it is in the skip list
            if (!IsInPlaceNameSkipList(venue.PlaceName)
                && !string.IsNullOrWhiteSpace(venue.PlaceName))
            {
                string? restaurantName = venue.PlaceName + $", {fBGroupService.GetCountryName(groupId)}";
                var currentNewUrl = _mapPinService.GetMapUrl(restaurantName);

                if (currentNewUrl != null)
                {
                    var pin = _mapPinService.GetPinFromCurrentUrl(true, venue.PlaceName);
                    if (pin != null)
                    {
                        if (!_mapPinService.IsPermanentlyClosed(pin.Label, out string meta))
                        {
                            // Add pin to AiGenerated
                            pin.MetaHtml = meta;
                            venue.Pin = _mappingService.Map<TopicPin, TopicPinCache>(pin);
                        }
                        else
                        {
                            Console.WriteLine($"Permanently Closed");
                        }
                    }
                    else
                    {
                        // pin not found - try with address string
                        var address = FilterAddress(venue.Address);
                        venue.Address = address;
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            restaurantName = venue.PlaceName + " " + address;
                            _mapPinService.GetMapUrl(restaurantName);

                            pin = _mapPinService.GetPinFromCurrentUrl(true, venue.PlaceName);
                            if (pin != null)
                            {
                                // Add pin to AiGenerated
                                if (!_mapPinService.IsPermanentlyClosed(pin.Label, out string meta))
                                {
                                    pin.MetaHtml = meta;
                                    // Add pin to AiGenerated
                                    venue.Pin = _mappingService.Map<TopicPin, TopicPinCache>(pin);
                                }
                                else
                                {
                                    Console.WriteLine($"Permanently Closed");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Still unable to process :{venue.PlaceName}");
                            }
                        }
                    }
                }
            }

            return;
        }

        private string? FilterAddress(string? address)
        {
            if (address == null) return null;
            foreach (var filter in _addressFilters)
            {
                address = address.ToLower().Replace(filter.ToLower(), "");
            }
            return address.Trim();
        }

        private bool IsInPlaceNameSkipList(string? placeName)
        {
            if (placeName == null) return false;
            foreach (var nameFilter in _nameFilters)
            {
                if (placeName.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
