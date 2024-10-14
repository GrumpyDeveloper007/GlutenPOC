using GlutenApp.Services;
using Maui.GoogleMaps;

namespace GlutenApp.Pages;

public partial class MainPage : ContentPage
{
    private GeoLocationService _geoLocationService = new GeoLocationService();
    private TestDataLoaderService _testDataLoaderService = new TestDataLoaderService();
    private Placemark _placemark;
    private Pin _selectedPin;
    private Position _selectedPosition;

    public MainPage()
    {
        InitializeComponent();
    }

    private void Pin_MarkerClicked(object? sender, PinClickedEventArgs e)
    {
        Console.WriteLine($"Pin Click {e?.Pin?.Label}");
        AddPinBtn.Text = $"{e?.Pin?.Label}";
        _selectedPin = e?.Pin;
    }


    private async void OnCounterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CardPage(_placemark.CountryCode));


        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        _placemark = await _geoLocationService.GetLocationCountryCode();
        Location.Text = _placemark.CountryCode;
        await _testDataLoaderService.LoadMauiAsset(map);
        map.PinClicked += Pin_MarkerClicked;


        var location = new Position(1.4798873, 103.7647029);
        MapSpan mapSpan = MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(0.444));
        map.MoveToRegion(mapSpan);
        var polyLine = new Polyline();
        var polygon = new Polygon();
        var circle = new Circle();
    }

    private void map_MapClicked(object sender, MapClickedEventArgs e)
    {
        Console.WriteLine($"MapClick: {e.Point.Latitude}, {e.Point.Longitude}");
        _selectedPin = null;
        _selectedPosition = e.Point;
    }

    private async void AddPinBtn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PinPage(_selectedPin, _selectedPosition));
    }
}

