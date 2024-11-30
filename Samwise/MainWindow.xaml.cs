using AutoMapper;
using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Helper;
using Gluten.Core.LocationProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data.MapsModel;
using Gluten.Data.PinCache;
using Microsoft.Extensions.Configuration;
using Samwise.Service;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly TopicsHelper _topicsHelper = new();
        private readonly MapsMetaExtractorService _mapsMetaExtractorService = new();
        private readonly MapPinService _mapPinService;
        private readonly PinHelper _pinHelper;
        private readonly DatabaseLoaderService _dbLoader;

        private int _index;
        private readonly List<GMapsPin> _pins;

        public MainWindow()
        {
            InitializeComponent();

            _dbLoader = new DatabaseLoaderService();
            _pinHelper = new PinHelper();
            _pins = _dbLoader.LoadGMPins();
            var pinCache = _dbLoader.GetPinCache();
            var selenium = new SeleniumMapsUrlProcessor();
            var geoService = new GeoService();

            _mapPinService = new MapPinService(selenium, _pinHelper, pinCache, geoService, new MapsMetaExtractorService());

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json")
                .Build();

            var settings = config.GetRequiredSection("Values").Get<SettingValues>();

            _mapPinService.GoToUrl("https://www.google.com/maps/search/gluten/@34.3975011,132.4539367,2422m/data=!3m1!1e3!4m2!2m1!6e5?entry=ttu&g_ep=EgoyMDI0MTExOS4yIKXMDSoASAFQAw%3D%3D");
        }

        private void ButStart_Click(object sender, RoutedEventArgs e)
        {
            _index = 0;
        }


        private void ShowData(int i)
        {
            if (i == _pins.Count) return;
            txtCounter.Text = $"{_index}/{_pins.Count}";
        }

        private void ButFacebook_Click(object sender, RoutedEventArgs e)
        {
            var url = "";
            BrowserHelper.OpenUrl(url);
        }

        private void ButLeft_Click(object sender, RoutedEventArgs e)
        {
            if (_index > 0)
            {
                _index--;
                ShowData(_index);
            }
        }

        private void ButRight_Click(object sender, RoutedEventArgs e)
        {
            if (_index < _pins.Count)
            {
                _index++;
                ShowData(_index);
            }
        }

        private void ButCaptureInfo_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var html = _mapPinService.GetFirstLabelInnerHTML();
            var root = new LabelNode();
            if (!string.IsNullOrWhiteSpace(html))
            {
                _mapsMetaExtractorService.TraverseHtml(html, root);

                for (int i = 0; i < root.Child.Count; i++)
                {
                    if (root.Child[i].ResultsNode)
                    {
                        foreach (var item in root.Child[i].Child)
                        {
                            if (item.Child.Count > 0)
                            {
                                var placeName = item.Child[0].Name;
                                var mapsUrl = item.Child[0].Href;
                                var mapPin = _mapPinService.TryToGenerateMapPin(mapsUrl, placeName, "");
                                var comment = _mapsMetaExtractorService.GetComment(item.InnerHtml);
                                var restaurantType = _mapsMetaExtractorService.GetRestaurantType(item.InnerHtml);

                                if (mapsUrl != null)
                                {
                                    var pin = new GMapsPin
                                    {
                                        Label = placeName,
                                        PlaceName = placeName,
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
                                    else if (item.Child.Count == 2)
                                    {
                                        pin.Stars = item.Child[1].Name;
                                    }
                                    else if (item.Child.Count == 3)
                                    {
                                        pin.Stars = item.Child[1].Name;
                                    }
                                    else
                                    {
                                        Console.WriteLine("No reviews or price guide");
                                    }
                                    GPinHelper.TryAddPin(_pins, pin);
                                }
                                else
                                {
                                    Console.WriteLine("Unknown entry");
                                }
                            }

                        }

                    }
                }

                //var placeNames = _mapPinService.GetMapPlaceNames();

                _dbLoader.SaveGMPins(_pins);
            }
            Mouse.OverrideCursor = null;
        }
    }
}
