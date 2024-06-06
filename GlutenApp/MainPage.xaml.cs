using GlutenApp.Services;
//using Microsoft.Maui.Controls.Maps;
//using Microsoft.Maui.Devices.Sensors;
//using Microsoft.Maui.Maps;
using Maui.GoogleMaps;
using Newtonsoft.Json;

namespace GlutenApp;

public partial class MainPage : ContentPage
{
    int count = 0;
    private GeoLocationService _geoLocationService = new GeoLocationService();
    private Placemark _placemark;

    public MainPage()
    {
        InitializeComponent();
    }

    private async Task LoadMauiAsset()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("bookmarks.json");
        using var reader = new StreamReader(stream);

        var contents = reader.ReadToEnd();
        dynamic test = JsonConvert.DeserializeObject(contents);
        //dynamic stuff = JObject.Parse(contents);
        var mapName = test[0];
        var locations = test[1][0];
        var locations2 = locations[17];

        var pins = new List<Pin>();


        foreach (var item in locations2)
        {
            var geo = item[11][0][6];
            var geoLatatude = geo[0][0][0];
            var geoLongitude = geo[0][0][1];
            var geoName = item[11][4][4];

            //Console.WriteLine(geoLatatude + "," + geoLongitude + "," + geoName);

            var pin = new Pin
            {
                Label = geoName,
                Address = geoName,
                Type = PinType.Place,
                Position = new Position(geoLatatude, geoLongitude),
                //Location = new Location(geoLatatude, geoLongitude),
            };
            //pin.MarkerClicked += Pin_MarkerClicked;
            map.Pins.Add(pin);
            //pins.Add(pin);

        }
        map.PinClicked += Pin_MarkerClicked;
        var i = map.Pins;
        //map.ItemsSource = pins;
        var i2 = map.Pins;

    }

    private void Pin_MarkerClicked(object? sender, PinClickedEventArgs e)
    {
        Console.WriteLine($"Pin Click {e?.Pin?.Label}");
        AddPinBtn.Text = $"{e?.Pin?.Label}";
    }


    private async void OnCounterClicked(object sender, EventArgs e)
    {
        await LoadMauiAsset();

        await Navigation.PushAsync(new CardView(_placemark.CountryCode));

        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        _placemark = await _geoLocationService.GetLocationCountryCode();
        Location.Text = _placemark.CountryCode;
        await LoadMauiAsset();

        //var location = new Location(1.4798873, 103.7647029);
        var location = new Position(1.4798873, 103.7647029);
        MapSpan mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(0.444));
        map.MoveToRegion(mapSpan);
        //var elements = map.MapElements;
        var polyLine = new Polyline();
        var polygon = new Polygon();
        var circle = new Circle();

    }

    private void map_MapClicked(object sender, MapClickedEventArgs e)
    {
        Console.WriteLine($"MapClick: {e.Point.Latitude}, {e.Point.Longitude}");
        //Console.WriteLine($"MapClick: {e.Location.Latitude}, {e.Location.Longitude}");
    }

    private void AddPinBtn_Clicked(object sender, EventArgs e)
    {

    }
}

