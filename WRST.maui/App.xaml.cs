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

            // Этот блок будет работать на macOS (Mac Catalyst) в .NET 10
#if MACCATALYST
        window.Deactivating += async (s, e) =>
        {
            var mainPage = MainPage.Current;
            if (mainPage == null || mainPage.IsSaved)
                return;

            // На Mac Catalyst мы перехватываем деактивацию/закрытие окна.
            // Примечание: Для полной отмены закрытия в MAUI на Mac используется 
            // вызов программного предотвращения закрытия через Shell/Application методы,
            // но в .NET 10 проще всего использовать механизм всплывающего окна на MainPage.
            
            // Получаем доступ к UI для вывода диалога
            bool? shouldSave = await mainPage.DisplayAlertAsync(
                "Несохранённые данные",
                "Вы хотите сохранить изменения перед выходом?",
                "Да", "Нет"
            );

            if (shouldSave == true)
            {
                await mainPage.SaveDataAsync();
                Current?.Quit(); // Закрываем приложение после сохранения
            }
            else if (shouldSave == false)
            {
                Current?.Quit(); // Закрываем без сохранения
            }
        };
#endif

            return window;
        }
    }
}
