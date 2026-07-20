#if MACCATALYST
using AppKit;
using Foundation;

namespace WRST.maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    // Статический конструктор вызывается при старте приложения на Mac
    static AppDelegate()
    {
        // Подменяем делегат нативного NSApplication на наш кастомный класс
        NSApplication.SharedApplication.Delegate = new MacCatalystAppDelegate();
    }
}

// Нативный класс-делегат из мира AppKit (macOS)
public class MacCatalystAppDelegate : NSApplicationDelegate
{
    // Предотвращает автоматическое закрытие процесса при закрытии последнего окна
    public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
    {
        return false; 
    }
}
#endif
