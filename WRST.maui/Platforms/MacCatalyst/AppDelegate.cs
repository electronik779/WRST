//#if MACCATALYST
//using AppKit;
//using Foundation;

//namespace WRST.maui
//{
//    [Register("AppDelegate")]
//    public class AppDelegate : MauiUIApplicationDelegate
//    {
//        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

//        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
//        {
//            // Возвращаем false, чтобы приложение не закрывалось автоматически после закрытия последнего окна.
//            // Это даёт нам время показать диалог в событии WillClose окна.
//            return false;
//        }
//    }
//}
//#endif