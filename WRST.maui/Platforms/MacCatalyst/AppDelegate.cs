using Foundation;
using ObjCRuntime;

namespace WRST.maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    static AppDelegate()
    {
        // Динамически регистрируем метод из AppKit через Objective-C рантайм
        var block = new BlockLiteral();
        block.SetupBlockUnsafe(TargetMethod, null);
        
        // Селектор оригинального метода из NSApplicationDelegate
        var selector = new Selector("applicationShouldTerminateAfterLastWindowClosed:");
        
        // Внедряем поведение напрямую в класс делегата нативного приложения
        Class.Get("NSApplication").GetMethod(selector).SetImplementation(ref block);
    }

    // Сигнатура метода должна возвращать нативный bool (byte/bool в Obj-C)
    [MonoPInvokeCallback(typeof(Func<IntPtr, IntPtr, bool>))]
    private static bool TargetMethod(IntPtr self, IntPtr sender)
    {
        return false; // Предотвращает автоматическое завершение приложения
    }
}
