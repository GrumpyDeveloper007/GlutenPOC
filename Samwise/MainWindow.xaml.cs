using Gluten.Core.DataProcessing.Service;
using Gluten.Core.Service;
using Gluten.Data.TopicModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace Samwise
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SeleniumMapsUrlProcessor _seleniumMapsUrlProcessor;
        private AIProcessingService _aIProcessingService;
        private TopicsHelper _topicsHelper = new TopicsHelper();

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

                var nlp = new NaturalLanguageProcessor(settings.AIEndPoint, settings.AIApiKey);
                _seleniumMapsUrlProcessor = new SeleniumMapsUrlProcessor();
                _seleniumMapsUrlProcessor.Start();

                _aIProcessingService = new AIProcessingService(nlp, _seleniumMapsUrlProcessor);

                _topics = _topicsHelper.TryLoadTopics(DBFileName);

                if (_topics == null)
                {
                    txtMessage.Text = "!! No topics available";
                    return;
                }

                ProcessNext();
            }
        }

        private void ProcessNext()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            _currentNewUrl = null;
            var restaurantName = "";
            while (_currentNewUrl == null)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void butFacebook_Click(object sender, RoutedEventArgs e)
        {
            var url = _topics[_index].FacebookUrl;
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
