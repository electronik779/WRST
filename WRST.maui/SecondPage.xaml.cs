using CommunityToolkit.Maui.Storage;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Maui.Controls.Shapes;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace WRST.maui;

public partial class SecondPage : ContentPage, IQueryAttributable
{
    // Для таблиц
    private ObservableCollection<TableRow> _controlData;
    private ObservableCollection<TableRow> _securityData;
    private ObservableCollection<TableRow> _volumeData;

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

    public ObservableCollection<TableRow> VolumeData
    {
        get => _volumeData;
        set { _volumeData = value; OnPropertyChanged(); }
    }

    public double AverageAnnualElectricityGeneration { get; set; }
    public double SumIdleResetVolume { get; set; }

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
    // Обеспеченности
    public ISeries[] InflowSecurityChartSeries { get; set; }
    public Axis[] SecXAxes { get; set; }
    public Axis[] IYAxes { get; set; }

    public ISeries[] ConsumptionSecurityChartSeries { get; set; }
    public Axis[] CYAxes { get; set; }

    public ISeries[] HeadSecurityChartSeries { get; set; }
    public Axis[] HYAxes { get; set; }

    public ISeries[] PowerSecurityChartSeries { get; set; }
    public Axis[] PYAxes { get; set; }
    // Диспетчерский
    public ISeries[] VolumeChartSeries { get; set; }
    public Axis[] VolXAxes { get; set; }
    public Axis[] VolYAxes { get; set; }

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

        // Безопасно извлекаем VolumeData
        if (query.TryGetValue("VolumeData", out var volumeDataObj) &&
            volumeDataObj is ObservableCollection<TableRow> volumeData)
        {
            VolumeData = volumeData;
        }

        // Безопасно извлекаем double число
        if (query.TryGetValue("AverageAnnualElectricityGeneration", out var averageGenerationObj) &&
            averageGenerationObj is double averageGenerationValue)
        {
            AverageAnnualElectricityGeneration = averageGenerationValue;

            OnPropertyChanged(nameof(AverageAnnualElectricityGeneration));
        }

        if (query.TryGetValue("SumIdleResetVolume", out var sumResetVolumeObj) &&
            sumResetVolumeObj is double sumResetVolumeValue)
        {
            SumIdleResetVolume = sumResetVolumeValue;

            OnPropertyChanged(nameof(SumIdleResetVolume));
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

        var security = new List<double>();
        var inflowSecurityValue = new List<double>();

        var consumptionSecurityValue = new List<double>();

        var headSecurityValue = new List<double>();

        var powerSecurityValue = new List<double>();

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

        foreach (var row in SecurityData)
        {
            security.Add(ParseCell(row.GetCell(0)));
            inflowSecurityValue.Add(ParseCell(row.GetCell(1)));

            consumptionSecurityValue.Add(ParseCell(row.GetCell(2)));

            headSecurityValue.Add(ParseCell(row.GetCell(3)));

            powerSecurityValue.Add(ParseCell(row.GetCell(4)));
        }
        var isPoints = security.Zip(inflowSecurityValue, (x, y) => new ObservablePoint(x, y)).ToArray();
        var csPoints = security.Zip(consumptionSecurityValue, (x, y) => new ObservablePoint(x, y)).ToArray();
        var hsPoints = security.Zip(headSecurityValue, (x, y) => new ObservablePoint(x, y)).ToArray();
        var psPoints = security.Zip(powerSecurityValue, (x, y) => new ObservablePoint(x, y)).ToArray();

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
                GeometrySize = 6, // Размер точек-узлов на линии
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
                GeometrySize = 6, // Размер точек-узлов на линии
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
                GeometrySize = 6, // Размер точек-узлов на линии
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

        // Обеспеченности
        var defaultColor = new SKColor(33, 150, 243);
        // Приток обеспеченность
        InflowSecurityChartSeries = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = isPoints,
                Name = "Приток",
                GeometrySize= 3,
                Stroke = new SolidColorPaint(defaultColor, 2)
            }
        };

        SecXAxes = new Axis[]
        {
            new Axis
            {
                Name = "Обеспеченность, %",
                MinLimit = 0,
                MaxLimit = 100
            }
        };

        IYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Приток, м³/с",
                Labeler = value => value.ToString("N0"),
                ForceStepToMin = false
            }
        };

        // Расход ГЭС обеспеченность
        ConsumptionSecurityChartSeries = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = csPoints,
                Name = "Расход ГЭС",
                GeometrySize = 3,
                Stroke = new SolidColorPaint(defaultColor, 2)
            }
        };

        CYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Расход ГЭС, м³/с",
                Labeler = value => value.ToString("N0"),
                ForceStepToMin = false
            }
        };

        // Напор обеспеченность
        HeadSecurityChartSeries = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = hsPoints,
                Name = "Статический напор",
                GeometrySize = 3,
                Stroke = new SolidColorPaint(defaultColor, 2)
            }
        };

        HYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Статический напор, м",
                Labeler = value => value.ToString("N0"),
                ForceStepToMin = false
            }
        };

        // Мощность обеспеченность
        PowerSecurityChartSeries = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = psPoints,
                Name = "Мощность",
                GeometrySize = 3,
                Stroke = new SolidColorPaint(defaultColor, 2)
            }
        };

        PYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Мощность, кВт",
                Labeler = value => value.ToString("N0"),
                ForceStepToMin = false
            }
        };

        // Объемы
        var seriesList = new List<ISeries>();

        double? ParseValue(TableRow row, int cellIndex)
        {
            if (row?.Cells == null || cellIndex >= row.Cells.Count) return null;
            string rawValue = row.Cells[cellIndex];
            if (string.IsNullOrWhiteSpace(rawValue)) return null;

            // 1. Удаляем обычные пробелы и неразрывные пробелы (\u00A0), очищающие разделители тысяч
            rawValue = rawValue.Replace(" ", "").Replace("\u00A0", "");

            // 2. Унифицируем разделитель дробной части (заменяем запятую на точку)
            rawValue = rawValue.Replace(',', '.').Trim();

            // 3. Парсим чистую строку, где остались только цифры и возможная точка
            if (double.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return null;
        }

        // 1. Диспетчерский график

        //// ---- БЛОК ОТЛАДКИ (ВЫВОД В КОНСОЛЬ) ----
        //System.Diagnostics.Debug.WriteLine($"=== ОТЛАДКА VolumeData. Всего строк: {VolumeData?.Count} ===");
        //if (VolumeData != null)
        //{
        //    int counter = 0;
        //    foreach (var row in VolumeData.Take(12)) // Смотрим первые 12 строк
        //    {
        //        string rawCell1 = (row?.Cells != null && row.Cells.Count > 1) ? row.Cells[1] : "НЕТ ЯЧЕЙКИ";
        //        double? parsed = ParseValue(row, 1);

        //        System.Diagnostics.Debug.WriteLine(
        //            $"Строка {counter}: RowLabel='{row?.RowLabel}' | Сырое значение Cells[1]='{rawCell1}' | Результат парсинга={parsed?.ToString() ?? "NULL"}"
        //        );
        //        counter++;
        //    }
        //}
        //System.Diagnostics.Debug.WriteLine("=== КОНЕЦ ЛОГА VolumeData ===");
        //// ----------------------------------------

        var volume1Values = VolumeData
            .Take(12)
            .Select(row => ParseValue(row, 1))
            .ToArray(); // Заменили на массив для стабильности LiveCharts

        seriesList.Add(new LineSeries<double?>
        {
            Values = volume1Values,
            Name = "Диспетчерский график",
            GeometrySize = 0, // Установили 0, чтобы убрать тяжелые маркеры точек
            LineSmoothness = 0,
            //Stroke = new SolidColorPaint(SKColors.Red, 3), // Делаем главную линию видимой и потолще
            Fill = null
        });

        // 2. Объемы по годам
        // 1. Создаем свой массив стандартных красивых цветов для графиков (Hex-формат)
        var customColors = new SKColor[]
        {
            SKColor.Parse("#1f77b4"), // Синий
            SKColor.Parse("#ff7f0e"), // Оранжевый
            SKColor.Parse("#2ca02c"), // Зеленый
            SKColor.Parse("#d62728"), // Красный
            SKColor.Parse("#9467bd"), // Фиолетовый
            SKColor.Parse("#8c564b"), // Коричневый
            SKColor.Parse("#e377c2"), // Розовый
            SKColor.Parse("#7f7f7f"), // Серый
            SKColor.Parse("#bcbd22"), // Оливковый
            SKColor.Parse("#17becf")  // Бирюзовый
        };

        int chunkSize = 12;
        int cycleNumber = 1;

        for (int i = 0; i < VolumeData.Count; i += chunkSize)
        {
            var chunk = VolumeData
                .Skip(i)
                .Take(chunkSize)
                .Select(row => ParseValue(row, 2))
                .ToArray();

            // 2. Берем цвет из нашего массива по кругу (оператор %)
            var baseColor = customColors[(cycleNumber - 1) % customColors.Length];

            seriesList.Add(new LineSeries<double?>
            {
                Values = chunk,
                Name = $"Год {cycleNumber}",
                GeometrySize = 0,
                LineSmoothness = 0.2,

                // 3. Применяем цвет и задаем толщину 1
                Stroke = new SolidColorPaint(baseColor, 1),
                // Stroke = new SolidColorPaint(baseColor.WithAlpha(120), 1), // 120 — прозрачность около 50%
                Fill = null
            });

            cycleNumber++;
        }

        // 3. Привязка к свойствам
        VolumeChartSeries = seriesList.ToArray();

        VolXAxes = new Axis[]
        {
            new Axis
            {
                Labels = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" },
                Name = "Месяцы"
            }
        };

        VolYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Объем на конец месяца, млн.м³",
                MinLimit = 0,
                Labeler = value => value.ToString("N0")
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
        OnPropertyChanged(nameof(PowerYAxes));

        OnPropertyChanged(nameof(InflowSecurityChartSeries));
        OnPropertyChanged(nameof(SecXAxes));
        OnPropertyChanged(nameof(IYAxes));

        OnPropertyChanged(nameof(ConsumptionSecurityChartSeries));
        OnPropertyChanged(nameof(CYAxes));

        OnPropertyChanged(nameof(HeadSecurityChartSeries));
        OnPropertyChanged(nameof(HYAxes));

        OnPropertyChanged(nameof(PowerSecurityChartSeries));
        OnPropertyChanged(nameof(PYAxes));

        OnPropertyChanged(nameof(VolumeChartSeries));
        OnPropertyChanged(nameof(VolXAxes));
        OnPropertyChanged(nameof(VolYAxes));
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

    private async void Save_Click(object sender, EventArgs e)
    {
        await ExportControlDataToCsvAsync();

    }

    public async Task ExportControlDataToCsvAsync()
    {
        try
        {
            var csvContent = new StringBuilder();

            // 1. Скрытый трюк для Excel: жестко задаем разделитель-запятую
            //csvContent.AppendLine("sep=;");

            // 1. Массив названий колонок для шапки (запятые внутри строк теперь безопасны)
            string[] headers = new string[]
            {
            "#", "Месяц", "Приток, м³/с", "Расход ГЭС, м³/с", "Сбросы, м³/с",
            "Отм. ВБ, м", "Отм. НБ, м", "Статический напор, м",
                "Мощность ГЭС, кВт", "Избыт. объем над дисп. остатком, млн.м³"
            };

            string headerLine = string.Join(";", headers.Select(EscapeCsvField));
            csvContent.AppendLine(headerLine);

            // 3. Наполняем строками из ControlData
            foreach (var row in ControlData)
            {
                // Берем первые 10 ячеек, очищаем от разделителей разрядов, а затем экранируем
                var processedCells = row.Cells
                    .Take(10)
                    .Select(CleanNumberFormat)
                    .Select(EscapeCsvField);

                string line = string.Join(";", processedCells);
                csvContent.AppendLine(line);
            }

            string Electricity = CleanNumberFormat(EscapeCsvField(AverageAnnualElectricityGeneration.
                ToString("F0")));
            string Reset = CleanNumberFormat(EscapeCsvField(SumIdleResetVolume.
                ToString("F0")));
            string tmpLine = $"Среднегодовая выработка {Electricity} кВтч" + ";" +
                $"Суммарный объем сбросов {Reset} млн.м³";
            csvContent.AppendLine("");
            csvContent.AppendLine(tmpLine);
            csvContent.AppendLine("");

            headers = Array.Empty<string>();
            headers = new [] { 
                "Обеспеченность, %", "Приток, м³/с", "Расход ГЭС, м³/с", "Статический напор, м",
                    "Мощность ГЭС, кВт"
                    };
            headerLine = string.Join(";", headers.Select(EscapeCsvField));
            csvContent.AppendLine(headerLine);

            foreach (var row in SecurityData)
            {
                // Берем первые 10 ячеек, очищаем от разделителей разрядов, а затем экранируем
                var processedCells = row.Cells
                    .Take(5)
                    .Select(CleanNumberFormat)
                    .Select(EscapeCsvField);

                string line = string.Join(";", processedCells);
                csvContent.AppendLine(line);
            }

            csvContent.AppendLine("");

            headers = Array.Empty<string>();
            headers = new[] {
                "#", "Месяц", "Диспетчерский объем, м³", "Фактический объем, м³"
                    };
            headerLine = string.Join(";", headers.Select(EscapeCsvField));
            csvContent.AppendLine(headerLine);

            int month = 1;
            for(int i = 0; i < VolumeData.Count; i++)
            {
                List<string> lineTmp = new List<string>();
                string tmp = VolumeData[i].GetCell(0);
                lineTmp.Add(tmp);
                lineTmp.Add(month.ToString());
                tmp = VolumeData[i].GetCell(1);
                lineTmp.Add(tmp);
                tmp = VolumeData[i].GetCell(2);
                lineTmp.Add(tmp);
                string line = string.Join (";", lineTmp.Select(CleanNumberFormat).Select(EscapeCsvField));
                csvContent.AppendLine(line);

                month++;
                if(month > 12) month = 1;
            }

            // 4. принудительно создаем кодировку UTF-8 с меткой BOM (true)
            var encodingWithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            byte[] fileBytes = encodingWithBom.GetBytes(csvContent.ToString());
            using var stream = new MemoryStream(fileBytes);

            // 5. Вызываем системный диалог сохранения файла
            var fileSaverResult = await FileSaver.Default.SaveAsync("Result_data.csv", stream, CancellationToken.None);

            if (fileSaverResult.IsSuccessful)
            {
                await DisplayAlertAsync(
                    "Успех!",
                    $"Файл успешно сохранен: {fileSaverResult.FilePath}\n(разделитель - точка с запятой)",
                    "OK"
                );
            }
            else if (fileSaverResult.Exception != null)
            {
                throw fileSaverResult.Exception;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Ошибка!",
                $"Не удалось сохранить файл: {ex.Message}",
                "OK"
            );
        }
    }

    // Метод для очистки чисел от разделителей разрядов
    private string CleanNumberFormat(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            return string.Empty;
        }

        // Удаляем обычные пробелы, неразрывные пробелы (\u00A0) и апострофы, 
        // которые часто используются для разделения тысяч (например, "1 500.50" -> "1500.50")
        string cleaned = field
            .Replace(" ", "")
            .Replace("\u00A0", "")
            .Replace("'", "");

        return cleaned;
    }

    // Вспомогательная функция для экранирования по стандарту CSV
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return string.Empty;
        }

        if (field.Contains(";") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}