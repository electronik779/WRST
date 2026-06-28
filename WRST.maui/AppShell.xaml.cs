namespace WRST.maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("SecondPage", typeof(SecondPage));
        }
    }
}
