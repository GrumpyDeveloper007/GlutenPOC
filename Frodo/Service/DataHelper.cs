using AutoMapper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Data;
using Gluten.Data.TopicModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    internal static class DataHelper
    {
        public static void CleanTopics(List<PinTopic> pins)
        {
            foreach (var item in pins)
            {
                item.Topics?.Clear();
            }
        }

        public static void RemoveEmptyPins(List<PinTopic> pins)
        {
            for (int i = pins.Count - 1; i >= 0; i--)
            {
                if (pins[i].Topics != null && pins[i].Topics.Count == 0)
                {
                    pins.RemoveAt(i);
                }
            }
        }

        public static bool IsPinWithinExpectedRange(string groupId, double geoLatatude, double geoLongitude)
        {
            //    { "379994195544478","Japan" },//Gluten-Free in Japan!
            //24.4556439,122.9483518
            //44.2725791,145.3226978
            if (groupId == "379994195544478" &&
                (geoLongitude < 122.9483518
                || geoLongitude > 145.3226978
                || geoLatatude < 24.4556439
                || geoLatatude > 44.2725791)
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
                || geoLatatude < 7.991949
                || geoLatatude > 23.506164)
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
                || geoLatatude < -10.242782
                || geoLatatude > -0.753594)
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
                || geoLatatude < 33.986797
                || geoLatatude > 38.555603)
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
    || geoLatatude < 5.265873
    || geoLatatude > 20.461801)
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
                || geoLatatude < -23.750312
                || geoLatatude > -9.975805)
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
                || geoLatatude < 21.812342
                || geoLatatude > 25.354796)
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
                || geoLatatude < 22.009237
                || geoLatatude > 22.614259)
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
                || geoLatatude < 5.939433
                || geoLatatude > 18.885508)
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
                || geoLatatude < 1.117591
                || geoLatatude > 1.502856)
                )
            {
                Console.WriteLine($"Rejecting pin for Singapore");
                return false;
            }

            return true;
        }

        public static bool IsInList(List<AiVenue> venues, AiVenue venue, int newVenueIndex)
        {
            for (int i = 0; i < venues.Count; i++)
            {
                AiVenue? item = venues[i];
                if (i != newVenueIndex
                    && item.Pin != null && venue.Pin != null
                    && item.Pin.GeoLongitude == venue.Pin.GeoLongitude
                    && item.Pin.GeoLatatude == venue.Pin.GeoLatatude)
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddIfNotExists(List<PinTopic> pins, PinTopic? matchingPinTopic,
            PinLinkInfo topicToAdd, TopicPin? pinToAdd, TopicPinCache? cachePin)
        {
            if (pinToAdd == null) return;
            if (pinToAdd == null) return;
            if (string.IsNullOrWhiteSpace(pinToAdd.GeoLatatude)) return;
            if (string.IsNullOrWhiteSpace(pinToAdd.GeoLongitude)) return;

            // if pin not found, add it to the list
            if (matchingPinTopic == null)
            {
                var newPin = new PinTopic
                {
                    GeoLatatude = double.Parse(pinToAdd.GeoLatatude),
                    GeoLongitude = double.Parse(pinToAdd.GeoLongitude),
                    Label = pinToAdd.Label,
                    Topics = [topicToAdd],
                };
                pins.Add(newPin);
                if (cachePin != null && !string.IsNullOrWhiteSpace(cachePin.MapsUrl))
                {
                    newPin.MapsLink = cachePin.MapsUrl;
                }
                return;
            }
            // dont add duplicates
            matchingPinTopic.Topics ??= [];
            if (cachePin != null && !string.IsNullOrWhiteSpace(cachePin.MapsUrl))
            {
                matchingPinTopic.MapsLink = cachePin.MapsUrl;
            }
            foreach (var existingTopic in matchingPinTopic.Topics)
            {
                if (existingTopic.NodeID == topicToAdd.NodeID)
                {
                    return;
                }
            }
            matchingPinTopic.Topics.Add(topicToAdd);
        }

        public static PinTopic? IsPinInList(TopicPin? topicPin, List<PinTopic> pins)
        {
            foreach (var pin in pins)
            {
                if (topicPin != null &&
                    !string.IsNullOrWhiteSpace(topicPin.GeoLatatude) &&
                    !string.IsNullOrWhiteSpace(topicPin.GeoLongitude) &&
                    pin.GeoLatatude == double.Parse(topicPin.GeoLatatude) && pin.GeoLongitude == double.Parse(topicPin.GeoLongitude))
                {
                    return pin;
                }
            }
            return null;
        }


    }
}
