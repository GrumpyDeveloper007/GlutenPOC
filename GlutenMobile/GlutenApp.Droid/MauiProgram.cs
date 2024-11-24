using Maui.GoogleMaps.Hosting;

namespace GlutenApp.Droid;

public static class MauiProgram
{


    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseSharedMauiApp();
        var platformConfig = new Maui.GoogleMaps.Android.PlatformConfig
        {
            BitmapDescriptorFactory = new CachingNativeBitmapDescriptorFactory()
        };

        builder.UseGoogleMaps(platformConfig);

        return builder.Build();
    }
}
