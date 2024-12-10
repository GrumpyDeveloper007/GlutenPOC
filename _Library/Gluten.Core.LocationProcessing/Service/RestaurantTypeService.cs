using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.LocationProcessing.Service
{
    public class RestaurantTypeService
    {
        private readonly List<string> _restaurantTypes = [];

        private readonly List<string> _rejectionList = [
            "Adult education school",
            "Aged Care Service",
            "Agricultural service",
            "Airline",
            "Animal hospital",
            "Arts organization",
            "Auditor",
            "Auto body parts supplier",
            "Bail bonds service",
            "Baseball field",
            "Beauty product supplier",
            "Beauty salon",
            "Biotechnology company",
            "Boat tour agency",
            //"Buddhist temple",
            "Bridge",
            "Business center",
            "Car dealer",
            "Cars",
            "Car racing track",
            "Charity",
            "Children's book store",
            "Clothes market",
            "Company",
            "Condominium complex",
            "Conference center",
            "Corporate office",
            "Cooking school",
            "Criminal justice attorney",
            "Cruise line company",
            //"Dairy farm",
            "Diagnostic center",
            "Driveshaft shop",
            "E-commerce service",
            "Education center",
            "Electronics manufacturer",
            "Embassy",
            "Energy equipment and solutions",
            "Executive search firm",
            "Exhibit",
            "Exporter",
            "Fashion designer",
            "Florist",
            "Fitness center",
            "Freight forwarding service",
            "Food manufacturer",
            "Furniture accessories",
            "Furniture maker",
            "Garden",
            "Glazier",
            "Golf course",
            //"Grain elevator",
            "Graphic designer",
            "Gym",
            "Hairdresser",
            "Handyman/Handywoman/Handyperson",
            "Health consultant",
            "Heritage building",
            "Hindu temple",
            //"Holiday home",
            //"Homestay",
            "Hair salon",
            "Historical landmark",
            "House cleaning service",
            "Housing complex",
            "Island",
            "Karaoke Equipment Hire Service",
            "Laundromat",
            "Language school",
            "Lawn bowls club",
            "Lawn care service",
            "Manufacturer",
            "Marriage or relationship counselor",
            "Masonic Centre",
            "Massage therapist",
            "Media house",
            "Medical diagnostic imaging center",
            "Medical laboratory",
            "Metal fabricator",
            "Metal machinery supplier",
            "Mountain peak",
            "Nail salon",
            "Performing arts theater",
            "Photography studio",
            "Playground",
            "Poultry farm",
            "Private hospital",
            "Private university",
            "Real estate agency",
            "Real estate consultant",
            "Real estate rental agency",
            "School",
            "Sculpture",
            "Subway station",
            "Senior high school",
            "Sento",
            "Sightseeing tour agency",
            "Software company",
            "Textile mill",
            "Tour agency",
            "Tourist information center",
            "Tradesmen",
            "Training centre",
            "Transportation service",
            "Travel Agents",
            "Used car dealer",
            "Vehicle exporter",
            "Wholesale plant nursery",
            "Wholesaler",
                        "Train station",
            "Airports",
            "airport",
            "Sightseeing tour agency",
            "Laundromat",
            "Historical landmark",
            "Island",
            "Electronics manufacturer",
            "Corporate office",
            "Theme park",
            "Subway station",
            "Food manufacturer",
            "Art museum",
            "International airport",
            "Airport",
            "Garden",
            "Cinema",
            "Manufacturer",
            "Observation deck",
            "Mountain peak",
            "Amusement park",
            "Bridge",
            "Soy sauce maker",
            "Housing development",
            "Massage spa",
            "Waterfall",
            "Delivery service",
            "Water treatment supplier",
            "River",
            "Event venue",
            "Museum",
            "Florist",
            "Park",
            "Tourist attraction",
            "Business park",
            "Hair salon",
            "Holiday apartment",
            "Car racing track",
            "Language school",
            "Host club",
            "Shinto shrine",
            "Cultural center",
            "Foreign consulate",
            "Non-profit organization",
            "Truck parts supplier",
            "Gift shop",
            "Lake",
            "Spa",
            "Festival",
            "Beach",
            "Dog trainer",
            "Concert hall",
            "Tour operator",
            "Art gallery",
            "Health and beauty sho",
            "public bath",
            "review",
            "City park",
            "Community center",
            "Cooking class",
            "Coworking space",
            "Garment exporter",
            "General hospital",
            "Medical clinic",
            "Modern art museum",
            "Mobile caterer",
            "Scenic spot",
            "public bath",
            "Yoga studio"
            ];

        /// <summary>
        /// Checks to see if the restaurant type is in the rejection list
        /// </summary>
        public bool IsRejectedRestaurantType(string? restaurantType)
        {

            if (string.IsNullOrWhiteSpace(restaurantType)) return false;
            return _rejectionList.Exists(o => o == restaurantType);
        }

        /// <summary>
        /// Gets a list of extracted restaurant types
        /// </summary>
        public List<string> GetRestaurantTypes()
        {
            _restaurantTypes.Sort();
            var data = new List<string>();
            var ignoreList = _rejectionList;
            foreach (var item in _restaurantTypes)
            {
                if (!item.Contains("shop", StringComparison.InvariantCultureIgnoreCase)
                    && !item.Contains("store", StringComparison.InvariantCultureIgnoreCase)
                    && !item.Contains("market", StringComparison.InvariantCultureIgnoreCase)
                    && !item.Contains("hotel", StringComparison.InvariantCultureIgnoreCase)
                    && !ignoreList.Any(s => item.Contains(s, StringComparison.InvariantCultureIgnoreCase))
                    )
                {
                    data.Add(item);
                }
            }
            return data;
        }

        /// <summary>
        /// Add a restaurant type to the data
        /// </summary>
        public void AddRestaurantType(string? restaurant)
        {
            if (!string.IsNullOrWhiteSpace(restaurant) && !_restaurantTypes.Contains(restaurant))
            {
                _restaurantTypes.Add(restaurant);
            }
        }

        /// <summary>
        /// clears the restaurant type data
        /// </summary>
        public void ClearRestaurantType()
        {
            _restaurantTypes.Clear();
        }
    }
}
