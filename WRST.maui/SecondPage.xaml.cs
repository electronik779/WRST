

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using System.Globalization;

namespace WRST.maui;

public partial class SecondPage : ContentPage, IQueryAttributable
{
    // Для таблиц
    private ObservableCollection<TableRow> _controlData;
    private ObservableCollection<TableRow> _securityData;
    private ObservableCollection<TableRow> _remainderData;

    public ObservableCollection<TableRow> ControlData
    {
        get => _controlData;
        set { _controlData = value; OnPropertyChanged(); }
    }

    public ObservableCollection<TableRow> SecurityData
    {
        get => _securityData;
        set { _securityData = value; OnPropertyChanged(); }
    }

    public double GuaranteedDischarge { get; set; }

    public ObservableCollection<TableRow> RemainderData
    {
        get => _remainderData;
        set { _remainderData = value; OnPropertyChanged(); }
    }

    // Для графиков
        // Расходы
    public ISeries[] DischChartSeries { get; set; }
    public Axis[] XAxes { get; set; }
    public Axis[] DischYAxes { get; set; }
        // ВБ
    public ISeries[] UpLevelChartSeries { get; set; }
    public Axis[] UpLevelYAxes { get; set; }
        // НБ
    public ISeries[] DownLevelChartSeries { get; set; }
    public Axis[] DownLevelYAxes { get; set; }
        // Статический напор
    public ISeries[] StaticHeadChartSeries { get; set; }
    public Axis[] StaticHeadYAxes { get; set; }
        // Мощность
    public ISeries[] PowerChartSeries { get; set; }
    public Axis[] PowerYAxes { get; set; }

    public SecondPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // Этот метод вызывается MAUI Shell автоматически ПЕРЕД открытием страницы
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // Безопасно извлекаем ControlData
        if (query.TryGetValue("ControlData", out var controlDataObj) &&
            controlDataObj is ObservableCollection<TableRow> controlData)
        {
            ControlData = controlData;
        }

        // Безопасно извлекаем SecurityData
        if (query.TryGetValue("SecurityData", out var securityDataObj) &&
            securityDataObj is ObservableCollection<TableRow> securityData)
        {
            SecurityData = securityData;
        }

        // Безопасно извлекаем double число
        if (query.TryGetValue("GuaranteedDischarg", out var dischargeObj) &&
            dischargeObj is double dischargeValue)
        {
            GuaranteedDischarge = dischargeValue;

            // Важно: уведомляем XAML, что лимит обновился, 
            // чтобы конвертер в таблице сразу его увидел
            OnPropertyChanged(nameof(GuaranteedDischarge));
        }

        // Безопасно извлекаем RemainderData
        if (query.TryGetValue("SecurityData", out var remainderDataObj) &&
            securityDataObj is ObservableCollection<TableRow> remainderData)
        {
            RemainderData = remainderData;
        }

        // Строим графики
        if (ControlData != null)
        {
            CreateChart();
        }
    }

    private void CreateChart()
    {
        var months = new List<string>();
        var inflowValues = new List<double>();
        var consumptionValues = new List<double>();
        var idleResetValues = new List<double>();

        var upstreamLevelValues = new List<double>();

        var downstreamLevelValues = new List<double>();

        var staticHeadValues = new List<double>();

        var powerValues = new List<double>();

        // Извлекаем данные из коллекции строк
        foreach (var row in ControlData)
        {
            // Подписи осей берем из столбца 1 (индекс 1 - "Месяц")
            months.Add(row.GetCell(1));

            // Парсим значения для столбцов 2, 3, 4
            inflowValues.Add(ParseCell(row.GetCell(2)));       // Приток
            consumptionValues.Add(ParseCell(row.GetCell(3)));  // Расход
            idleResetValues.Add(ParseCell(row.GetCell(4)));     // Сбросы

            upstreamLevelValues.Add(ParseCell(row.GetCell(5))); //ВБ

            downstreamLevelValues.Add(ParseCell(row.GetCell(6))); // НБ

            staticHeadValues.Add(ParseCell(row.GetCell(7))); // Статический напор

            powerValues.Add(ParseCell(row.GetCell(8))); // Мощность
        }

        // Расходы
            // Формируем столбчатые серии (ColumnSeries)
        DischChartSeries = new ISeries[]
        {
            new ColumnSeries<double> { Values = inflowValues, Name = "Приток" },
            new ColumnSeries<double> { Values = consumptionValues, Name = "Расход ГЭС" },
            new ColumnSeries<double> { Values = idleResetValues, Name = "Сбросы" }
        };

        // 2. Настраиваем ось X (Горизонтальную)
        XAxes = new Axis[]
        {
            new Axis
            {
                Name = "Месяц", // Название оси X
                Labels = months.ToArray(),
                LabelsRotation = 0
            }
        };

        // 3. Настраиваем ось Y (Вертикальную)
        DischYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Расход, м³/с", // Название оси Y
                Labeler = value => value.ToString("N0") // Форматирование чисел на оси (без дробной части)
            }
        };
        
            // ВБ
        UpLevelChartSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = upstreamLevelValues,
                Name = "Отметка ВБ",
                GeometrySize = 8, // Размер точек-узлов на линии
                LineSmoothness = 0.5 // Степень сглаживания линии (0 - ломаная, 1 - максимально плавная)
            }
        };

        UpLevelYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Отм.ВБ, м",
                Labeler = value => value.ToString("N2"), // Выводим два знака после запятой для метров
                // Разрешаем SkiaSharp автоматически подбирать границы шкалы под рабочий диапазон ГЭС,
                // чтобы график не начинался строго с нуля метров:
                ForceStepToMin = false
            }
        };

            // НБ
        DownLevelChartSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = downstreamLevelValues,
                Name = "Отметка НБ",
                GeometrySize = 8, // Размер точек-узлов на линии
                LineSmoothness = 0.5 // Степень сглаживания линии (0 - ломаная, 1 - максимально плавная)
            }
        };

        DownLevelYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Отм.НБ, м",
                Labeler = value => value.ToString("N2"), 
                ForceStepToMin = false
            }
        };
        
            // Статический напор
        StaticHeadChartSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = staticHeadValues,
                Name = "Статический напор",
                GeometrySize = 8, // Размер точек-узлов на линии
                LineSmoothness = 0.5 // Степень сглаживания линии (0 - ломаная, 1 - максимально плавная)
            }
        };

        StaticHeadYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Статический напор, м",
                Labeler = value => value.ToString("N2"), 
                ForceStepToMin = false
            }
        };
        
            // Мощности
        PowerChartSeries = new ISeries[]
        {
            new ColumnSeries<double> { Values = powerValues, Name = "Мощность ГЭС" }
        };

        // 3. Настраиваем ось Y (Вертикальную)
        PowerYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Мощность ГЭС, кВт", // Название оси Y
                Labeler = value => value.ToString("N0") // Форматирование чисел на оси (без дробной части)
            }
        };

        // Уведомляем интерфейс об обновлении всех осей
        OnPropertyChanged(nameof(DischChartSeries));
        OnPropertyChanged(nameof(XAxes));
        OnPropertyChanged(nameof(DischYAxes));

        OnPropertyChanged(nameof(UpLevelChartSeries));
        OnPropertyChanged(nameof(UpLevelYAxes));

        OnPropertyChanged(nameof(DownLevelChartSeries));
        OnPropertyChanged(nameof(DownLevelYAxes));

        OnPropertyChanged(nameof(StaticHeadChartSeries));
        OnPropertyChanged(nameof(StaticHeadYAxes));

        OnPropertyChanged(nameof(PowerChartSeries));
        OnPropertyChanged(nameof(XAxes));
        OnPropertyChanged(nameof(PowerYAxes));
    }

    private double ParseCell(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        // 1. Удаляем обычные пробелы и неразрывные пробелы (\u00A0), которые часто добавляет формат N2
        string cleanValue = value.Replace(" ", "").Replace("\u00A0", "");

        // 2. Унифицируем разделитель: заменяем запятую на точку для инвариантной культуры
        cleanValue = cleanValue.Replace(',', '.');

        // 3. Безопасно парсим результат
        if (double.TryParse(cleanValue, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        return 0;
    }

    private void Save_Click(object sender, EventArgs e)
    {

    }
}

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