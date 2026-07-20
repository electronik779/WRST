using AppKit;
using Foundation;
using UIKit;

namespace WRST.maui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
        {
            // Возвращаем false, чтобы приложение не закрывалось автоматически после закрытия последнего окна.
            // Это даёт нам время показать диалог.
            return false;
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Этот метод вызывается перед завершением работы приложения.
            // Здесь можно выполнить принудительное сохранение, если пользователь не успел ответить.
            base.WillTerminate(notification);
        }
    }
}