

namespace WRST.maui;

[QueryProperty(nameof(TableData), "WECData")]

public partial class SecondPage : ContentPage
{
    public List<TableRow> TableData
    {
        set
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                if (value == null || TableControl == null) return;
                TableControl.ItemsSource = value;
            });
        }
    }

    
    private void Save_Click(object sender, EventArgs e)
    {

    }
}