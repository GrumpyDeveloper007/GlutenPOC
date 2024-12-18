// Ignore Spelling: geo

using Gluten.Core.Interface;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gluten.Core.DataProcessing.Service
{
    /// <summary>
    /// Provides information about FB groups, 
    /// TODO: Move hard coded elements to DB
    /// </summary>
    public class FBGroupService(IConsole Console)
    {
        private readonly Dictionary<string, string> _knownGroupIds = new()
        {
            {"806902313439614","Thailand" }, // not gf group
            {"769136475365475","NA" }, // dales gf map
            {"573768437691444","NA" },


            {"379994195544478","Japan" },//Gluten-Free in Japan!
            {"182984958515029","Singapore" },//Gluten Free Singapore - Support Group
            {"3087018218214300","Indonesia" },//Gluten Free in Bali
            {"422262581142441","China" },//Gluten Free in Hong Kong
            {"660915839470807","Vietnam" },//Gluten Free Saigon (Ho Chi Minh City)
            {"823200180025057","Vietnam" },//Gluten-free Hội An Community
            {"353439621914938","Taiwan" }, //Gluten free Taipei
            {"319517678837045","Vietnam" },//Gluten Free Hanoi
            {"1015752345220391","Thailand" },//Gluten Free Chiang Mai
            {"1053129328213251","Thailand" },//Gluten Free Thailand
            {"852980778556330","Fiji" },//Gluten Free Fiji
            {"1420852834795381","South Korea" }, //Wheat and Gluten-Free in South Korea
            {"302515126584130","Philippines" }, //Gluten-Free Philippines
            {"422284561238159","South Korea" },

            {"488425731191722","Malaysia" },//Off The Wheaten Path In Kuala Lumpur (Gluten Free Tips, Recipes & Findings)
            {"1720098858232675","United Arab Emirates" },//Gluten Free - UAE
            {"687227675922496","Spain" },
            {"383755778784374","Italy" },//Gluten Free Italy
            {"229495282203436","South Africa" },//Living gluten free in South Africa

            {"1025248344200757","Australia" }, //Gluten Free Eating Out Sydney & New South Wales, Australia
            {"9413340041","United Kingdom" },      //Coeliacs Eat Out Too UK

            {"847553335358305","" },//Gluten Free Travel Around the World
            {"292593134198337","" },//Coeliacs Eat Abroad - only back to july
            {"195689771214297","" },//Celiac Travel
            {"798810970466035","" },//Celiac Travel Group

            {"1066858930047711","" },//Coeliacs Eat Out Take Two
            
            {"247208302148491","India" },//Gluten free , Grain free ,healthy living in India

            {"550373421739534","Australia" },//Australia's Gluten & Celiac/Coeliac Support Group
            {"1452094601717166","Australia" },//Gluten Free Melbourne
            {"307872078321","Australia" },//Gluten Free Tasmania
            {"625162559593528","Australia" },//Gluten Free Brisbane
            {"629876021246440","Australia" },//Where Coeliacs Eat - Australia

            // Not active
            {"450713908359721","Cambodia" },//Cambodia
            {"746699194039212","Vietnam" },//Vietnam 
            {"191520127713279","Singapore" },//Singapore
            {"428227140573301","Japan" },//Japan
            {"1587317368127948","Thailand" },//Thailand
            {"403103165372802","Cambodia" },//Cambodia,
            {"361337232353766","Vietnam" }, // not active
            
            {"286367932803894","Cambodia" },//Cambodia Travel
            {"1300758866697297","Cambodia/Vietnam" },//Cambodia & Vietnam Travel Tips
            {"309301445942480","Cambodia" },//Cambodia Travels & Tips
            
            {"342422672937608","Indonesia" },//Gluten Free Bali
            
            {"187020706020686","Australia" },//Gold Coast Gluten Free							public group
            {"1535703166696570","United States" },//Coeliac and Gluten Free York			public group
            
            //searchable gluten groups -
            {"974429813510442","Cambodia" },        //Cambodia travel
            {"147347329055","Cambodia" },           //Cambodia Expats Club
            {"517665179677444","Cambodia" },        //Cambodia travelers
            {"3061458170649280","Cambodia" },       //Siem Reap Expat Connection
            {"161220651105962","Cambodia" },        //Expats & Locals in Kampot & Kep
            
            //{"","" },
        };

        private readonly Dictionary<string, string> _groupIdToCity = new()
        {
            {"3087018218214300","Bali" },//Gluten Free in Bali
            {"422262581142441","Hong Kong" },//Gluten Free in Hong Kong
            {"660915839470807","Ho Chi Minh City" },//Gluten Free Saigon (Ho Chi Minh City)
            {"823200180025057","Hội An " },//Gluten-free Hội An Community
            {"353439621914938","Taipei" }, //Gluten free Taipei
            {"319517678837045","Hanoi" },//Gluten Free Hanoi
            {"1015752345220391","Chiang Mai" },//Gluten Free Chiang Mai

            {"488425731191722","Kuala Lumpur" },//Off The Wheaten Path In Kuala Lumpur (Gluten Free Tips, Recipes & Findings)

            {"1025248344200757","New South Wales" }, //Gluten Free Eating Out Sydney & New South Wales, Australia

            {"1452094601717166","Melbourne" },//Gluten Free Melbourne
            {"307872078321","Tasmania" },//Gluten Free Tasmania
            {"625162559593528","Brisbane" },//Gluten Free Brisbane
            
            
            {"342422672937608","Bali" },//Gluten Free Bali
            
            {"187020706020686","Gold Coast " },//Gold Coast Gluten Free							public group
            {"1535703166696570","New York" },//Coeliac and Gluten Free York			public group
            
            {"3061458170649280","Siem" },       //Siem Reap Expat Connection
        };



        public bool IsGenericGroup(string groupId)
        {
            return string.IsNullOrWhiteSpace(GetCountryName(groupId));
        }

        public string GetCityName(string groupId)
        {
            if (_groupIdToCity.TryGetValue(groupId, out var value))
            {
                return value;
            }
            return "";
        }

        /// <summary>
        /// Get a country name based on what group the message was posted in (groupId)
        /// </summary>
        public string GetCountryName(string groupId)
        {
            try
            {
                if (!_knownGroupIds.ContainsKey(groupId)) Console.WriteLine($"Unknown group :{groupId}");
                return _knownGroupIds[groupId];
            }
            catch (Exception ex)
            {
                Console.WriteLineRed(ex.Message);
                return "NA";
            }

        }

        /// <summary>
        /// Provides some pin filtering based on geo location of the group and pin, 
        /// e.g. why would a pin for the Japan group be located in USA, something went wrong, remove it
        /// </summary>
        public bool IsPinWithinExpectedRange(string groupId, TopicPin pin)
        {
            if (pin.GeoLatitude == null || pin.GeoLongitude == null) return false;
            double geoLatitude = double.Parse(pin.GeoLatitude);
            double geoLongitude = double.Parse(pin.GeoLongitude);

            //29.3786989,-13.2875609
            //27.031709,-18.3669599
            if (groupId == "687227675922496" &&
                (geoLongitude < -18.3669599
                || geoLongitude > -13.2875609
                || geoLatitude < 27.031709
                || geoLatitude > 29.3786989)
                )
            {
                Console.WriteLine($"Rejecting pin for Gran Canaria");
                return false;
            }

            //    { "379994195544478","Japan" },//Gluten-Free in Japan!
            //24.4556439,122.9483518
            //44.2725791,145.3226978
            if (groupId == "379994195544478" &&
                (geoLongitude < 122.9483518
                || geoLongitude > 145.3226978
                || geoLatitude < 24.4556439
                || geoLatitude > 44.2725791)
                )
            {
                Console.WriteLine($"Rejecting pin for Japan");
                return false;
            }

            //{ "1025248344200757","Australia" },

            //{ "361337232353766","Vietnam" },
            //{ "319517678837045","Vietnam" },//Gluten Free Hanoi
            //{ "660915839470807","Vietnam" },//Gluten Free Saigon (Ho Chi Minh City)
            //{ "823200180025057","Vietnam" },//Gluten-free Hội An Community
            //23.506164, 110.599140
            //7.991949, 103.962525
            if ((groupId == "361337232353766"
                || groupId == "319517678837045"
                || groupId == "660915839470807"
                || groupId == "823200180025057") &&
                (geoLongitude < 103
                || geoLongitude > 110.599140
                || geoLatitude < 7.991949
                || geoLatitude > 23.506164)
                )
            {
                Console.WriteLine($"Rejecting pin for Vietnam");
                return false;
            }

            //{ "3087018218214300","Bail" },
            //-0.753594, 129.772972
            //-10.242782, 104.804353
            if (groupId == "3087018218214300" &&
                (geoLongitude < 104.804353
                || geoLongitude > 129.772972
                || geoLatitude < -10.242782
                || geoLatitude > -0.753594)
                )
            {
                Console.WriteLine($"Rejecting pin for Bail");
                return false;
            }

            //{ "1420852834795381","South Korea" },
            //38.555603, 130.072375
            //33.986797, 125.842844
            if (groupId == "1420852834795381" &&
                (geoLongitude < 125.842844
                || geoLongitude > 130.072375
                || geoLatitude < 32.986797
                || geoLatitude > 38.555603)
                )
            {
                Console.WriteLine($"Rejecting pin for South Korea");
                return false;
            }
            //{ "1015752345220391","Thailand" },//Gluten Free Chiang Mai
            //{ "806902313439614","Thailand" },
            //{ "1053129328213251","Thailand" },//Gluten Free Thailand
            //20.461801, 105.474063
            //5.265873, 97.869131
            if ((groupId == "1015752345220391"
                || groupId == "806902313439614"
                || groupId == "1053129328213251"
                ) &&
                (geoLongitude < 97.869131
                || geoLongitude > 105.474063
                || geoLatitude < 5.265873
                || geoLatitude > 20.461801)
                )
            {
                Console.WriteLine($"Rejecting pin for Thailand");
                return false;
            }

            //{ "852980778556330","Fiji" },
            //-9.975805, -168.527795
            //-23.750312, 173.138554
            if (groupId == "852980778556330" &&
                (geoLongitude < -168.527795 // TODO: Wrap problem
                || geoLongitude > 179.138554
                || geoLatitude < -23.750312
                || geoLatitude > -9.975805)
                )
            {
                Console.WriteLine($"Rejecting pin for Fiji");
                return false;
            }
            //{ "353439621914938","Taipei" },
            //25.354796, 122.632103
            //21.812342, 119.509825
            if (groupId == "353439621914938" &&
                (geoLongitude < 119.509825
                || geoLongitude > 122.632103
                || geoLatitude < 21.812342
                || geoLatitude > 25.354796)
                )
            {
                Console.WriteLine($"Rejecting pin for Taipei");
                return false;
            }
            //{ "422262581142441","Hong Kong" },//Gluten Free in Hong Kong
            //22.614259, 114.473388
            //22.009237, 113.810679
            if (groupId == "422262581142441" &&
                (geoLongitude < 113.810679
                || geoLongitude > 114.473388
                || geoLatitude < 22.009237
                || geoLatitude > 22.614259)
                )
            {
                Console.WriteLine($"Rejecting pin for Hong Kong");
                return false;
            }
            //{ "302515126584130","Philippines" },
            //18.885508, 127.466215
            //5.939433, 116.953634
            if (groupId == "302515126584130" &&
                (geoLongitude < 116.953634
                || geoLongitude > 127.466215
                || geoLatitude < 5.939433
                || geoLatitude > 18.885508)
                )
            {
                Console.WriteLine($"Rejecting pin for Philippines");
                return false;
            }
            //{ "182984958515029","Singapore" },//Gluten Free Singapore - Support Group
            //1.502856, 104.110868
            //1.117591, 103.584457
            if (groupId == "182984958515029" &&
                (geoLongitude < 103.584457
                || geoLongitude > 104.110868
                || geoLatitude < 1.117591
                || geoLatitude > 1.502856)
                )
            {
                Console.WriteLine($"Rejecting pin for Singapore");
                return false;
            }

            return true;
        }

    }
}
