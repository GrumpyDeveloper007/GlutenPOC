﻿using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data.TopicModel;
using Microsoft.Extensions.Configuration;
using Samwise.Service;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;


namespace Samwise
{
    /// <summary>
    /// Interactive topic processor, ideally this would be handled with AI, but for now we do some basic searching and present
    /// it to the user and allow them time to check and select their own map location, then update the DB
    /// </summary>
    public partial class MainWindow : Window
    {
        private SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor;
        private AIProcessingService _aIProcessingService;
        private TopicsHelper _topicsHelper = new TopicsHelper();
        private PinHelper _pinHelper = new PinHelper();

        private int _index;
        private List<Topic> _topics;
        private string _currentNewUrl;
        private string DBFileName = "D:\\Coding\\Gluten\\Topics.json";


        public MainWindow()
        {
            InitializeComponent();
        }

        private void butStart_Click(object sender, RoutedEventArgs e)
        {
            _index = 0;
            if (_aIProcessingService == null)
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json")
                .Build();

                var settings = config.GetRequiredSection("Values").Get<SettingValues>();

                _topics = _topicsHelper.TryLoadTopics(DBFileName);

                //FixIncorrectPins();

                var nlp = new NaturalLanguageProcessor(settings.AIEndPoint, settings.AIApiKey);
                _seleniumMapsUrlProcessor = new SeleniumMapsUrlProcessor();
                _seleniumMapsUrlProcessor.Start();

                _aIProcessingService = new AIProcessingService(nlp, _seleniumMapsUrlProcessor);


                if (_topics == null)
                {
                    txtMessage.Text = "!! No topics available";
                    return;
                }

                ProcessNext();
            }
        }

        private void FixIncorrectPins()
        {
            //https://www.google.com/maps/place/Otsuna+Sushi/@35.7281855,139.7452636,17z/data=!4m6!3m5!1s0x60188dbbed184005:0x88ffa854362ddcfd!8m2!3d35.7278459!4d139.7459932!16s%2Fg%2F1tsbm3f7!5m1!1e4?entry=ttu&g_ep=EgoyMDI0MTAyMS4xIKXMDSoASAFQAw%3D%3D
            var incorrectPins = "";
            for (int i = 0; i < _topics.Count; i++)
            {
                for (int t = 0; t < _topics[i].UrlsV2.Count; t++)
                {
                    if (_topics[i].UrlsV2[t].Pin != null)
                    {
                        var geolat = "";
                        var geolong = "";
                        _pinHelper.TryGetLocationFromDataParameter(_topics[i].UrlsV2[t].Url, ref geolat, ref geolong);

                        if (geolong == "" || geolong == "") continue;
                        if (_topics[i].UrlsV2[t].Pin.GeoLatatude != geolat
                        || _topics[i].UrlsV2[t].Pin.GeoLongitude != geolong)
                        {
                            incorrectPins += $"{_topics[i].UrlsV2[t].Pin.Label}\r\n";
                            _topics[i].UrlsV2[t].Pin.GeoLatatude = geolat;
                            _topics[i].UrlsV2[t].Pin.GeoLongitude = geolong;
                        }
                    }
                }
            }
            _topicsHelper.SaveTopics(DBFileName, _topics);
        }

        private void ProcessNext()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            _currentNewUrl = null;
            var restaurantName = "";
            while (_currentNewUrl == null && _index < _topics.Count)
            {
                _currentNewUrl = _aIProcessingService.ProcessTopic(_topics[_index], ref restaurantName);
                if (_currentNewUrl == null)
                {
                    _index++;
                }
            }
            ShowData(_index);

            if (_currentNewUrl == null)
            {
                txtUrl.Text = "";
                txtMessage.Text = "";
            }
            else
            {
                txtUrl.Text = _currentNewUrl;
            }
            txtSearchToken.Text = restaurantName;
            Mouse.OverrideCursor = null;

        }

        private void ShowData(int i)
        {
            if (_topics == null) return;
            if (i == _topics.Count) return;

            txtMessage.Text = _topics[i].Title;
            var tokens = "";
            if (_topics[i].AiTitleInfoV2 != null)
            {
                foreach (var item in _topics[i].AiTitleInfoV2)
                {
                    tokens += $"{item.Category} : {item.Text}\n";
                }
            }
            txtTokenList.Text = tokens;
            txtCounter.Text = $"{_index}/{_topics.Count}";
        }

        private void butAccept_Click(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            _aIProcessingService.UpdatePinList(_currentNewUrl, _topics[_index], ref counter);
            _topicsHelper.SaveTopics(DBFileName, _topics);
            _index++;
            ProcessNext();
        }

        private void butReject_Click(object sender, RoutedEventArgs e)
        {
            _topicsHelper.SaveTopics(DBFileName, _topics);
            _index++;
            ProcessNext();
        }

        private void butAddToIgnore_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void butFacebook_Click(object sender, RoutedEventArgs e)
        {
            var url = _topics[_index].FacebookUrl;
            BrowserHelper.OpenUrl(url);
        }
    }
}