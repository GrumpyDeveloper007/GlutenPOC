using Gluten.Core.DataProcessing.Service;
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
    /// TODO: Clean up/remove code, this app isnt really needed
    /// 
    /// Interactive topic processor, ideally this would be handled with AI, but for now we do some basic searching and present
    /// it to the user and allow them time to check and select their own map location, then update the DB
    /// 
    /// Note: This has become more of a sandbox to create the code that can be used without user interaction
    /// </summary>
    public partial class MainWindow : Window
    {
        private SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor;
        private AIProcessingService _aIProcessingService;
        private TopicsHelper _topicsHelper = new TopicsHelper();
        private AiPinGeneration _aiPinGeneration;

        private int _index;
        private int _mapIndex;
        private List<DetailedTopic> _topics;
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

                var dbLoader = new DatabaseLoaderService();

                //var nlp = new NaturalLanguageProcessor(settings.AIEndPoint, settings.AIApiKey);
                var pinHelper = dbLoader.GetPinHelper();
                var selenium = new SeleniumMapsUrlProcessor();
                var mapper = new MappingService();
                var fbGroupService = new FBGroupService();
                //var nlp = new NaturalLanguageProcessor(settings.AIEndPoint, settings.AIApiKey);
                _seleniumMapsUrlProcessor = new SeleniumMapsUrlProcessor();

                _aIProcessingService = new AIProcessingService(_seleniumMapsUrlProcessor, pinHelper, mapper);
                _aiPinGeneration = new AiPinGeneration(_aIProcessingService, mapper, fbGroupService);


                if (_topics == null)
                {
                    txtMessage.Text = "!! No topics available";
                    return;
                }
                _index = 315;
                ShowData(_index);

                //ProcessNext();
            }
        }


        private void ProcessNext()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            _currentNewUrl = null;

            while (_currentNewUrl == null && _index < _topics.Count)
            {
                while (_currentNewUrl == null && _topics[_index].AiVenues != null && _mapIndex < _topics[_index].AiVenues.Count)
                {
                    ShowData(_index);

                    _currentNewUrl = _aiPinGeneration.GetMapPinForPlaceName(_topics[_index].AiVenues[_mapIndex]);
                    // Note: Remove line below to pause processing and review
                    _currentNewUrl = null;

                    if (_currentNewUrl == null) _mapIndex++;
                }

                // Save after every update
                _topicsHelper.SaveTopics(DBFileName, _topics);

                if (_currentNewUrl == null)
                {
                    _index++;
                    _mapIndex = 0;
                }
            }

            // TODO: Refactor
            //while (_currentNewUrl == null && _index < _topics.Count)
            //{
            //    _currentNewUrl = _aIProcessingService.ProcessTopic(_topics[_index], ref restaurantName);
            //    if (_currentNewUrl == null)
            //    {
            //        _index++;
            //    }
            //}

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
            //txtSearchToken.Text = restaurantName;
            Mouse.OverrideCursor = null;
        }

        private void ShowData(int i)
        {
            if (_topics == null) return;
            if (i == _topics.Count) return;

            txtMessage.Text = _topics[i].Title;
            var tokens = "";
            /*if (_topics[i].AiTitleInfoV2 != null)
            {
                foreach (var item in _topics[i].AiTitleInfoV2)
                {
                    tokens += $"{item.Category} : {item.Text}\n";
                }
            }*/
            if (_topics[i].AiVenues != null)
            {
                foreach (var item in _topics[i].AiVenues)
                {
                    item.Address = _aiPinGeneration.FilterAddress(item.Address?.Trim());

                    if (!_aiPinGeneration.IsInPlaceNameSkipList(item.PlaceName))
                    {
                        tokens += $"{item.PlaceName} : {item.Address}\n";
                    }
                }
            }
            txtTokenList.Text = tokens;
            txtCounter.Text = $"{_index}/{_topics.Count}";
        }

        private void butAccept_Click(object sender, RoutedEventArgs e)
        {
            //int counter = 0;
            //_aIProcessingService.UpdatePinList(_currentNewUrl, _topics[_index], ref counter);

            var pin = _aIProcessingService.GetPinFromCurrentUrl(true);
            if (pin != null)
            {
                // Add pin to AiGenerated
                _topics[_index].AiVenues[_mapIndex].Pin = pin;
            }
            _topicsHelper.SaveTopics(DBFileName, _topics);
            _mapIndex++;
            //_index++;
            ProcessNext();
        }

        private void butReject_Click(object sender, RoutedEventArgs e)
        {
            _topicsHelper.SaveTopics(DBFileName, _topics);
            _mapIndex++;
            //_index++;
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

        private void butLeft_Click(object sender, RoutedEventArgs e)
        {
            if (_index > 0)
            {
                _index--;
                ShowData(_index);
            }
        }

        private void butRight_Click(object sender, RoutedEventArgs e)
        {
            if (_index < _topics.Count)
            {
                _index++;
                ShowData(_index);
            }
        }
    }
}
