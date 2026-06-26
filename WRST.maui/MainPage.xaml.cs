using CommunityToolkit.Maui.Storage;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
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
        double[,] InflowTableData = new double[0, 0]; // Приток.
        double[,] BathygraphyTableData = new double[0, 0]; // характеристика верхнего бьефа.
        double[,] RemainderAccordingDispatchScheduleTableData = new double[0, 0]; // Диспетчерские остатки.
        double[,] IntakeFromReservoirTableData = new double[0, 0]; // Отбор из водохранилища.
        double[,] CharacteristicOfDownstreamTableData = new double[0, 0]; // Характеристика нижнего бьефа. 

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
                // 1.Находим строку TableRow, к которой принадлежит этот Entry
                if (entry.BindingContext is string && entry.Parent is StackLayout parentLayout)
                {
                    // Находим индекс текущего поля ввода среди соседей (это и есть индекс ячейки)
                    int cellIndex = parentLayout.Children.IndexOf(entry);

                    // Находим саму строку данных
                    if (parentLayout.BindingContext is TableRow row)
                    {
                        // Сохраняем введенный пользователем текст в память коллекции Cells
                        if (cellIndex >= 0 && cellIndex < row.Cells.Count)
                        {
                            row.Cells[cellIndex] = entry.Text;
                        }
                    }
                }

                string text = entry.Text?.Replace(',', '.') ?? "";

                // Проверяем: не пусто ли и является ли целым числом
                bool isValid = !string.IsNullOrWhiteSpace(text) &&
                              int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out _);

                if (isValid)
                {
                    // Возвращаем стандартное состояние (цвета подтянутся из Normal/Focused в зависимости от темы)
                    VisualStateManager.GoToState(entry, "Normal");
                }
                else
                {
                    // Переключаем в наше кастомное состояние ошибки
                    VisualStateManager.GoToState(entry, "Invalid");
                }
            }
        }

        // Проверка корректности ввода дробных чисел в текстовое поле
        private void OnDoubleEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                // 1.Находим строку TableRow, к которой принадлежит этот Entry
                if (entry.BindingContext is string && entry.Parent is StackLayout parentLayout)
                {
                    // Находим индекс текущего поля ввода среди соседей (это и есть индекс ячейки)
                    int cellIndex = parentLayout.Children.IndexOf(entry);

                    // Находим саму строку данных
                    if (parentLayout.BindingContext is TableRow row)
                    {
                        // Сохраняем введенный пользователем текст в память коллекции Cells
                        if (cellIndex >= 0 && cellIndex < row.Cells.Count)
                        {
                            row.Cells[cellIndex] = entry.Text;
                        }
                    }
                }

                string text = entry.Text?.Replace(',', '.') ?? "";

                // Проверяем: не пусто ли и является ли целым числом
                bool isValid = !string.IsNullOrWhiteSpace(text) &&
                              double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out _);

                if (isValid)
                {
                    // Возвращаем стандартное состояние (цвета подтянутся из Normal/Focused в зависимости от темы)
                    VisualStateManager.GoToState(entry, "Normal");
                }
                else
                {
                    // Переключаем в наше кастомное состояние ошибки
                    VisualStateManager.GoToState(entry, "Invalid");
                }
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

            //InflowCollectionView.ItemsSource = null;
            //InflowCollectionView.ItemsSource = InflowData;
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

            //BathygraphyCollectionView.ItemsSource = null;
            //BathygraphyCollectionView.ItemsSource = BathygraphyData;
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

            //DownstreamCollectionView.ItemsSource = null;
            //DownstreamCollectionView.ItemsSource = DownstreamData;
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

                InflowCollectionView.ItemsSource = InflowData;
                BathygraphyCollectionView.ItemsSource = BathygraphyData;
                DownstreamCollectionView.ItemsSource = DownstreamData;
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
            var culture = CultureInfo.InvariantCulture;

            for (int i = 0; i < data1.Count; i++)
            {
                string rawValue1 = data1[i];
                string processedValue = "0";

                if (!string.IsNullOrWhiteSpace(rawValue1))
                {
                    rawValue1 = rawValue1.Replace(',', '.');
                    if (double.TryParse(rawValue1, NumberStyles.Any, culture, out double value))
                    {
                        processedValue = value.ToString(culture);
                    }
                }
                table[1].SetCell(i, processedValue);
            }
            //table[1].RaiseCellsChanged();
        }

        private void FillTableFromCSV(ObservableCollection<TableRow> table, List<string> data1, List<string> data2)
        {
            // table[0] is row 1 data, table[1] is row 2 data
            if (table.Count < 2) return;
            var culture = CultureInfo.InvariantCulture;

            for (int i = 0; i < data1.Count; i++)
            {
                // Строка 1 (table[0])
                string rawValue1 = data1[i];
                string processedValue1 = "0";
                if (!string.IsNullOrWhiteSpace(rawValue1))
                {
                    rawValue1 = rawValue1.Replace(',', '.');
                    if (double.TryParse(rawValue1, NumberStyles.Any, culture, out double value1))
                    {
                        processedValue1 = value1.ToString(culture);
                    }
                }
                table[0].SetCell(i, processedValue1);

                // Строка 2 (table[1]) - ИСПРАВЛЕНО: проверяем rawValue2 вместо rawValue1
                string rawValue2 = i < data2.Count ? data2[i] : null;
                string processedValue2 = "0";
                if (!string.IsNullOrWhiteSpace(rawValue2))
                {
                    rawValue2 = rawValue2.Replace(',', '.');
                    if (double.TryParse(rawValue2, NumberStyles.Any, culture, out double value2))
                    {
                        processedValue2 = value2.ToString(culture);
                    }
                }
                table[1].SetCell(i, processedValue2);
            }
            //table[0].RaiseCellsChanged();
            //table[1].RaiseCellsChanged();
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
                    string rawValue = row.Cells[cellIndex];

                    if (!string.IsNullOrWhiteSpace(rawValue))
                    {
                        // Принудительно меняем запятую на точку для InvariantCulture
                        rawValue = rawValue.Replace(',', '.');

                        if (double.TryParse(rawValue, NumberStyles.Any, culture, out double val))
                        {
                            return val.ToString(culture);
                        }
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

            InflowTableData = new double[1, InflowCount];
            BathygraphyTableData = new double[2, BathygraphyCount];
            RemainderAccordingDispatchScheduleTableData = new double[1, 12];
            IntakeFromReservoirTableData = new double[1, 12];
            CharacteristicOfDownstreamTableData = new double[2, CharacteristicOfDownstreamCount];

            // Получаем значения из таблиц
            try
            {
                // 1. Приток: данные находятся в InflowData[1] (так как индекс 0 — это шапка "Месяцы")
                if (InflowData.Count >= 2)
                    ExportTableToMatrix(InflowData, InflowTableData, startRowIndex: 1);

                // 2. Батиграфия: Отметки в BathygraphyData[0], Объемы в BathygraphyData[1]
                // Если у вас в батиграфии НЕТ строки-шапки, передаем startRowIndex: 0
                if (BathygraphyData.Count >= 2)
                    ExportTableToMatrix(BathygraphyData, BathygraphyTableData, startRowIndex: 0);

                // 3. Нижний бьеф: Расходы в DownstreamData[0], Уровни в DownstreamData[1]
                if (DownstreamData.Count >= 2)
                    ExportTableToMatrix(DownstreamData, CharacteristicOfDownstreamTableData, startRowIndex: 0);

                // 4. Остаток по диспетчерскому графику (12 месяцев)
                if (RemainderData.Count >= 1)
                    ExportTableToMatrix(RemainderData, RemainderAccordingDispatchScheduleTableData, startRowIndex: 1);

                // 5. Забор из водохранилища (12 месяцев)
                if (IntakeData.Count >= 1)
                    ExportTableToMatrix(IntakeData, IntakeFromReservoirTableData, startRowIndex: 1);
            }
            catch (Exception ex)
            {
                DisplayAlertAsync("Ошибка в исходных данных", ex.Message, "OK");
                return;
            }

            // Проверяем кпд агрегата
            if (Efficiency >= 1)
            {
                DisplayAlertAsync("Ошибка", "Кпд агрегата не может быть равен\nили больше единицы", "OK");
                return;
            }

            int MonthOrdinalNumber = 0; // Порядковый номер месяца
            int CalendarMonth = BeginningMonth - 1; // Календарный номер месяца в индексах
                                                    // Индексы массивов 0-11, месяцы - 1-12, поэтому -1.
            int PreviousMonth = CalendarMonth - 1; // Предыдущий месяц
            if (PreviousMonth < 0) PreviousMonth = 11; // Сразу в индексах
            double ExcessVolume = 0; // Избыточный объем
            double CurrentConsumption = 0; // Текущий расход ГЭС
            double IdleDischargeFlowRate = 0; // Расход холостых сбросов
            double IncreaseVolume = 0; // Приращение объема водохранилища
            double ResidualVolumePreviousMonth =
                RemainderAccordingDispatchScheduleTableData[0, PreviousMonth]; // Диспетчерский остаток 
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

            foreach (double i in InflowTableData)
            {
                Debug.WriteLine(i);
            }

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
                        HeadLoss = kHeadLoss * CurrentConsumption * Math.Abs(CurrentConsumption);
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
                MonthElectricityProduction = Power[i] * 730;
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
            var culture = CultureInfo.InvariantCulture;

            if (!double.TryParse(UsefulVolumeInput.Text?.Replace(',', '.'), NumberStyles.Any, culture, out UsefullVolume)) return;
            if (!double.TryParse(UselessVolumeInput.Text?.Replace(',', '.'), NumberStyles.Any, culture, out UselessVolume)) return;
            if (!double.TryParse(GuaranteedDischargeInput.Text?.Replace(',', '.'), NumberStyles.Any, culture, out GuaranteedDischarge)) return;
            if (!double.TryParse(FullDischargeInput.Text?.Replace(',', '.'), NumberStyles.Any, culture, out FullDischarge)) return;
            if (!double.TryParse(HeadLossInput.Text?.Replace(',', '.'), NumberStyles.Any, culture, out kHeadLoss)) return;
            if (!double.TryParse(EfficiencyInput.Text?.Replace(',', '.'), NumberStyles.Any, culture, out Efficiency)) return;
        }

        private void ExportTableToMatrix(ObservableCollection<TableRow> uiTable, double[,] targetMatrix, int startRowIndex)
        {
            var culture = CultureInfo.InvariantCulture;
            int matrixRows = targetMatrix.GetLength(0); // Количество строк в массиве (1 или 2)
            int matrixCols = targetMatrix.GetLength(1); // Количество колонок (InflowCount, 12 и т.д.)

            Debug.WriteLine($"=== СБОР ДАННЫХ: Таблица содержит {uiTable.Count} строк. Ожидаем матрицу {matrixRows}x{matrixCols} ===");

            for (int r = 0; r < matrixRows; r++)
            {
                // Вычисляем индекс строки в uiTable. 
                // Если startRowIndex = 1, то первая строка данных будет взята из uiTable[1], вторая из uiTable[2]
                int uiRowIndex = startRowIndex + r;

                if (uiRowIndex >= uiTable.Count)
                {
                    Debug.WriteLine($"[ОШИБКА] Индекс строки {uiRowIndex} выходит за пределы таблицы (всего строк: {uiTable.Count})");
                    break;
                }

                var row = uiTable[uiRowIndex];
                Debug.WriteLine($"Строка {uiRowIndex}: Название='{row.RowLabel}', Количество ячеек Cells={row.Cells?.Count}");

                if (row.Cells == null || row.Cells.Count == 0)
                {
                    Debug.WriteLine($"[ОШИБКА] Список Cells пуст или равен null для строки {uiRowIndex}!");
                    continue;
                }

                for (int c = 0; c < matrixCols; c++)
                {
                    if (c >= row.Cells.Count)
                    {
                        Debug.WriteLine($"[ПРЕДУПРЕЖДЕНИЕ] Индекс колонки {c} превышает размер Cells ({row.Cells.Count})");
                        break;
                    }

                    string rawValue = row.Cells[c]?.Replace(',', '.') ?? "0";

                    if (double.TryParse(rawValue, NumberStyles.Any, culture, out double parsedValue))
                    {
                        targetMatrix[r, c] = parsedValue;
                        if (c < 3) Debug.WriteLine($"  Ячейка [{c}] успешно записана: '{rawValue}' -> {parsedValue}");
                    }
                    else
                    {
                        Debug.WriteLine($"[ОШИБКА] Не удалось распарсить текст в ячейке [{c}]: '{rawValue}'");
                        targetMatrix[r, c] = 0.0; // Значение по умолчанию при ошибке ввода
                    }
                }
            }
        }

        private void FillArrayFromCollectionX(ObservableCollection<TableRow> collection, double[,] array)
        {
            //int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            var row = collection[1];

            for (int j = 0; j < cols; j++)
                {
                // Защита от выхода за границы списка Cells в строке
                if (j >= row.Cells.Count) break;

                    string cellValue = row.Cells[j];

                    // Заменяем запятые на точки для универсального парсинга дробных чисел
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        cellValue = cellValue.Replace(',', '.');
                    }

                    // Безопасный парсинг. Если строка пустая или не число, запишется 0.0
                    if (double.TryParse(cellValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedValue))
                    {
                        array[0, j] = parsedValue;
                    }
                    else
                    {
                        array[0, j] = 0.0; // Значение по умолчанию при ошибке парсинга
                    }
                }
        }
        private void FillArrayFromCollectionXY(ObservableCollection<TableRow> collection, double[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                // Защита от выхода за границы, если в коллекции строк меньше, чем размерность массива
                if (i >= collection.Count) break;

                var row = collection[i];
                if (row?.Cells == null) continue;

                for (int j = 0; j < cols; j++)
                {
                    // Защита от выхода за границы списка Cells в строке
                    if (j >= row.Cells.Count) break;

                    string cellValue = row.Cells[j];

                    // Заменяем запятые на точки для универсального парсинга дробных чисел
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        cellValue = cellValue.Replace(',', '.');
                    }

                    // Безопасный парсинг. Если строка пустая или не число, запишется 0.0
                    if (double.TryParse(cellValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedValue))
                    {
                        array[i, j] = parsedValue;
                    }
                    else
                    {
                        array[i, j] = 0.0; // Значение по умолчанию при ошибке парсинга
                    }
                }
            }
        }

        private bool CheckData()
        {
            // Проверяем каждое поле по очереди. 
            // Если метод проверки возвращает true (ошибка), сразу выходим из CheckData.

            if (CheckIntVariable("Номер календарного месяца начала расчета", BeginningMonthInput.Text)) return true;
            if (CheckIntVariable("Количество значений притока", InflowCountInput.Text)) return true;

            if (CheckDoubleVariable("Полезный объем водохранилища", UsefulVolumeInput.Text)) return true;
            if (CheckDoubleVariable("Мертвый объем водохранилища", UselessVolumeInput.Text)) return true;

            if (CheckIntVariable("Количество точек батиграфической характеристики", BathygraphyCountInput.Text)) return true;
            if (CheckIntVariable("Количество точек характеристики нижнего бьефа", CharacteristicOfDownstreamCountInput.Text)) return true;

            if (CheckDoubleVariable("Гарантированный расход ГЭС", GuaranteedDischargeInput.Text)) return true;
            if (CheckDoubleVariable("Полный (максимальный) расход ГЭС", FullDischargeInput.Text)) return true;
            if (CheckDoubleVariable("Потери напора", HeadLossInput.Text)) return true;
            if (CheckDoubleVariable("Кпд гидроагрегата", EfficiencyInput.Text)) return true;

            return false; // Если дошли сюда, значит все данные валидны
        }

        private bool CheckIntVariable(string varName, string variable)
        {
            if (!int.TryParse(variable, out _))
            {
                DisplayAlertAsync("Ошибка!", $"«{varName}» - введены неверные данные.", "OK");
                return true;
            }
            return false;
        }

        private bool CheckDoubleVariable(string varName, string variable)
        {
            if (string.IsNullOrWhiteSpace(variable))
            {
                DisplayAlertAsync("Ошибка!", $"«{varName}» - поле не может быть пустым.", "OK");
                return true; // Возвращаем true (есть ошибка)
            }

            // Принудительно меняем запятую на точку
            string safeVariable = variable.Replace(',', '.');

            if (!double.TryParse(safeVariable, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                DisplayAlertAsync("Ошибка!", $"«{varName}» - введены неверные данные.", "OK");
                return true; // Возвращаем true (есть ошибка)
            }

            return false; // Ошибок нет
        }

        private void OnEntryFocused(object? sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                // Dispatcher дает платформе завершить внутренние процессы фокусировки,
                // после чего мы безопасно выделяем весь текст
                Dispatcher.Dispatch(() =>
                {
                    if (entry.IsFocused && !string.IsNullOrEmpty(entry.Text))
                    {
                        entry.CursorPosition = 0;
                        entry.SelectionLength = entry.Text.Length;
                    }
                });
            }
        }

    }
}
