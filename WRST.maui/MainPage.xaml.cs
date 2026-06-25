using CommunityToolkit.Maui.Storage;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Text;

namespace WRST.maui
{
    public partial class MainPage : ContentPage
    {
        // 1. Связываем коллекции с BindingContext
        public ObservableCollection<TableRow> InflowData { get; set; } = new();
        public ObservableCollection<TableRow> BathygraphyData { get; set; } = new();
        public ObservableCollection<TableRow> RemainderData { get; set; } = new();
        public ObservableCollection<TableRow> IntakeData { get; set; } = new();
        public ObservableCollection<TableRow> DownstreamData { get; set; } = new();

        // Исходные данные
        int BeginningMonth = 0; // Месяц начала расчета
        int InflowCount = 0; // Количество значений притока
        int BathygraphyCount = 0; // Количество точек характеристики верхнего бьефа
        int CharacteristicOfDownstreamCount = 0; // Количество точек характеристики нижнего бьефа
        double UsefullVolume = 0; // Полезный объем
        double UselessVolume = 0; // Мертвый объем
        double GuaranteedDischarge = 0; // Гарантированный расход ГЭС
        double FullDischarge = 0; // Максимальный расход ГЭС
        double kHeadLoss = 0; // Потери напора / коэффициент при Q^2
        double Efficiency = 0; // Кпд агрегата

        // Исходные данные. Массивы (строки x столбцы)
        double[,] InflowTableData = new double[0, 0]; // Приток. Т1
        double[,] BathygraphyTableData = new double[0, 0]; // характеристика верхнего бьефа. Т2
        double[,] RemainderAccordingDispatchScheduleTableData = new double[0, 0]; // Диспетчерские остатки. Т3
        double[,] IntakeFromReservoirTableData = new double[0, 0]; // Отбор из водохранилища. Т4
        double[,] CharacteristicOfDownstreamTableData = new double[0, 0]; // Характеристика нижнего бьефа. Любой другой префикс 

        public MainPage()
        {
            InitializeComponent();

            BindingContext = this;

            // Инициализация таблиц диспетчерских остатков и отборов
            InitializeRemainderAndIntakeTables();
        }

        private void InitializeRemainderAndIntakeTables()
        {
            // Создаем строки для таблицы диспетчерских остатков (12 месяцев)
            RemainderData.Clear();

            // Первая строка - заголовки
            var headerRow = new TableRow();
            headerRow.Index = 0;
            headerRow.IsEditable = false;
            headerRow.RowBackgroundColor = "LightGray";
            headerRow.RowLabel = "Месяц"; // Метка только в заголовке
            headerRow.InitializeCells(12, ""); // Только данные (месяцы)
            for (int i = 0; i < 12; i++)
            {
                headerRow.Cells[i] = (i + 1).ToString();
            }
            RemainderData.Add(headerRow);

            // Вторая строка - ввод данных
            var inputRow = new TableRow();
            inputRow.Index = 1;
            inputRow.IsEditable = true;
            inputRow.RowBackgroundColor = "White";
            inputRow.RowLabel = "Объем, млн.м³";
            inputRow.InitializeCells(12, "0");
            RemainderData.Add(inputRow);

            RemainderCollectionView.ItemsSource = null;
            RemainderCollectionView.ItemsSource = RemainderData;

            // Создаем строки для таблицы отборов (12 месяцев)
            IntakeData.Clear();

            // Первая строка - заголовки
            var headerRowInt = new TableRow();
            headerRowInt.Index = 0;
            headerRowInt.IsEditable = false;
            headerRowInt.RowBackgroundColor = "LightGray";
            headerRowInt.RowLabel = "Месяц"; // Метка только в заголовке
            headerRowInt.InitializeCells(12, ""); // Только данные (месяцы)
            for (int i = 0; i < 12; i++)
            {
                headerRowInt.Cells[i] = (i + 1).ToString();
            }
            IntakeData.Add(headerRowInt);

            // Вторая строка - ввод данных
            var inputRowInt = new TableRow();
            inputRowInt.Index = 1;
            inputRowInt.IsEditable = true;
            inputRowInt.RowBackgroundColor = "White";
            inputRowInt.RowLabel = "Расход, м³/с";
            inputRowInt.InitializeCells(12, "0");
            IntakeData.Add(inputRowInt);

            IntakeCollectionView.ItemsSource = null;
            IntakeCollectionView.ItemsSource = IntakeData;
        }

        // Выбор схемы питания. Индивидуальная - задаем потери напора, групповая - задаем к при Q^2
        private void OnRadioChanged(object? sender, CheckedChangedEventArgs e)
        {
            // Нас интересует только момент, когда кнопку выбрали (Value = true)
            if (sender is RadioButton radioButton && e.Value)
            {
                // Проверяем значение Value, которое мы указали в XAML
                string? selectedValue = radioButton.Value?.ToString();

                if (selectedValue == "0")
                {
                    HeadLossLable.Text = "Потери напора, м";
                }
                else if (selectedValue == "1")
                {
                    HeadLossLable.Text = "Коэффициент потерь при Qгэс²";
                }
            }
        }

        // Проверка корректности ввода целых чисел в текстовое поле
        private void OnIntEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string text = entry.Text?.Replace(',', '.') ?? "";

                // Если пусто — считаем ошибкой
                if (string.IsNullOrWhiteSpace(text))
                {
                    entry.TextColor = Colors.Red; // Или Black, если поле необязательное
                }

                // Проверка на число
                bool isValid = int.TryParse(text,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out _);

                entry.TextColor = isValid ? Colors.Black : Colors.Red;
            }
        }

        // Проверка корректности ввода дробных чисел в текстовое поле
        private void OnDoubleEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string text = entry.Text?.Replace(',', '.') ?? "";

                // Если пусто — считаем ошибкой
                if (string.IsNullOrWhiteSpace(text))
                {
                    entry.TextColor = Colors.Red; // Или Black, если поле необязательное
                }

                // Проверка на число
                bool isValid = double.TryParse(text,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out _);

                entry.TextColor = isValid ? Colors.Black : Colors.Red;
            }
        }

        // По нажатию кнопки "Создать" напротив поля ввода Количество значений притока
        // рисуем таблицу притока
        private void InflowButton_Clicked(object sender, EventArgs e)
        {
            // Получение значения начального месяца. Проверка корректности
            if (!int.TryParse(BeginningMonthInput.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out BeginningMonth))
            {
                DisplayAlertAsync("Внимание.",
                    "Необходимо задать\n" +
                    "«Номер календарного месяца начала расчета»", "OK");
                return;
            }
            // Получение значения количества точек притока. Проверка корректности
            if (!int.TryParse(InflowCountInput.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out InflowCount))
            {
                DisplayAlertAsync("Внимание.",
                    "Необходимо задать\n" +
                    "«Количество значений притока»", "OK");
                return;
            }

            // Проверка: какое введено значение начального месяца. Если не 1-12 - выход
            if (BeginningMonth <= 0 || BeginningMonth > 12)
            {
                BeginningMonthInput.Text = "0";
                DisplayAlertAsync("Информация.",
                    "Допустимый диапазон значений для\n" +
                    "«Номер календарного месяца начала расчета» -\n1-12.", "OK");
                return;
            }

            // Проверка: какое введено количество значений притока. Если не 1-600 - выход
            if (InflowCount <= 0 || InflowCount > 600)
            {
                InflowCountInput.Text = "0";
                DisplayAlertAsync("Информация.",
                    "Допустимый диапазон значений для\n" +
                    "«Количество значений притока» -\n1-600. Автоматическое округление,\n" +
                    "кратно 12, в большую сторону.", "OK");
                return;
            }

            // Проверяем, что количество значений кратно 12 месяцам. Если нет - корректируем
            if (InflowCount % 12 != 0)
            {
                while (InflowCount % 12 != 0) { InflowCount++; }
                InflowCountInput.Text = InflowCount.ToString();
                DisplayAlertAsync("Информация.",
                    "«Количество значений притока»\n" +
                    "было скорректировано.", "OK");
            }

            InflowData.Clear();

            // Первая строка - заголовки
            var headerRow = new TableRow();
            headerRow.Index = 0;
            headerRow.IsEditable = false;
            headerRow.RowBackgroundColor = "LightGray";
            headerRow.RowLabel = "Месяц"; // Метка только в заголовке
            headerRow.InitializeCells(InflowCount, ""); // Только данные (месяцы)
            for (int i = 0; i < InflowCount; i++)
            {
                int tmpMonth = BeginningMonth + i;
                if (tmpMonth > InflowCount) tmpMonth -= InflowCount;
                headerRow.Cells[i] = tmpMonth.ToString();
            }
            InflowData.Add(headerRow);

            // Вторая строка - ввод данных
            var inputRow = new TableRow();
            inputRow.Index = 1;
            inputRow.IsEditable = true;
            inputRow.RowBackgroundColor = "White";
            inputRow.RowLabel = "Приток, м³/с";
            inputRow.InitializeCells(InflowCount, "0");
            InflowData.Add(inputRow);

            InflowCollectionView.ItemsSource = null;
            InflowCollectionView.ItemsSource = InflowData;
        }

        // По нажатию кнопки "Создать" напротив поля ввода Количество точек характеристики верхнего бьефа
        // рисуем таблицу верхнего бьефа
        private void BathygraphyButton_Clicked(object sender, EventArgs e)
        {
            // Получение значения количества точек характеристики верхнего бьефа. Проверка корректности
            if (!int.TryParse(BathygraphyCountInput.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out BathygraphyCount))
            {
                DisplayAlertAsync("Внимание.",
                    "Необходимо задать\n" +
                    "«Количество точек характеристики верхнего бьефа»", "OK");
                return;
            }

            // Проверка: какое введено количество точек характеристики верхнего бьефа. Если не 2-20 - выход
            if (BathygraphyCount < 2 || BathygraphyCount > 20)
            {
                BathygraphyCountInput.Text = "0";
                DisplayAlertAsync("Информация.", "Допустимый диапазон значений для\n" +
                    "«Количество точек характеристики верхнего бьефа» -\n2-20.", "OK");
                return;
            }

            BathygraphyData.Clear();

            // Первая строка - объем
            var inputRowVol = new TableRow();
            inputRowVol.Index = 0;
            inputRowVol.IsEditable = true;
            inputRowVol.RowBackgroundColor = "White";
            inputRowVol.RowLabel = "Объем, млн.м³"; // Метка в заголовке
            inputRowVol.InitializeCells(BathygraphyCount, "0"); // Только данные 
            BathygraphyData.Add(inputRowVol);

            // Вторая строка - отметка
            var inputRowEl = new TableRow();
            inputRowEl.Index = 1;
            inputRowEl.IsEditable = true;
            inputRowEl.RowBackgroundColor = "White";
            inputRowEl.RowLabel = "Отметка, м";
            inputRowEl.InitializeCells(BathygraphyCount, "0");
            BathygraphyData.Add(inputRowEl);

            BathygraphyCollectionView.ItemsSource = null;
            BathygraphyCollectionView.ItemsSource = BathygraphyData;
        }

        // По нажатию кнопки "Создать" напротив поля ввода Количество точек характеристики нижнего бьефа
        // рисуем таблицу нижнего бьефа
        private void CharacteristicOfDownstreamButton_Clicked(object sender, EventArgs e)
        {
            // Получение значения количества точек характеристики нижнего бьефа. Проверка корректности
            if (!int.TryParse(CharacteristicOfDownstreamCountInput.Text,
                NumberStyles.Any, CultureInfo.InvariantCulture,
                out CharacteristicOfDownstreamCount))
            {
                DisplayAlertAsync("Внимание.",
                    "Необходимо задать\n" +
                    "«Количество точек характеристики нижнего бьефа»", "OK");
                return;
            }

            // Проверка: какое введено количество точек характеристики нижнего бьефа. Если не 2-20 - выход
            if (CharacteristicOfDownstreamCount < 2 || CharacteristicOfDownstreamCount > 20)
            {
                CharacteristicOfDownstreamCountInput.Text = "0";
                DisplayAlertAsync("Информация.", "Допустимый диапазон значений для\n" +
                    "«Количество точек характеристики нижнего бьефа» -\n2-20.", "OK");
            }

            DownstreamData.Clear();

            // Первая строка - расход
            var inputRowFl = new TableRow();
            inputRowFl.Index = 0;
            inputRowFl.IsEditable = true;
            inputRowFl.RowBackgroundColor = "White";
            inputRowFl.RowLabel = "Расход, м³/с";
            inputRowFl.InitializeCells(CharacteristicOfDownstreamCount, "0");
            DownstreamData.Add(inputRowFl);

            // Вторая строка - отметка
            var inputRowEl = new TableRow();
            inputRowEl.Index = 1;
            inputRowEl.IsEditable = true;
            inputRowEl.RowBackgroundColor = "White";
            inputRowEl.RowLabel = "Отметка, м";
            inputRowEl.InitializeCells(CharacteristicOfDownstreamCount, "0");
            DownstreamData.Add(inputRowEl);

            DownstreamCollectionView.ItemsSource = null;
            DownstreamCollectionView.ItemsSource = DownstreamData;
        }

        // Считывание исходных данных из файла
        private async void Open_Click(object sender, EventArgs e)
        {
            var culture = CultureInfo.InvariantCulture;
            try
            {
                // Выбор файла
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Выберите CSV файл",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> {
                        { DevicePlatform.WinUI, new[] { ".csv" } },
                        { DevicePlatform.MacCatalyst, new[] { "csv" } }
                    })
                });

                if (result == null) return;

                var blocks = new List<List<string>>();
                using (var reader = new StreamReader(await result.OpenReadAsync()))
                {
                    string? line;
                    // Читаем, пока ReadLineAsync не вернет null
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            blocks.Add(line.Split(';').Select(s => s.Trim()).ToList());
                        }
                    }
                }
                if (blocks.Count < 6)
                {
                    await DisplayAlertAsync("Ошибка!", "Файл поврежден.", "ОК");
                    return;
                }

                // Блок 1: Одиночные поля
                var b1 = blocks[0];
                BeginningMonthInput.Text = b1.ElementAtOrDefault(0);
                int.TryParse(BeginningMonthInput.Text, NumberStyles.Any, culture, out BeginningMonth);

                // Установка RadioButton
                string radioVal = b1.ElementAtOrDefault(1) ?? "0";
                var target = RadioGroup.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.Value?.ToString() == radioVal);
                if (target != null) target.IsChecked = true;

                UsefulVolumeInput.Text = b1.ElementAtOrDefault(2) ?? "0";
                UselessVolumeInput.Text = b1.ElementAtOrDefault(3) ?? "0";
                GuaranteedDischargeInput.Text = b1.ElementAtOrDefault(4) ?? "0";
                FullDischargeInput.Text = b1.ElementAtOrDefault(5) ?? "0";
                HeadLossInput.Text = b1.ElementAtOrDefault(6) ?? "0";
                EfficiencyInput.Text = b1.ElementAtOrDefault(7) ?? "0";

                // Блок 2: Inflow
                int.TryParse(blocks[1].FirstOrDefault(), out InflowCount);
                InflowCountInput.Text = InflowCount.ToString();

                // Блок 3: Bathygraphy
                int.TryParse(blocks[2].FirstOrDefault(), out BathygraphyCount);
                BathygraphyCountInput.Text = BathygraphyCount.ToString();

                // Блок 4: Downstream
                int.TryParse(blocks[3].FirstOrDefault(), out CharacteristicOfDownstreamCount);
                CharacteristicOfDownstreamCountInput.Text = CharacteristicOfDownstreamCount.ToString();

                // Блоки 5 и 6: Remainder и Intake (по 12 мес)
                // Для этих таблиц уже есть инициализация, просто заполняем данными

                // Обновляем UI
                UpdateUI(blocks);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Ошибка", ex.Message, "OK");
                //ResetInputs();
            }
        }

        private void UpdateUI(List<List<string>> blocks)
        {
            try
            {
                InflowButton_Clicked(null, null);
                BathygraphyButton_Clicked(null, null);
                CharacteristicOfDownstreamButton_Clicked(null, null);

                // Блок 2: Inflow
                var inflowData = blocks[1].Skip(1).Take(InflowCount).ToList();
                FillTableFromCSV(InflowData, inflowData);

                // Блок 3: Bathygraphy
                var bathyData1 = blocks[2].Skip(1).Take(BathygraphyCount).ToList();
                var bathyData2 = blocks[2].Skip(BathygraphyCount + 1).Take(BathygraphyCount).ToList();
                FillTableFromCSV(BathygraphyData, bathyData1, bathyData2);

                // Блок 4: Downstream
                var downstreamData1 = blocks[3].Skip(1).Take(CharacteristicOfDownstreamCount).ToList();
                var downstreamData2 = blocks[3].Skip(CharacteristicOfDownstreamCount + 1).Take(CharacteristicOfDownstreamCount).ToList();
                FillTableFromCSV(DownstreamData, downstreamData1, downstreamData2);

                // Блоки 5 и 6: Remainder и Intake (по 12 мес)
                var remainderData = blocks[4].Take(12).ToList();
                FillTableFromCSV(RemainderData, remainderData);

                var intakeData = blocks[5].Take(12).ToList();
                FillTableFromCSV(IntakeData, intakeData);
            }
            catch (Exception ex)
            {
                // Обработка ошибки
                Console.WriteLine($"Ошибка обновления UI: {ex.Message}");
            }
        }

        private void FillTableFromCSV(ObservableCollection<TableRow> table, List<string> data1)
        {
            // table[0] is header, table[1] is data
            if (table.Count < 2) return;

            for (int i = 0; i < data1.Count; i++)
            {
                string valStr = double.TryParse(data1[i], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out double val) ? val.ToString(CultureInfo.InvariantCulture) : "0";

                table[1].SetCell(i, valStr);
            }
        }

        private void FillTableFromCSV(ObservableCollection<TableRow> table, List<string> data1, List<string> data2)
        {
            // table[0] is row 1 data, table[1] is row 2 data
            if (table.Count < 2) return;

            for (int i = 0; i < data1.Count; i++)
            {
                string valStr1 = double.TryParse(data1[i], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out double val1) ? val1.ToString(CultureInfo.InvariantCulture) : "0";
                table[0].SetCell(i, valStr1);

                string valStr2 = double.TryParse(data2[i], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out double val2) ? val2.ToString(CultureInfo.InvariantCulture) : "0";
                table[1].SetCell(i, valStr2);
            }
        }

        // Сохранение данных в файл
        private async void Save_Click(object sender, EventArgs e)
        {
            // Проверяем, что в текстовых полях есть числа
            if (CheckData()) return;

            try
            {
                GetFields();
                var culture = CultureInfo.InvariantCulture; // Точка всегда будет разделителем

                // 1. Блок 1: Основные параметры
                var selectedValue = RadioGroup.Children.OfType<RadioButton>()
                    .FirstOrDefault(rb => rb.IsChecked)?.Value?.ToString() ?? "0";

                string b1 = string.Join(";", new[] {
                    BeginningMonth.ToString(culture),
                    selectedValue,
                    UsefullVolume.ToString(culture),
                    UselessVolume.ToString(culture),
                    GuaranteedDischarge.ToString(culture),
                    FullDischarge.ToString(culture),
                    kHeadLoss.ToString(culture),
                    Efficiency.ToString(culture)
                });

                // 2. Блоки данных
                string b2 = InflowCount + ";" + GetRowData(InflowData, 1, InflowCount);

                string b3 = BathygraphyCount + ";" +
                            GetRowData(BathygraphyData, 0, BathygraphyCount) + ";" +
                            GetRowData(BathygraphyData, 1, BathygraphyCount);

                string b4 = CharacteristicOfDownstreamCount + ";" +
                            GetRowData(DownstreamData, 0, CharacteristicOfDownstreamCount) + ";" +
                            GetRowData(DownstreamData, 1, CharacteristicOfDownstreamCount);

                string b5 = GetRowData(RemainderData, 1, 12);
                string b6 = GetRowData(IntakeData, 1, 12);

                // 3. Сборка и сохранение
                string content = string.Join(Environment.NewLine, new[] { b1, b2, b3, b4, b5, b6 });

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                var result = await FileSaver.Default.SaveAsync("Initial_data.csv", stream, CancellationToken.None);

                if (result.IsSuccessful)
                    await DisplayAlertAsync("Успех!", "Файл сохранен.\n(разделитель - точка с запятой).", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Ошибка", "Ошибка записи: " + ex.Message, "OK");
            }
        }

        // Универсальный метод с поддержкой форматов чисел
        //private string GetRowData(ObservableCollection<TableRow> table, int columnIndex, int count)
        //{
        //    var culture = CultureInfo.InvariantCulture;
        //    return string.Join(";", Enumerable.Range(0, count).Select(i =>
        //    {
        //        if (i < table.Count && columnIndex < table[i].Cells.Count)
        //        {
        //            // Пытаемся распарсить значение как число
        //            if (double.TryParse(table[i].Cells[columnIndex], NumberStyles.Any, culture, out double val))
        //            {
        //                return val.ToString(culture);
        //            }
        //        }
        //        return "0";
        //    }));
        //}
        private string GetRowData(ObservableCollection<TableRow> table, int rowIndex, int count)
        {
            var culture = CultureInfo.InvariantCulture;

            // Проверяем, существует ли вообще строка с таким индексом в таблице
            if (rowIndex >= table.Count)
            {
                // Возвращаем строку из нулей, если строки нет
                return string.Join(";", Enumerable.Repeat("0", count));
            }

            var row = table[rowIndex];

            return string.Join(";", Enumerable.Range(0, count).Select(cellIndex =>
            {
                // Проверяем, что индекс ячейки не выходит за границы списка Cells
                if (cellIndex < row.Cells.Count)
                {
                    // Пытаемся распарсить значение как число
                    if (double.TryParse(row.Cells[cellIndex], NumberStyles.Any, culture, out double val))
                    {
                        return val.ToString(culture);
                    }
                }
                return "0"; // Если ячейки нет или там не число
            }));
        }

        // Выполнение расчета
        private void Execute_Click(object sender, EventArgs e)
        {
            // Проверяем, что в текстовых полях есть числа
            if (CheckData()) return;

            // Получаем значения из текстовых полей
            GetFields();

            // Проверяем кпд агрегата
            if (Efficiency >= 1)
            {
                DisplayAlertAsync("Ошибка", "Кпд агрегата не может быть равен\nили больше единицы", "OK");
                return;
            }

            int MonthOrdinalNumber = 0; // Порядковый номер месяца
            int CalendarMonth = BeginningMonth - 1; // Календарный номер месяца в индексах
                                                    // Индексы массивов 0-11, месяцы - 1-12, поэтому -1.
            double ExcessVolume = 0; // Избыточный объем
            double CurrentConsumption = 0; // Текущий расход ГЭС
            double IdleDischargeFlowRate = 0; // Расход холостых сбросов
            double IncreaseVolume = 0; // Приращение объема водохранилища
            double ResidualVolumePreviousMonth =
                RemainderAccordingDispatchScheduleTableData[0, CalendarMonth - 1]; // Диспетчерский остаток 
                                                                                   // в предыдущем месяце (т.к. начинаем с полного водохранилища, то он 
                                                                                   // д.б. равен полезному объему)
            double ResidualVolumeCurrentMonth = 0; // Диспетчерский остаток в текущем месяце
            double RequiredVolumeAccordingDispatchSchedule = 0; // Требуемый объем по диспетчерскому графику
            double[] Consumption = new double[InflowCount]; // Фактический расход ГЭС
            double[] IdleReset = new double[InflowCount]; // Фактические холостые сбросы
            double[] ActualResidualVolume = new double[InflowCount]; // Фактический остаточный объем
                                                                     // над диспетчерским графиком
            double VolumeInReservoir = 0; // Объем в водохранилище
            double DischargeIntoDownstream = 0; // Расход в нижний бьеф
            double[] UpstreamLevel = new double[InflowCount]; // Отметка ВБ
            double[] DownstreamLevel = new double[InflowCount]; // Отметка НБ
            double[] StaticHead = new double[InflowCount]; // Статический напор ГЭС
            double HeadLoss = 0; // Потери напора ГЭС
            double[] Power = new double[InflowCount]; // Мощность ГЭС

            while (MonthOrdinalNumber < InflowCount)
            {
                // Параметры предыдущего месяца
                CurrentConsumption = GuaranteedDischarge + ExcessVolume / 2.63;
                if (CurrentConsumption > FullDischarge) CurrentConsumption = FullDischarge;
                IdleDischargeFlowRate = 0;

                // Вариант 1 - между диспетчерской линией и НПУ
                IncreaseVolume = (InflowTableData[0, MonthOrdinalNumber] - CurrentConsumption -
                    IdleDischargeFlowRate - IntakeFromReservoirTableData[0, CalendarMonth]) * 2.63;
                ResidualVolumeCurrentMonth = ResidualVolumePreviousMonth + IncreaseVolume;
                RequiredVolumeAccordingDispatchSchedule =
                    RemainderAccordingDispatchScheduleTableData[0, CalendarMonth];

                // Вариант 2 - вышли за НПУ
                if (ResidualVolumeCurrentMonth > UsefullVolume)
                {
                    CurrentConsumption = CurrentConsumption + (ResidualVolumeCurrentMonth - UsefullVolume) / 2.63;
                    if (CurrentConsumption > FullDischarge)
                    // Холостые сбросы
                    {
                        IdleDischargeFlowRate = CurrentConsumption - FullDischarge;
                        CurrentConsumption = FullDischarge;
                        ResidualVolumeCurrentMonth = UsefullVolume;
                    }
                    else
                    // Нет сбросов
                    {
                        IncreaseVolume = (InflowTableData[0, MonthOrdinalNumber] - CurrentConsumption -
                    IdleDischargeFlowRate - IntakeFromReservoirTableData[0, CalendarMonth]) * 2.63;
                        ResidualVolumeCurrentMonth = ResidualVolumePreviousMonth + IncreaseVolume;
                    }
                }
                // Вариант 3 - ниже диспетчерской линии
                else if (ResidualVolumeCurrentMonth < RequiredVolumeAccordingDispatchSchedule)
                {
                    CurrentConsumption = CurrentConsumption +
                        (ResidualVolumeCurrentMonth - RequiredVolumeAccordingDispatchSchedule) / 2.63;
                    IncreaseVolume = (InflowTableData[0, MonthOrdinalNumber] - CurrentConsumption -
                        IdleDischargeFlowRate - IntakeFromReservoirTableData[0, CalendarMonth]) * 2.63;
                    ResidualVolumeCurrentMonth = ResidualVolumePreviousMonth + IncreaseVolume;
                }

                // Запоминаем результаты и вычисляем статический напор, мощность
                Consumption[MonthOrdinalNumber] = CurrentConsumption;
                IdleReset[MonthOrdinalNumber] = IdleDischargeFlowRate;
                ActualResidualVolume[MonthOrdinalNumber] = ResidualVolumeCurrentMonth -
                    RequiredVolumeAccordingDispatchSchedule;
                VolumeInReservoir = ResidualVolumeCurrentMonth + UselessVolume;
                DischargeIntoDownstream = CurrentConsumption + IdleDischargeFlowRate;
                UpstreamLevel[MonthOrdinalNumber] =
                    LinearInterpolation(VolumeInReservoir, BathygraphyTableData);
                DownstreamLevel[MonthOrdinalNumber] =
                    LinearInterpolation(DischargeIntoDownstream, CharacteristicOfDownstreamTableData);
                StaticHead[MonthOrdinalNumber] = UpstreamLevel[MonthOrdinalNumber] -
                    DownstreamLevel[MonthOrdinalNumber];

                object selectedValue = RadioButtonGroup.GetSelectedValue(RadioGroup);
                if (selectedValue != null)
                {
                    string select = selectedValue!.ToString()!;
                    if (select == "0")
                    {
                        HeadLoss = kHeadLoss;
                    }
                    else
                    {
                        HeadLoss = kHeadLoss * CurrentConsumption * CurrentConsumption;
                    }
                }
                Power[MonthOrdinalNumber] = 9.81 * CurrentConsumption *
                    (StaticHead[MonthOrdinalNumber] - HeadLoss) * Efficiency;

                // Переприсваивание
                ResidualVolumePreviousMonth = ResidualVolumeCurrentMonth;

                // Следующий месяц
                MonthOrdinalNumber++;
                CalendarMonth++;
                if (CalendarMonth > 11) CalendarMonth = 0;
            }
            // Суммарный объем холостых сбросов
            double MonthIdleResetVolume = 0;
            double SumIdleResetVolume = 0;
            for (int i = 0; i < InflowCount; i++)
            {
                MonthIdleResetVolume = IdleReset[i] * 2.63;
                SumIdleResetVolume += MonthIdleResetVolume;
            }

            // Среднегодовая выработка
            double MonthElectricityProduction = 0;
            double SumElectricityProduction = 0;
            double AverageAnnualElectricityGeneration = 0;
            for (int i = 0; i < InflowCount; i++)
            {
                MonthElectricityProduction = Power[i] * 720;
                SumElectricityProduction += MonthElectricityProduction;
            }
            AverageAnnualElectricityGeneration = SumElectricityProduction * 12 / InflowCount;
        }

        private double LinearInterpolation(double argument, double[,] xy)
        {
            int i1 = 0;
            double dx = 0;
            double result = 0;
            int quantity = xy.GetLength(0);
            for (int i = 1; i < quantity; i++)
            {
                if (argument - xy[0, i] <= 0)
                {
                    i1 = i - 1;
                    dx = xy[0, i] - xy[0, i1];
                    result = (xy[1, i] * (argument - xy[0, i1]) - xy[1, i1] * (argument - xy[0, i])) / dx;
                    return result;
                }
            }
            i1 = quantity - 2;
            dx = xy[0, quantity - 1] - xy[0, i1];
            result = (xy[1, quantity - 1] * (argument - xy[0, i1]) -
                xy[1, i1] * (argument - xy[0, quantity - 1])) / dx;
            return result;
        }

        private void GetFields()
        {
            if (!double.TryParse(UsefulVolumeInput.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out UsefullVolume)) return;
            if (!double.TryParse(UselessVolumeInput.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out UselessVolume)) return;
            if (!double.TryParse(GuaranteedDischargeInput.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out GuaranteedDischarge)) return;
            if (!double.TryParse(FullDischargeInput.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out FullDischarge)) return;
            if (!double.TryParse(HeadLossInput.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out kHeadLoss)) return;
            if (!double.TryParse(EfficiencyInput.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out Efficiency)) return;
        }

        private bool CheckData()
        {
            bool err = false;

            err = CheckIntVariable("Номер календарного месяца начала расчета", BeginningMonthInput.Text);

            err = CheckIntVariable("Количество значений притока", InflowCountInput.Text);

            err = CheckDoubleVariable("Полезный объем водохранилища", UsefulVolumeInput.Text);

            err = CheckDoubleVariable("Мертвый объем водохранилища", UselessVolumeInput.Text);

            err = CheckIntVariable("Количество точек батиграфической характеристики",
                BathygraphyCountInput.Text);

            err = CheckIntVariable("Количество точек характеристики нижнего бьефа",
                CharacteristicOfDownstreamCountInput.Text);

            err = CheckDoubleVariable("Гарантированный расход ГЭС", GuaranteedDischargeInput.Text);

            err = CheckDoubleVariable("Полный (максимальный) расход ГЭС", FullDischargeInput.Text);

            err = CheckDoubleVariable("Потери напора", HeadLossInput.Text);

            err = CheckDoubleVariable("Кпд гидроагрегата", EfficiencyInput.Text);

            return err;
        }

        private bool CheckIntVariable(string varName, string variable)
        {
            bool err = false;

            if (!int.TryParse(variable, out _)) err = true;
            if (err) DisplayAlertAsync("Ошибка!", $"«{varName}» - введены неверные данные.", "OK");

            return err;
        }

        private bool CheckDoubleVariable(string varName, string variable)
        {
            bool err = false;

            if (!double.TryParse(variable, NumberStyles.Any,
                        CultureInfo.InvariantCulture, out double _result))
                err = true;
            if (err) DisplayAlertAsync("Ошибка!", $"«{varName}» - введены неверные данные.", "OK");
            return err;
        }
    }
}
