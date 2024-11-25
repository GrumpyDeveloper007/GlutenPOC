using GlutenApp.Pages;

namespace GlutenApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("MainPage", typeof(MainPage));
        Routing.RegisterRoute("CardPage", typeof(CardPage));
        Routing.RegisterRoute("PinPage", typeof(PinPage));
    }
}
