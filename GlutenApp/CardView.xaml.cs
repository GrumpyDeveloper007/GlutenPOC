using GlutenApp.Services;

namespace GlutenApp;

public partial class CardView : ContentPage
{
    private CardLookupService cardLookupService = new CardLookupService();
    private string _countryCode;

    public CardView(string countryCode)
    {
        _countryCode = countryCode;
        InitializeComponent();
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        CardLanguage.Text = _countryCode;
        CardImage.Source = cardLookupService.GetCardPath(_countryCode);
    }
}