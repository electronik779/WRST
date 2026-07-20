namespace WRST.maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

#if MACCATALYST
        // В .NET 10 на Mac событие Destroying срабатывает при закрытии окна
        window.Destroying += async (s, e) =>
        {
            // Используем global::, чтобы компилятор не путал класс MainPage со свойством App.MainPage
            var mainPage = global::WRST.maui.MainPage.Current;
            
            if (mainPage == null || mainPage.IsSaved)
                return;

            // Показываем диалог через страницу
            bool shouldSave = await mainPage.DisplayAlert(
                "Несохранённые данные",
                "Вы хотите сохранить изменения перед выходом?",
                "Да", "Нет"
            );

            if (shouldSave)
            {
                await mainPage.SaveDataAsync();
                this.Quit(); // Завершаем процесс приложения на Mac
            }
            else
            {
                this.Quit(); // Закрываем без сохранения
            }
        };
#endif

        return window;
    }
}
