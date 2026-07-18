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
                        // Проверяем текущую активную системную тему приложения
                        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

                        if (isDark)
                        {
                            // Приглушенный темно-бордовый для темной темы
                            return Color.FromArgb("#5A1A1A");
                        }
                        else
                        {
                            // Ваш исходный нежно-розовый для светлой темы
                            return Color.FromArgb("#FFD2D2");
                        }
                    }
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}