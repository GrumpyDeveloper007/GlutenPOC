using Gluten.Core.DataProcessing.Helper;
using Gluten.Core.Helper;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.PinCache;
using Gluten.Data.TopicModel;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Processes the AIVenue data type
    /// </summary>
    public class AiVenueProcessorService(MapPinService mapPinService, MappingService mappingService,
        FBGroupService fBGroupService)
    {
        private readonly MapPinService _mapPinService = mapPinService;
        private readonly MappingService _mappingService = mappingService;

        /// <summary>
        /// Returns a list of urls if the placename search results in multiple results,
        /// Filters returned results by placename, so if a search is very general, 
        /// for example 'Store', we don't just return lots of random locations
        /// </summary>
        public bool IsPlaceNameAChain(AiVenue venue, List<string> chainUrls, string groupId)
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
                var searchString = StringHelper.RemoveDiacritics(venue.PlaceName).ToUpper();
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
        public void GetMapPinForPlaceName(AiVenue venue, string groupId, List<string> chainUrls)
        {
            //var chainUrls = new List<string>();
            if (venue == null) return;
            // Don't process if it is in the skip list
            if (!AiDataFilterHelper.IsInPlaceNameSkipList(venue.PlaceName)
                && !string.IsNullOrWhiteSpace(venue.PlaceName))
            {

                string? restaurantName = venue.PlaceName + $", {fBGroupService.GetCountryName(groupId)}";
                Console.WriteLine($"Searching for {restaurantName}");
                var currentNewUrl = _mapPinService.GetMapUrl(restaurantName);

                if (currentNewUrl != null)
                {
                    var pin = _mapPinService.GetPinFromCurrentUrl(venue.PlaceName);

                    // If we are unable to get a specific pin, generate chain urls, to add later
                    if (pin == null)
                    {
                        if (IsPlaceNameAChain(venue, chainUrls, groupId))
                        {
                            Console.WriteLine($"Removing chain pin");
                            venue.PlaceName = null;
                            venue.Address = null;
                            return;
                        }
                    }


                    if (pin != null)
                    {
                        venue.Pin = _mappingService.Map<TopicPin, TopicPinCache>(pin);
                    }
                    else
                    {
                        // pin not found - try with address string
                        var address = AiDataFilterHelper.FilterAddress(venue.Address);
                        venue.Address = address;
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            restaurantName = venue.PlaceName + " " + address + $", {fBGroupService.GetCountryName(groupId)}";
                            Console.WriteLine($"Searching for (with address) {restaurantName}");
                            _mapPinService.GetMapUrl(restaurantName);

                            pin = _mapPinService.GetPinFromCurrentUrl(venue.PlaceName);
                            if (pin != null)
                            {
                                // Add pin to AiGenerated
                                if (!_mapPinService.IsPermanentlyClosed(pin.Label))
                                {
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
                        else
                        {
                            Console.WriteLine($"Still unable to process, no address :{venue.PlaceName}, address : {venue.Address}");
                        }
                    }
                }
            }

            return;
        }
    }
}
