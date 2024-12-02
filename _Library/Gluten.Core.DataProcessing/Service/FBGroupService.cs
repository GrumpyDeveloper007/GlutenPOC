// Ignore Spelling: geo

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
    public class FBGroupService
    {
        private readonly Dictionary<string, string> _knownGroupIds = new()
        {
            {"379994195544478","Japan" },//Gluten-Free in Japan!
            {"1025248344200757","Australia" },
            {"361337232353766","Vietnam" },
            {"806902313439614","Thailand" },
            {"3087018218214300","Indonesia" },
            {"1420852834795381","South Korea" },
            {"1015752345220391","Thailand" },//Gluten Free Chiang Mai
            {"852980778556330","Fiji" },
            {"353439621914938","Taiwan" },
            {"319517678837045","Vietnam" },//Gluten Free Hanoi
            {"660915839470807","Vietnam" },//Gluten Free Saigon (Ho Chi Minh City)
            {"823200180025057","Vietnam" },//Gluten-free Hội An Community
            {"422262581142441","China" },//Gluten Free in Hong Kong
            {"302515126584130","Philippines" },
            {"1053129328213251","Thailand" },//Gluten Free Thailand
            {"182984958515029","Singapore" },//Gluten Free Singapore - Support Group
            { "769136475365475","" }
//            {"","" },
        };

        public string GetCountryName(string groupId)
        {
            return _knownGroupIds[groupId];
        }


        /// <summary>
        /// Provides some pin filtering based on geo location of the group and pin, 
        /// e.g. why would a pin for the Japan group be located in USA, something went wrong, remove it
        /// </summary>
        public static bool IsPinWithinExpectedRange(string groupId, double geoLatitude, double geoLongitude)
        {
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
                (geoLongitude < 103.962525
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
                || geoLatitude < 33.986797
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
                || geoLongitude > 173.138554
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
