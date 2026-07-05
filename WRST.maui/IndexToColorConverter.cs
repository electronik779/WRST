using System.Globalization;

namespace WRST.maui
{
    public class IndexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                // Проверяем текущую активную системную тему приложения
                bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

                if (isDark)
                {
                    // Чередование для ТЕМНОЙ темы: темно-серый и чуть светлее
                    return (index % 2 == 0) ? Color.FromArgb("#1C1C1E") : Color.FromArgb("#2C2C2E");
                }
                else
                {
                    // Чередование для СВЕТЛОЙ темы (ваш исходный код)
                    return (index % 2 == 0) ? Colors.White : Color.FromArgb("#F4F4F6");
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
