using Gluten.Data.TopicModel;

namespace Gluten.Core.DataProcessing.Service
{
    public class AiPinGeneration(AIProcessingService aIProcessingService, MappingService mappingService,
        FBGroupService fBGroupService)
    {
        private readonly AIProcessingService _aIProcessingService = aIProcessingService;
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
            "OSAKA RESTAURANT",
            "Tsuji Safety Food",
            "Equal Eats",
            "Backup restaurant name",
            "Hilton Tokyo breakfast buffet",
            "Seibu Department store",
            "Sushi restaurant",
            "Conveyor belt sushi place",
            "Yakiniku/Korean BBQ restaurant"
             ];

        private readonly List<string> _okToSkip = [
            "Starbucks",
            "Lawson natural",
            "Lawsons",
            "Yakiniku",
            "Bikkuri Donki",
            "Mos Burger",
            "Pierre Herme",
            "Teddy's Better Burgers",
            "Seijo Isshi",
            "Seijo Ishii",
            "Eggs 'n Things",
            "Bio-Ral",
            "Supermarket",
            "Aeon",
            "Domino’s",
            "Choice",
            "GFT's",
            "GFT",
            "Tesco",
            "Subway",
            "Kaldi",
            "Choice",
            "Street food stall",
            "East Cafe",
            "Kingdom"
             ];


        /// <summary>
        /// Returns a list of urls if the placename search results in multiple results,
        /// Filters returned results by placename, so if a search is very general, 
        /// for example 'Store', we dont just return lots of random locations
        /// </summary>
        public bool IsPlaceNameAChain(AiVenue venue, List<string> chainUrls, string groupId)
        {
            if (!IsInPlaceNameSkipList(venue.PlaceName)
            && venue.Pin == null
                && !string.IsNullOrWhiteSpace(venue.PlaceName))
            {
                _aIProcessingService.GetMapUrl(venue.PlaceName + $", {fBGroupService.GetCountryName(groupId)}");
                var mapUrls = _aIProcessingService.GetMapUrls();
                var placeNames = _aIProcessingService.GetMapPlaceNames();

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
        public string? GetMapPinForPlaceName(AiVenue venue, string groupId)
        {
            string? currentNewUrl = null;
            if (venue == null) return null;
            // Dont process if it is in the skip list
            if (!IsInPlaceNameSkipList(venue.PlaceName)
                && !string.IsNullOrWhiteSpace(venue.PlaceName))
            {
                string? restaurantName = venue.PlaceName + $", {fBGroupService.GetCountryName(groupId)}";
                currentNewUrl = _aIProcessingService.GetMapUrl(restaurantName);

                if (currentNewUrl != null)
                {
                    var pin = _aIProcessingService.GetPinFromCurrentUrl(true, venue.PlaceName);
                    if (pin != null)
                    {
                        if (!_aIProcessingService.IsPermanentlyClosed(pin.Label, out string meta))
                        {
                            // Add pin to AiGenerated
                            pin.MetaHtml = meta;
                            venue.Pin = _mappingService.Map<TopicPin, TopicPinCache>(pin);
                        }
                        else
                        {
                            Console.WriteLine($"Permanently Closed");
                        }
                        currentNewUrl = null;

                    }
                    else
                    {
                        // pin not found - try with address string
                        var address = FilterAddress(venue.Address);
                        venue.Address = address;
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            restaurantName = venue.PlaceName + " " + address;
                            currentNewUrl = _aIProcessingService.GetMapUrl(restaurantName);

                            pin = _aIProcessingService.GetPinFromCurrentUrl(true, venue.PlaceName);
                            if (pin != null)
                            {
                                // Add pin to AiGenerated
                                if (!_aIProcessingService.IsPermanentlyClosed(pin.Label, out string meta))
                                {
                                    pin.MetaHtml = meta;
                                    // Add pin to AiGenerated
                                    venue.Pin = _mappingService.Map<TopicPin, TopicPinCache>(pin);
                                }
                                else
                                {
                                    Console.WriteLine($"Permanently Closed");
                                }
                                currentNewUrl = null;
                            }
                            else
                            {
                                // still unable to process
                            }
                        }
                    }
                }

                if (currentNewUrl != null
                    && IsInOkToSkip(restaurantName))
                {
                    // If we cannot find a map location and the name is in the 'ok to skip' list we continue
                    // e.g. restaurant chains without specific address info
                    currentNewUrl = null;
                }
            }

            return currentNewUrl;
        }


        public string? FilterAddress(string? address)
        {
            if (address == null) return null;
            foreach (var filter in _addressFilters)
            {
                address = address.ToLower().Replace(filter.ToLower(), "");
            }
            return address.Trim();
        }

        public bool IsInPlaceNameSkipList(string? placeName)
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

        private bool IsInOkToSkip(string placeName)
        {
            foreach (var nameFilter in _okToSkip)
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
