using GlutenApp.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;

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

    private async void OnCounterClicked(object sender, EventArgs e)
    {

        await Navigation.PushAsync(new CardView(_placemark.CountryCode));

        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        Pin pin = new Pin
        {
            Label = "Santa Cruz",
            Address = "The city with a boardwalk",
            Type = PinType.Place,
            Location = new Location(36.9628066, -122.0194722)
        };
        map.Pins.Add(pin);

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        _placemark = await _geoLocationService.GetLocationCountryCode();
        Location.Text = _placemark.CountryCode;

    }
}

