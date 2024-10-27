using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.DataProcessing.Service
{
    public class AiPinGeneration
    {
        private AIProcessingService _aIProcessingService;

        public AiPinGeneration(AIProcessingService aIProcessingService)
        {
            _aIProcessingService = aIProcessingService;
        }

        private List<string> _addressFilters = new List<string>() {
            "( exact location not specified)",
            "(No specific address mentioned)",
            "no specific address provided",
            "Google Maps link",
        };

        private List<string> _nameFilters = new List<string>() {
            "FamilyMart",
            "Family Mart",
            "7/11",
            "7-11",
            "7-Eleven",
            "Groceries",
            "Train Station",
            "Supermarket",
            "Unknown Restaurant",
            "Tokyo shops",
            "Google Drive",
            "Kindom"
             };

        private List<string> _okToSkip = new List<string>() {
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
            "Tesco",
            "Subway",
            "Kaldi"
             };

        public string? GetMapPinForPlaceName(AiVenue venue)
        {
            string? currentNewUrl = null;
            var restaurantName = "";
            if (venue == null) return null;
            // Dont process if we have already created a pin or name is in the skip list
            if (!IsInPlaceNameSkipList(venue.PlaceName)
            && venue.Pin == null
                && !string.IsNullOrWhiteSpace(venue.PlaceName))
            {
                // TODO: work out or generate country
                restaurantName = venue.PlaceName + ", Japan";
                currentNewUrl = _aIProcessingService.GetMapUrl(restaurantName);

                if (currentNewUrl != null)
                {
                    var pin = _aIProcessingService.GetPinFromCurrentUrl(true);
                    if (pin != null)
                    {
                        // Add pin to AiGenerated
                        venue.Pin = pin;
                        currentNewUrl = null;
                    }
                    else
                    {
                        // pin now found - try with address string
                        var address = FilterAddress(venue.Address);
                        if (!string.IsNullOrWhiteSpace(address))
                        {
                            restaurantName = venue.PlaceName + " " + address;
                            currentNewUrl = _aIProcessingService.GetMapUrl(restaurantName);

                            pin = _aIProcessingService.GetPinFromCurrentUrl(true);
                            if (pin != null)
                            {
                                // Add pin to AiGenerated
                                venue.Pin = pin;
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
                // offline process
                //currentNewUrl = null;
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
                if (placeName.ToLower().Contains(nameFilter.ToLower()))
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
                if (placeName.ToLower().Contains(nameFilter.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
