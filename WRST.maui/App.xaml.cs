using Microsoft.Extensions.DependencyInjection;

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

            // Установка размеров
            window.Width = 800;
            window.Height = 800;

            return window;

            //return new Window(new AppShell());
        }
    }
}