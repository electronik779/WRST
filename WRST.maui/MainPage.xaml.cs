using CommunityToolkit.Maui.Storage;
using System.Data;
using System.Globalization;
using System.Text;

namespace WRST.maui
{
    public partial class MainPage : ContentPage
    {
        // Исходные данные
        int BeginningMonth = 0; // Месяц начала расчета
        int InflowCount = 0; // Количество значений притока
        int BathygraphyCount = 0; // Количество точек характеристики верхнего бьефа
        int CharacteristicOfDownstreamCount = 0; // Количество точек характеристики нижнего бьефа
        double UsefulVolume = 0; // Полезный объем
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

            // Рисуем таблицу диспетчерских остатков
            List<string> RowNames = new List<string>() { "Остатки, млн.м³" };
            BuildGrid(RemainderAccordingDispatchScheduleFixedColumn,
                RemainderAccordingDispatchScheduleScrollableGrid, 12, 1, "T3", 1, "Месяц", RowNames);

            // Рисуем таблицу отборов из водохранилища
            RowNames.Clear();
            RowNames.Add("Отбор, м³/с");
            BuildGrid(IntakeFromReservoirFixedColumn, IntakeFromRreservoirScrollableGrid, 12, 1, "T4",
                1, "Месяц", RowNames);
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

            // Рисуем таблицу притока
            List<string> RowNames = new List<string>() { "Приток, м³/с" };
            BuildGrid(InflowFixedColumn, InflowScrollableGrid, InflowCount, 1, "T1",
                        BeginningMonth, "Месяц, #", RowNames);
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

            // Рисуем таблицу верхнего бьефа
            List<string> RowNames = new List<string>() { "Объем, млн.м³", "Отметка ВБ, м" };
            BuildGrid(BathygraphyFixedColumn, BathygraphyScrollableGrid, BathygraphyCount, 2, "T2",
                1, "#", RowNames);
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

            // Рисуем таблицу нижнего бьефа
            List<string> RowNames = new List<string>() { "Расход, м³/с", "Отметка НБ, м" };
            BuildGrid(CharacteristicOfDownstreamFixedColumn, CharacteristicOfDownstreamScrollableGrid,
                CharacteristicOfDownstreamCount, 2, "T5", 1, "#", RowNames);
        }

        // Рисование таблиц
        // 1 - название таблицы заголовков строк (см. xaml)
        // 2 - название таблицы (см. xaml)
        // 3 - количество столбцов таблицы
        // 4 - количество строк таблицы
        // 5 - префикс - соответствие массиву данных
        // 6 - начало нумерации столбцов
        // 7 - название строки названий столбцов
        // 8 - список названий строк
        private void BuildGrid(Grid fixedGrid, Grid scrollGrid, int colCount, int rowCount,
            string prefix, int begin, string headerName, List<string> rowNames)
        {
            // Инициализируем массив данных нужного размера
            if (prefix == "T1") InflowTableData = new double[rowCount, colCount];
            else if (prefix == "T2") BathygraphyTableData = new double[rowCount, colCount];
            else if (prefix == "T3") RemainderAccordingDispatchScheduleTableData = new double[rowCount, colCount];
            else if (prefix == "T4") IntakeFromReservoirTableData = new double[rowCount, colCount];
            else CharacteristicOfDownstreamTableData = new double[rowCount, colCount];

            // Очистка обеих частей
            fixedGrid.Children.Clear(); fixedGrid.RowDefinitions.Clear(); fixedGrid.ColumnDefinitions.Clear();
            scrollGrid.Children.Clear(); scrollGrid.RowDefinitions.Clear(); scrollGrid.ColumnDefinitions.Clear();

            // Настройка строк (одинаковая для обеих частей)
            for (int r = 0; r <= rowCount; r++) // +1 для заголовка
            {
                var height = r == 0 ? 35 : 40;
                fixedGrid.RowDefinitions.Add(new RowDefinition { Height = height });
                scrollGrid.RowDefinitions.Add(new RowDefinition { Height = height });
            }

            // Настройка столбцов
            fixedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
            for (int c = 0; c < colCount; c++)
                scrollGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 80 });

            // Угловая ячейка (в фиксированной части) (название строки названий столбцов)
            var corner = CreateHeaderBorder(headerName, Colors.LightSlateGray); // Верхняя левая ячейка - надпись
            Grid.SetRow(corner, 0);
            fixedGrid.Children.Add(corner);

            // Заголовки столбцов (в прокручиваемой части)
            for (int c = 0; c < colCount; c++)
            {
                int head = c + begin;
                if (head > colCount) head -= colCount;
                var colHeader = CreateHeaderBorder((head).ToString(), Colors.DarkGray);
                Grid.SetRow(colHeader, 0);
                Grid.SetColumn(colHeader, c);
                scrollGrid.Children.Add(colHeader);
            }

            // Заполнение строк
            for (int r = 0; r < rowCount; r++)
            {
                // Заголовок строки -> в ФИКСИРОВАННУЮ сетку
                var rowHeader = CreateHeaderBorder(rowNames[r], Colors.LightGray);
                Grid.SetRow(rowHeader, r + 1);
                fixedGrid.Children.Add(rowHeader);

                // Ячейки данных -> в ПРОКРУЧИВАЕМУЮ сетку
                for (int c = 0; c < colCount; c++)
                {
                    var entry = new Entry
                    {
                        Placeholder = "0",
                        Keyboard = Keyboard.Numeric,
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontSize = 12,
                        AutomationId = $"{r},{c}", // Координаты для обработчика
                        TextColor = Colors.Black,
                        Text = ""
                    };

                    // Подписка на валидацию и сохранение
                    entry.TextChanged += OnNumericTextChanged;

                    var cellBorder = new Border
                    {
                        Stroke = Colors.Gray,
                        StrokeThickness = 0.5,
                        Content = entry
                    };
                    Grid.SetRow(cellBorder, r + 1);
                    Grid.SetColumn(cellBorder, c);
                    scrollGrid.Children.Add(cellBorder);
                }
            }
        }

        // Проверка корректности ввода чисел в таблицы. Если не число - выделяется красным,
        // в массив заносится 0
        private void OnNumericTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is not Entry entry) return;

            if (string.IsNullOrWhiteSpace(e.NewTextValue)) return;

            // Заменяем точку на запятую (или наоборот) для универсальности
            string cleanValue = e.NewTextValue.Replace(',', '.');

            // Пытаемся преобразовать в число
            if (double.TryParse(cleanValue, NumberStyles.Any,
                CultureInfo.InvariantCulture, out double result))
            {
                // Если число валидное — возвращаем обычный цвет и сохраняем
                entry.TextColor = Colors.Black;
                SaveToDataArray(entry.AutomationId, result, entry);
            }
            else
            {
                // Если ввели буквы — красим текст в красный
                entry.TextColor = Colors.Red;
            }
        }

        // Сохранение в массив введенных в таблицу данных
        private void SaveToDataArray(string authId, double value, Entry entry)
        {
            var coords = authId.Split(',');
            int r = int.Parse(coords[0]);
            int c = int.Parse(coords[1]);

            // Определяем, к какой таблице относится Entry через его родителя (Grid)
            var parentGrid = entry.Parent?.Parent as Grid; // Border -> Grid

            if (parentGrid == InflowScrollableGrid)
                InflowTableData[r, c] = value;
            else if (parentGrid == BathygraphyScrollableGrid)
                BathygraphyTableData[r, c] = value;
            else if (parentGrid == RemainderAccordingDispatchScheduleScrollableGrid)
                RemainderAccordingDispatchScheduleTableData[r, c] = value;
            else if (parentGrid == IntakeFromRreservoirScrollableGrid)
                IntakeFromReservoirTableData[r, c] = value;
            else CharacteristicOfDownstreamTableData[r, c] = value;
        }

        // Оформление ячеек
        private Border CreateHeaderBorder(string text, Color bgColor)
        {
            return new Border
            {
                BackgroundColor = bgColor,
                Stroke = Colors.DimGray,
                StrokeThickness = 1,
                Content = new Label
                {
                    Text = text,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
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
                    string line;
                    while ((line = await reader.ReadLineAsync() ?? "") != null)
                        blocks.Add(line.Split(';').Select(s => s.Trim()).ToList());
                }

                if (blocks.Count < 6) return;

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
                var inflowData = new List<List<string>> { ParseBlockData(blocks[1].Skip(1), InflowCount) };

                // Блок 3: Bathygraphy
                int.TryParse(blocks[2].FirstOrDefault(), out BathygraphyCount);
                BathygraphyCountInput.Text = BathygraphyCount.ToString();
                var bathyData = new List<List<string>>
                    {
                        ParseBlockData(blocks[2].Skip(1), BathygraphyCount),
                        ParseBlockData(blocks[2].Skip(BathygraphyCount + 1), BathygraphyCount)
                    };

                // Блок 4: Downstream
                int.TryParse(blocks[3].FirstOrDefault(), out CharacteristicOfDownstreamCount);
                CharacteristicOfDownstreamCountInput.Text = CharacteristicOfDownstreamCount.ToString();
                var downstreamData = new List<List<string>>
                    {
                        ParseBlockData(blocks[3].Skip(1), CharacteristicOfDownstreamCount),
                        ParseBlockData(blocks[3].Skip(CharacteristicOfDownstreamCount + 1), CharacteristicOfDownstreamCount)
                    };

                // Блоки 5 и 6: Remainder и Intake (по 12 мес)
                var remainderData = new List<List<string>> { ParseBlockData(blocks[4], 12) };
                var intakeData = new List<List<string>> { ParseBlockData(blocks[5], 12) };

                // 4. Отрисовка
                UpdateUI(inflowData, bathyData, downstreamData, remainderData, intakeData);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Ошибка", ex.Message, "OK");
                ResetInputs();
            }
        }

        // Вспомогательная функция для парсинга списка строк в числа при чтении файла
        private List<string> ParseBlockData(IEnumerable<string> rawData, int count)
        {
            var culture = CultureInfo.InvariantCulture;
            return rawData.Take(count).Select(s =>
            {
                // Пытаемся распарсить с точкой, если не выходит — возвращаем "0"
                return double.TryParse(s, NumberStyles.Any, culture, out double val)
                    ? val.ToString(culture)
                    : "0";
            }).ToList();
        }

        // Очистка текстовых полей при ошибке чтения
        private void ResetInputs()
        {
            string zero = "0";
            BeginningMonthInput.Text = InflowCountInput.Text = UsefulVolumeInput.Text =
            UselessVolumeInput.Text = BathygraphyCountInput.Text = zero;
            // ... и так далее
        }

        // Вспомогательный метод для чистоты кода
        private void UpdateUI(List<List<string>> inflow, List<List<string>> bathy, List<List<string>> down, List<List<string>> rem, List<List<string>> intake)
        {
            BuildGrid(InflowFixedColumn, InflowScrollableGrid, InflowCount, 1, "T1", BeginningMonth, "Месяц, #", new() { "Приток, м³/с" });
            FillTableFromData(InflowScrollableGrid, inflow);

            BuildGrid(BathygraphyFixedColumn, BathygraphyScrollableGrid, BathygraphyCount, 2, "T2", 1, "#", new() { "Объем, млн.м³", "Отметка ВБ, м" });
            FillTableFromData(BathygraphyScrollableGrid, bathy);

            FillTableFromData(RemainderAccordingDispatchScheduleScrollableGrid, rem);
            FillTableFromData(IntakeFromRreservoirScrollableGrid, intake);

            BuildGrid(CharacteristicOfDownstreamFixedColumn, CharacteristicOfDownstreamScrollableGrid, CharacteristicOfDownstreamCount, 2, "T5", 1, "#", new() { "Расход, м³/с", "Отметка НБ, м" });
            FillTableFromData(CharacteristicOfDownstreamScrollableGrid, down);
        }

        private void FillTableFromData(Grid scrollGrid, List<List<string>> csvData)
        {
            // Проходим по всем элементам в прокручиваемой сетке
            foreach (var child in scrollGrid.Children)
            {
                // Нас интересуют только Border, внутри которых лежат Entry
                if (child is Border border && border.Content is Entry entry)
                {
                    // Получаем координаты ячейки в Grid
                    int row = Grid.GetRow(border) - 1; // -1, так как 0-я строка — это заголовок
                    int col = Grid.GetColumn(border);

                    // Проверяем, есть ли данные для этой ячейки в нашем списке
                    if (row >= 0 && row < csvData.Count)
                    {
                        var csvRow = csvData[row];
                        if (col >= 0 && col < csvRow.Count)
                        {
                            // Устанавливаем текст из CSV
                            entry.Text = csvRow[col];
                        }
                    }
                }
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
                    UsefulVolume.ToString(culture),
                    UselessVolume.ToString(culture),
                    GuaranteedDischarge.ToString(culture),
                    FullDischarge.ToString(culture),
                    kHeadLoss.ToString(culture),
                    Efficiency.ToString(culture)
                });

                // 2. Блоки данных (используем обновленный GetRowData)
                string b2 = InflowCount + ";" + GetRowData(InflowTableData, 0, InflowCount);

                string b3 = BathygraphyCount + ";" +
                            GetRowData(BathygraphyTableData, 0, BathygraphyCount) + ";" +
                            GetRowData(BathygraphyTableData, 1, BathygraphyCount);

                string b4 = CharacteristicOfDownstreamCount + ";" +
                            GetRowData(CharacteristicOfDownstreamTableData, 0, CharacteristicOfDownstreamCount) + ";" +
                            GetRowData(CharacteristicOfDownstreamTableData, 1, CharacteristicOfDownstreamCount);

                string b5 = GetRowData(RemainderAccordingDispatchScheduleTableData, 0, 12);
                string b6 = GetRowData(IntakeFromReservoirTableData, 0, 12);

                // 3. Сборка и сохранение
                string content = string.Join(Environment.NewLine, new[] { b1, b2, b3, b4, b5, b6 });

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                var result = await FileSaver.Default.SaveAsync("Input.csv", stream, CancellationToken.None);

                if (result.IsSuccessful)
                    await DisplayAlertAsync("Успех!", "Файл сохранен.\n(разделитель - точка с запятой).", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Ошибка", "Ошибка записи: " + ex.Message, "OK");
            }
        }

        // Универсальный метод с поддержкой форматов чисел
        private string GetRowData(double[,] data, int rowIndex, int count)
        {
            var culture = CultureInfo.InvariantCulture;
            return string.Join(";", Enumerable.Range(0, count).Select(i =>
            {
                var val = data[rowIndex, i];
                // Если это число (double/decimal/float), форматируем с точкой
                return val is IFormattable formattable
                    ? formattable.ToString(null, culture)
                    : (val.ToString() ?? "0");
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
            if(Efficiency >= 1)
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
                RemainderAccordingDispatchScheduleTableData[1, CalendarMonth - 1]; // Диспетчерский остаток 
                                                                                   // в предыдущем месяце (т.к. начинаем с полного водохранилища, то он 
                                                                                   // д.б. равен полезному объему)
            double ResidualVolumeCurrentMonth = 0; // Диспетчерский остаток в текущем месяце
            double RequiredVolumeAccordingDispatchSchedule = 0; // Требуемый объем по диспетчерскому графику
            double[] Consumption = new double [InflowCount]; // Фактический расход ГЭС
            double[] IdleReset = new double [InflowCount]; // Фактические холостые сбросы
            double[] ActualResidualVolume = new double [InflowCount]; // Фактический остаточный объем
                                                                      // над диспетчерским графиком
            double VolumeInReservoir = 0; // Объем в водохранилище
            double DischargeIntoDownstream = 0; // Расход в нижний бьеф
            double[] UpstreamLevel = new double [InflowCount]; // Отметка ВБ
            double[] DownstreamLevel = new double [InflowCount]; // Отметка НБ
            double[] StaticHead = new double [InflowCount]; // Статический напор
            double HeadLoss = 0; // Потери напора
            double[] Power = new double [InflowCount];

            while (MonthOrdinalNumber <= InflowCount)
            {
                // Параметры предыдущего месяца
                CurrentConsumption = GuaranteedDischarge + ExcessVolume / 2.63;
                if (CurrentConsumption > FullDischarge) CurrentConsumption = FullDischarge;
                IdleDischargeFlowRate = 0;

                // Вариант 1 - между диспетчерской линией и НПУ
                IncreaseVolume = (InflowTableData[1, MonthOrdinalNumber] - CurrentConsumption -
                    IdleDischargeFlowRate - IntakeFromReservoirTableData[1, CalendarMonth]) * 2.63;
                ResidualVolumeCurrentMonth = ResidualVolumePreviousMonth + IncreaseVolume;
                RequiredVolumeAccordingDispatchSchedule = 
                    RemainderAccordingDispatchScheduleTableData[1, CalendarMonth];
                
                // Вариант 2 - вышли за НПУ
                if (ResidualVolumeCurrentMonth > UsefulVolume)
                {
                    CurrentConsumption = CurrentConsumption + (ResidualVolumeCurrentMonth - UsefulVolume) / 2.63;
                    if (CurrentConsumption > FullDischarge)
                    // Холостые сбросы
                    {
                        IdleDischargeFlowRate = CurrentConsumption - FullDischarge;
                        CurrentConsumption = FullDischarge;
                        ResidualVolumeCurrentMonth = UsefulVolume;
                    }
                    else
                    // Нет сбросов
                    {
                        IncreaseVolume = (InflowTableData[1, MonthOrdinalNumber] - CurrentConsumption -
                    IdleDischargeFlowRate - IntakeFromReservoirTableData[1, CalendarMonth]) * 2.63;
                        ResidualVolumeCurrentMonth = ResidualVolumePreviousMonth + IncreaseVolume;
                    }
                }
                // Вариант 3 - ниже диспетчерской линии
                else if (ResidualVolumeCurrentMonth < RequiredVolumeAccordingDispatchSchedule)
                {
                    CurrentConsumption = CurrentConsumption + 
                        (ResidualVolumeCurrentMonth - RequiredVolumeAccordingDispatchSchedule) / 2.63;
                    IncreaseVolume = (InflowTableData[1, MonthOrdinalNumber] - CurrentConsumption -
                        IdleDischargeFlowRate - IntakeFromReservoirTableData[1, CalendarMonth]) * 2.63;
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
            }
            // Суммарный объем холостых сбросов
            double MonthIdleResetVolume = 0;
            double SumIdleResetVolume = 0;
            for (int i = 0; i < InflowCount; i++)
            {
                MonthIdleResetVolume = IdleReset[i] * 2.63;
                SumIdleResetVolume =+ MonthIdleResetVolume;
            }

            // Среднегодовая выработка
            double MonthElectricityProduction = 0;
            double SumElectricityProduction = 0;
            double AverageAnnualElectricityGeneration = 0;
            for (int i = 0; i < InflowCount; i++)
            {
                MonthElectricityProduction = Power[i] * 720;
                SumElectricityProduction =+ MonthElectricityProduction;
            }
            AverageAnnualElectricityGeneration = SumElectricityProduction * 12 / InflowCount;
        }

        private double LinearInterpolation (double argument, double[,] xy)
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
                CultureInfo.InvariantCulture, out UsefulVolume)) return;
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
