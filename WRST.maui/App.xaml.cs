#if MACCATALYST
    using AppKit;
#endif

namespace WRST.maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
            //var window = new Window(new MainPage());

            // Установка размеров
            window.Width = 800;
            window.Height = 800;

#if MACCATALYST
            window.Created += (s, e) =>
            {
                var nativeWindow = window.Handler?.PlatformView as NSWindow;
                if (nativeWindow != null)
                {
                    nativeWindow.WillClose += async (sender, args) =>
                    {
                        var mainPage = MainPage.Current;
                        if (mainPage == null || mainPage.IsSaved)
                            return;

                        var alert = new NSAlert()
                        {
                            AlertStyle = NSAlertStyle.Warning,
                            MessageText = "Несохранённые данные",
                            InformativeText = "Вы хотите сохранить изменения перед выходом?",
                        };
                        alert.AddButton("Да");
                        alert.AddButton("Нет");

                        var result = alert.RunModal();

                        if (result == 1000) // "Да"
                        {
                            await mainPage.SaveDataAsync();
                            NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication.Delegate);
                        }
                        else if (result == 1001) // "Нет"
                        {
                            NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication.Delegate);
                        }
                    };
                }
            };
#endif
            return window;
        }
    }
}