#if MACCATALYST
using AppKit;
using Foundation;

namespace WRST.maui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        // Используем global:: для явного указания пространства имён
        public override bool ApplicationShouldTerminateAfterLastWindowClosed(global::AppKit.NSApplication sender)
        {
            return false; // предотвращает автоматическое завершение
        }
    }
}
#endif