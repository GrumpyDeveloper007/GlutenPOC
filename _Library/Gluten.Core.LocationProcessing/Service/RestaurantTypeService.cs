using OpenQA.Selenium.BiDi.Modules.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.LocationProcessing.Service
{
    public class RestaurantTypeService
    {
        private List<string> _rejectionList = [
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
            "Business center",
            "Car dealer",
            "Cars",
            "Charity",
            "Company",
            "Condominium complex",
            "Conference center",
            "Cooking school",
            "Criminal justice attorney",
            "Cruise line company",
            //"Dairy farm",
            "Diagnostic center",
            "E-commerce service",
            "Education center",
            "Embassy",
            "Energy equipment and solutions",
            "Executive search firm",
            "Exhibit",
            "Exporter",
            "Fashion designer",
            "Fitness center",
            "Freight forwarding service",
            "Furniture accessories",
            "Furniture maker",
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
            "House cleaning service",
            "Housing complex",
            "Karaoke Equipment Hire Service",
            "Lawn bowls club",
            "Lawn care service",
            "Marriage or relationship counselor",
            "Masonic Centre",
            "Massage therapist",
            "Media house",
            "Medical diagnostic imaging center",
            "Medical laboratory",
            "Metal fabricator",
            "Metal machinery supplier",
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
            "Senior high school",
            "Sento",
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
            ];

        public bool IsRejectedRestaurantType(string? restaurantType)
        {
            if (string.IsNullOrWhiteSpace(restaurantType)) return false;
            return _rejectionList.Exists(o => o == restaurantType);
        }
    }
}
