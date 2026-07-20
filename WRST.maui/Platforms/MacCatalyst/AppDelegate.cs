using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace WRST.maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    // Импортируем оригинальные функции среды выполнения Objective-C
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string className);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "class_getInstanceMethod")]
    private static extern IntPtr class_getInstanceMethod(IntPtr cls, IntPtr sel);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "method_setImplementation")]
    private static extern IntPtr method_setImplementation(IntPtr method, ref BlockLiteral block);

    static AppDelegate()
    {
        // Получаем указатель на класс NSApplication из AppKit
        IntPtr nsAppClass = objc_getClass("NSApplication");
        if (nsAppClass == IntPtr.Zero) return;

        // Создаем блок с нашей кастомной логикой
        var block = new BlockLiteral();
        block.SetupBlockUnsafe(TargetMethod, null);

        // Получаем селектор метода управления жизненным циклом окон
        var selector = new Selector("applicationShouldTerminateAfterLastWindowClosed:");
        
        // Находим сам метод внутри класса
        IntPtr method = class_getInstanceMethod(nsAppClass, selector.Handle);
        if (method != IntPtr.Zero)
        {
            // Перезаписываем реализацию метода на нашу собственную функцию
            method_setImplementation(method, ref block);
        }
    }

    // Эта функция будет вызвана операционной системой macOS при закрытии окон
    [MonoPInvokeCallback(typeof(Func<IntPtr, IntPtr, bool>))]
    private static bool TargetMethod(IntPtr self, IntPtr sender)
    {
        return false; // Запрещаем закрытие процесса приложения
    }
}
