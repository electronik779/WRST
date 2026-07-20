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
        // Подписываемся на попытку закрытия окна через менеджер приложения
        this.CloseWindowRequested += async (s, e) =>
        {
            var mainPage = MainPage.Current;
            if (mainPage == null || mainPage.IsSaved)
                return; // Если всё сохранено, окно закроется штатно

            // e.Cancel = true; НЕ поддерживается напрямую в CloseWindowRequested, 
            // поэтому в .NET 10 используется переопределение на уровне Window

            // Чтобы предотвратить моментальное исчезновение UI, мы переносим 
            // проверку сохранности на уровень закрытия конкретного окна ниже
        };

        // Самый надежный способ в .NET 10 для Mac: событие уничтожения окна
        window.Destroying += OnWindowDestroying;
#endif

        return window;
    }

#if MACCATALYST
    private async void OnWindowDestroying(object? sender, EventArgs e)
    {
        var mainPage = MainPage.Current;
        if (mainPage == null || mainPage.IsSaved)
            return;

        // Показываем нативный диалог MAUI
        bool? shouldSave = await mainPage.DisplayAlertAsync(
            "Несохранённые данные",
            "Вы хотите сохранить изменения перед выходом?",
            "Да", "Нет"
        );

        if (shouldSave == true)
        {
            await mainPage.SaveDataAsync();
            this.Quit(); // Полностью завершаем процесс приложения
        }
        else if (shouldSave == false)
        {
            this.Quit(); // Завершаем процесс без сохранения
        }
    }
#endif
}
