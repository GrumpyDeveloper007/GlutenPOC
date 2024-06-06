using Microsoft.Extensions.Logging;

namespace GlutenApp;

public static class MauiProgramExtensions
{
    public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
    {
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
        //.UseMauiMaps();
        ;

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder;
    }
}
