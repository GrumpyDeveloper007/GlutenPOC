using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Data.MapsModel;
using Microsoft.Extensions.Configuration;
using Samwise.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static Gluten.Core.LocationProcessing.Service.MapsMetaExtractorService;


namespace Samwise
{
    /// <summary>
    /// Interactive google maps helper tool, used to extract information from searches 
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MapsMetaExtractorService _mapsMetaExtractorService = new();
        private readonly MapPinService _mapPinService;
        private readonly DatabaseLoaderService _dbLoader;

        private int _index;
        private readonly Dictionary<string, GMapsPin> _pins;

        public MainWindow()
        {
            InitializeComponent();

            _dbLoader = new DatabaseLoaderService();
            var pins = _dbLoader.LoadGMPins();
            _pins = new Dictionary<string, GMapsPin>();
            foreach (var pin in pins)
                _pins.Add(pin.GeoLatitude + ":" + pin.GeoLongitude, pin);
            txtMessage.Text = _pins.Count().ToString();
            var pinCache = _dbLoader.GetPinCache();
            var selenium = new SeleniumMapsUrlProcessor();
            var geoService = new GeoService();

            _mapPinService = new MapPinService(selenium, pinCache, geoService, new MapsMetaExtractorService());

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json")
                .Build();

            var settings = config.GetRequiredSection("Values").Get<SettingValues>();

            _mapPinService.GoToUrl("https://www.google.com/maps/search/gluten/@34.3975011,132.4539367,2422m/data=!3m1!1e3!4m2!2m1!6e5?entry=ttu&g_ep=EgoyMDI0MTExOS4yIKXMDSoASAFQAw%3D%3D");
        }

        private void ShowData(int i)
        {
            if (i == _pins.Count) return;
            txtCounter.Text = $"{_index}/{_pins.Count}";
        }

        private void ButCaptureInfo_Click(object sender, RoutedEventArgs e)
        {
            var html = _mapPinService.GetFirstLabelInnerHTML();
            if (string.IsNullOrWhiteSpace(html)) return;

            Mouse.OverrideCursor = Cursors.Wait;
            var root = new LabelNode();
            _mapsMetaExtractorService.TraverseHtml(html, root);

            for (int i = 0; i < root.Child.Count; i++)
            {
                if (!root.Child[i].ResultsNode) continue;

                foreach (var item in root.Child[i].Child)
                {
                    if (item.Child.Count == 0) continue;
                    var placeName = item.Child[0].Name;
                    var mapsUrl = item.Child[0].Href;

                    if (string.IsNullOrWhiteSpace(mapsUrl))
                    {
                        Console.WriteLine("Unknown entry");
                        continue;
                    }
                    //var mapPin = _mapPinService.TryToGenerateMapPin(mapsUrl, placeName, "");
                    var mapPin = PinHelper.GenerateMapPin(mapsUrl, placeName, "");
                    if (mapPin == null) continue;
                    var spans = _mapsMetaExtractorService.GetSpanNodes(item.InnerHtml);
                    var comment = _mapsMetaExtractorService.GetComment(spans);
                    var restaurantType = _mapsMetaExtractorService.GetRestaurantType(spans);
                    var pin = new GMapsPin
                    {
                        Label = placeName,
                        RestaurantType = restaurantType,
                        MapsUrl = mapsUrl,
                        GeoLatitude = mapPin.GeoLatitude,
                        GeoLongitude = mapPin.GeoLongitude,
                        Comment = comment
                    };
                    if (item.Child.Count == 4)
                    {
                        pin.Stars = item.Child[1].Name;
                        pin.Price = item.Child[2].Name;
                    }
                    else if (item.Child.Count == 2 || item.Child.Count == 3)
                    {
                        pin.Stars = item.Child[1].Name;
                    }
                    else
                    {
                        Console.WriteLine("No reviews or price guide");
                    }
                    GPinHelper.TryAddPin(_pins, pin);
                }
            }

            var placeNames = _mapPinService.GetMapPlaceNames();

            txtMessage.Text = _pins.Count().ToString();
            _dbLoader.SaveGMPins(_pins.Values.ToList());

            Mouse.OverrideCursor = null;
        }
    }
}
