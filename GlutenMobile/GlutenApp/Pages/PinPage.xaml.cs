using Maui.GoogleMaps;

namespace GlutenApp.Pages;

public partial class PinPage : ContentPage
{
    private Pin _selectedPin;
    private Position _selectedPosition;

    public PinPage(Pin pin, Position position)
    {
        InitializeComponent();
        _selectedPin = pin;
        _selectedPosition = position;
    }
}