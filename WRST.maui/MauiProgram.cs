using CommunityToolkit.Maui;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Maui.LifecycleEvents;
#if WINDOWS
    using WinRT.Interop;
    using static Microsoft.UI.Win32Interop;
    using Microsoft.UI.Windowing;
#endif

namespace WRST.maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                    events.AddWindows(windowsLifecycleBuilder =>
                    {
                        windowsLifecycleBuilder.OnWindowCreated(window =>
                        {
                            var handle = WindowNative.GetWindowHandle(window);
                            var id = GetWindowIdFromWindow(handle); // теперь без префикса
                            var appWindow = AppWindow.GetFromWindowId(id);

                             appWindow.Closing += async (s, e) =>
                             {
                                 var mainPage = MainPage.Current;
                                 if (mainPage == null || mainPage.IsSaved)
                                     return;
                             
                                 e.Cancel = true;
                             
                                 bool? shouldSave = await mainPage.DisplayAlertAsync(
                                     "Несохранённые данные",
                                     "Вы хотите сохранить изменения перед выходом?",
                                     "Да", "Нет"
                                 );
                             
                                 if (shouldSave == true)
                                 {
                                     await mainPage.SaveDataAsync();
                                     Application.Current?.Quit();
                                 }
                                 else if (shouldSave == false)
                                 {
                                     Application.Current?.Quit();
                                 }
                             };
                        });
                    });
#endif
#if MACCATALYST
                // Теперь метод AddMacCatalyst будет успешно найден компилятором
                events.AddMacCatalyst(mac => mac
                    .ApplicationShouldTerminateAfterLastWindowClosed(sender => false)
                );
#endif
                })

                .UseSkiaSharp()
                .UseLiveCharts();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
