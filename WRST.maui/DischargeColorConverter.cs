using System.Globalization;

namespace WRST.maui
{
    public class DischargeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value теперь — это весь объект строки TableRow
            if (value is TableRow row && row.Cells.Count > 3)
            {
                string cellValue = row.Cells[3];
                if (double.TryParse(cellValue.Replace(',', '.'), CultureInfo.InvariantCulture, out double currentDischarge))
                {
                    // Сравниваем напрямую со свойством из этой же строки
                    if (currentDischarge < row.GuaranteedLimit)
                    {
                        return Color.FromArgb("#FFD2D2"); // Красный фон при дефиците
                    }
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
