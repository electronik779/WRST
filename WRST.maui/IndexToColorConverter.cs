using System.Globalization;

namespace WRST.maui
{
    // Конвертер: принимает индекс (int) и возвращает цвет фона
    public class IndexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                // Для четных индексов — белый, для нечетных — светло-серый
                return (index % 2 == 0) ? Colors.White : Color.FromArgb("#F4F4F6");
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
