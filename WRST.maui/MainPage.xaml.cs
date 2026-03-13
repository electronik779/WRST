using System.Text;
using CommunityToolkit.Maui.Storage;

namespace WRST.maui
{
    public partial class MainPage : ContentPage
    {
        int BeginningMonth = 0;
        int InflowCount = 0;
        int BathygraphicCount = 0;
        int CharacteristicOfDownstreamCount = 0;
        double UsefullVolume = 0;
        double UselessVolume = 0;
        double GuaranteedDischarge = 0;
        double FullDischarge = 0;
        double HeadLoss = 0;
        double Efficiency = 0;

        // Массивы для данных (строки x столбцы)
        string[,] InflowTableData = new string[0, 0];
        string[,] BathygraphicTableData = new string[0, 0];
        string[,] RemainderAccordingDispatchScheduleTableData = new string[0, 0];
        string[,] IntakeFromRreservoirTableData = new string[0, 0];
        string[,] CharacteristicOfDownstreamTableData = new string[0, 0];

        public MainPage()
        {
            InitializeComponent();

            List<string> RowNames = new List<string>() { "Остатки, млн.м³" };
            BuildGrid(RemainderAccordingDispatchScheduleFixedColumn,
                RemainderAccordingDispatchScheduleScrollableGrid, 12, 1, "T3", 1, "Месяц", RowNames);

            RowNames.Clear();
            RowNames.Add("Отбор, м³/с");
            BuildGrid(IntakeFromRreservoirFixedColumn, IntakeFromRreservoirScrollableGrid, 12, 1, "T4",
                1, "Месяц", RowNames);
        }

        private void OnRadioChanged(object? sender, CheckedChangedEventArgs e)
        {
            // Нас интересует только момент, когда кнопку выбрали (Value = true)
            if (sender is RadioButton radioButton && e.Value)
            {
                // Проверяем значение Value, которое мы указали в XAML
                string? selectedValue = radioButton.Value?.ToString();

                if (selectedValue == "o")
                {
                    HeadLossLable.Text = "Потери напора, м";
                }
                else if (selectedValue == "g")
                {
                    HeadLossLable.Text = "Коэффициент потерь при Qгэс²";
                }
            }
        }

        private void OnIntEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                // 2. Сама логика проверки одного поля

                string text = entry.Text?.Replace(',', '.') ?? "";

                // Если пусто — считаем ошибкой (или нет, зависит от вашей логики)
                if (string.IsNullOrWhiteSpace(text))
                {
                    entry.TextColor = Colors.Red; // Или Black, если поле необязательное
                }

                // Проверка на число
                bool isValid = int.TryParse(text,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out _);

                entry.TextColor = isValid ? Colors.Black : Colors.Red;
            }
        }

        private void OnDoubleEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                // 2. Сама логика проверки одного поля

                string text = entry.Text?.Replace(',', '.') ?? "";

                // Если пусто — считаем ошибкой (или нет, зависит от вашей логики)
                if (string.IsNullOrWhiteSpace(text))
                {
                    entry.TextColor = Colors.Red; // Или Black, если поле необязательное
                }

                // Проверка на число
                bool isValid = double.TryParse(text,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out _);

                entry.TextColor = isValid ? Colors.Black : Colors.Red;
            }
        }

        private void InflowButton_Clicked(object sender, EventArgs e)
        {
            if (!int.TryParse(BeginningMonthInput.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out BeginningMonth) ||
                !int.TryParse(InflowCountInput.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out InflowCount)) return;

            if (BeginningMonth <= 0) BeginningMonth = 1;
            if (BeginningMonth > 12) BeginningMonth = 12;
            BeginningMonthInput.Text = BeginningMonth.ToString();

            if (InflowCount <= 0) InflowCount = 1;
            if (InflowCount > 600) InflowCount = 600;
            InflowCountInput.Text = InflowCount.ToString();

            if (InflowCount % 12 != 0)
            {
                while (InflowCount % 12 != 0) { InflowCount++; }
                InflowCountInput.Text = InflowCount.ToString();
            }

            List<string> RowNames = new List<string>() { "Приток, м³/с" };
            BuildGrid(InflowFixedColumn, InflowScrollableGrid, InflowCount, 1, "T1",
                        BeginningMonth, "Месяц, #", RowNames);
        }

        private void BathygraphicButton_Clicked(object sender, EventArgs e)
        {
            if (!int.TryParse(BathygraphicCountInput.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out BathygraphicCount)) return;

            if (BathygraphicCount < 2) BathygraphicCount = 2;
            if (BathygraphicCount > 20) BathygraphicCount = 20;
            BathygraphicCountInput.Text = BathygraphicCount.ToString();

            List<string> RowNames = new List<string>() { "Объем, млн.м³", "Отметка ВБ, м" };
            BuildGrid(BathygraphicFixedColumn, BathygraphicScrollableGrid, BathygraphicCount, 2, "T2",
                1, "#", RowNames);
        }

        private void CharacteristicOfDownstreamButton_Clicked(object sender, EventArgs e)
        {
            if (!int.TryParse(CharacteristicOfDownstreamCountInput.Text,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out CharacteristicOfDownstreamCount)) return;

            if (CharacteristicOfDownstreamCount < 2) CharacteristicOfDownstreamCount = 2;
            if (CharacteristicOfDownstreamCount > 20) CharacteristicOfDownstreamCount = 20;
            CharacteristicOfDownstreamCountInput.Text = CharacteristicOfDownstreamCount.ToString();

            List<string> RowNames = new List<string>() { "Расход, м³/с", "Отметка НБ, м" };
            BuildGrid(CharacteristicOfDownstreamFixedColumn, CharacteristicOfDownstreamScrollableGrid,
                CharacteristicOfDownstreamCount, 2, "T5", 1, "#", RowNames);
        }

        private void BuildGrid(Grid fixedGrid, Grid scrollGrid, int colCount, int dataRowCount,
            string prefix, int begin, string headerName, List<string> rowNames)
        {
            // Инициализируем массив данных нужного размера
            if (prefix == "T1") InflowTableData = new string[dataRowCount, colCount];
            else if (prefix == "T2") BathygraphicTableData = new string[dataRowCount, colCount];
            else if (prefix == "T3") RemainderAccordingDispatchScheduleTableData = new string[dataRowCount, colCount];
            else if (prefix == "T4") IntakeFromRreservoirTableData = new string[dataRowCount, colCount];
            else CharacteristicOfDownstreamTableData = new string[dataRowCount, colCount];

            // Очистка обеих частей
            fixedGrid.Children.Clear(); fixedGrid.RowDefinitions.Clear(); fixedGrid.ColumnDefinitions.Clear();
            scrollGrid.Children.Clear(); scrollGrid.RowDefinitions.Clear(); scrollGrid.ColumnDefinitions.Clear();

            // 1. Настройка строк (одинаковая для обеих частей)
            for (int r = 0; r <= dataRowCount; r++) // +1 для заголовка
            {
                var height = r == 0 ? 35 : 40;
                fixedGrid.RowDefinitions.Add(new RowDefinition { Height = height });
                scrollGrid.RowDefinitions.Add(new RowDefinition { Height = height });
            }

            // 2. Настройка столбцов
            fixedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
            for (int c = 0; c < colCount; c++)
                scrollGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 80 });

            // 3. Угловая ячейка (в фиксированной части)
            var corner = CreateHeaderBorder(headerName, Colors.LightSlateGray); // Верхняя левая ячейка - надпись
            Grid.SetRow(corner, 0);
            fixedGrid.Children.Add(corner);

            // 4. Заголовки столбцов (в прокручиваемой части)
            for (int c = 0; c < colCount; c++)
            {
                int head = c + begin;
                if (head > colCount) head -= colCount;
                var colHeader = CreateHeaderBorder((head).ToString(), Colors.DarkGray);
                Grid.SetRow(colHeader, 0);
                Grid.SetColumn(colHeader, c);
                scrollGrid.Children.Add(colHeader);
            }

            // 5. Заполнение строк
            for (int r = 0; r < dataRowCount; r++)
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

        private void OnNumericTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is not Entry entry) return;

            if (string.IsNullOrWhiteSpace(e.NewTextValue)) return;

            // Заменяем точку на запятую (или наоборот) для универсальности
            string cleanValue = e.NewTextValue.Replace(',', '.');

            // Пытаемся преобразовать в число
            if (double.TryParse(cleanValue, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result))
            {
                // Если число валидное — возвращаем обычный цвет и сохраняем
                entry.TextColor = Colors.Black;
                SaveToDataArray(entry.AutomationId, cleanValue, entry);
            }
            else
            {
                // Если ввели буквы — красим текст в красный
                entry.TextColor = Colors.Red;
            }
        }

        private void SaveToDataArray(string authId, string value, Entry entry)
        {
            var coords = authId.Split(',');
            int r = int.Parse(coords[0]);
            int c = int.Parse(coords[1]);

            // Определяем, к какой таблице относится Entry через его родителя (Grid)
            var parentGrid = entry.Parent?.Parent as Grid; // Border -> Grid

            if (parentGrid == InflowScrollableGrid)
                InflowTableData[r, c] = value;
            else if (parentGrid == BathygraphicScrollableGrid)
                BathygraphicTableData[r, c] = value;
            else if (parentGrid == RemainderAccordingDispatchScheduleScrollableGrid)
                RemainderAccordingDispatchScheduleTableData[r, c] = value;
            else if (parentGrid == IntakeFromRreservoirScrollableGrid)
                IntakeFromRreservoirTableData[r, c] = value;
            else CharacteristicOfDownstreamTableData[r, c] = value;
        }

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

        private void Open_Click(object sender, EventArgs e)
        {

        }
        private async void Save_Click(object sender, EventArgs e)
        {
            bool err = false;

            err = CheckData();
            if (err) return;

            GetFields();

            List<string> block1 = new List<string>();
            block1.Add(BeginningMonth.ToString());
            var selectedValue = RadioButtonGroup.GetSelectedValue(RadioGroup)?.ToString();
            if (selectedValue == "o")
            {
                block1.Add("0");
            }
            else if (selectedValue == "g")
            {
                block1.Add("1");
            }
            block1.Add(UsefullVolume.ToString());
            block1.Add(UselessVolume.ToString());
            block1.Add(GuaranteedDischarge.ToString());
            block1.Add(FullDischarge.ToString());
            block1.Add(HeadLoss.ToString());
            block1.Add(Efficiency.ToString());

            List<string> block2 = new List<string>();
            block2.Add(InflowCount.ToString());
            for (int i = 0; i < InflowCount; i++)
            {
                block2.Add(InflowTableData[0, i].ToString());
            }

            List<string> block3 = new List<string>();
            block3.Add(BathygraphicCount.ToString());
            for (int i = 0; i < BathygraphicCount; i++)
            {
                block3.Add(BathygraphicTableData[0, i].ToString());
            }
            for (int i = 0; i < BathygraphicCount; i++)
            {
                block3.Add(BathygraphicTableData[1, i].ToString());
            }

            List<string> block4 = new List<string>();
            block4.Add(CharacteristicOfDownstreamCount.ToString());
            for (int i = 0; i < CharacteristicOfDownstreamCount; i++)
            {
                block4.Add(CharacteristicOfDownstreamTableData[0, i].ToString());
            }
            for (int i = 0; i < CharacteristicOfDownstreamCount; i++)
            {
                block4.Add(CharacteristicOfDownstreamTableData[1, i].ToString());
            }

            List<string> block5 = new List<string>();
            for (int i = 0; i < 12; i++)
            {
                block5.Add(RemainderAccordingDispatchScheduleTableData[0, i].ToString());
            }

            List<string> block6 = new List<string>();
            for (int i = 0; i < 12; i++)
            {
                block6.Add(IntakeFromRreservoirTableData[0, i].ToString());
            }

            string b1 = string.Join(";", block1);
            string b2 = string.Join(";", block2);
            string b3 = string.Join(";", block3);
            string b4 = string.Join(";", block4);
            string b5 = string.Join(";", block5);
            string b6 = string.Join(";", block6);

            List<string> output = new List<string> { b1, b2, b3, b4, b5, b6 };

            string content = string.Join(Environment.NewLine, output);

            try
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                string defaultName = "Result.csv";

                var result = await FileSaver.Default.SaveAsync(defaultName, stream, CancellationToken.None);

                if (result.IsSuccessful)
                {
                    await DisplayAlertAsync("Успех", "Файл сохранен", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Ошибка", ex.Message, "OK");
            }
        }
        private void Execute_Click(object sender, EventArgs e)
        {
            bool err = false;

            err = CheckData();
            if (err) return;

            GetFields();
        }

        private void GetFields()
        {
            if (!double.TryParse(UsefullVolumeInput.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out UsefullVolume)) return;
            if (!double.TryParse(UselessVolumeInput.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out UselessVolume)) return;
            if (!double.TryParse(GuaranteedDischargeInput.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out GuaranteedDischarge)) return;
            if (!double.TryParse(FullDischargeInput.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out FullDischarge)) return;
            if (!double.TryParse(HeadLossInput.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out HeadLoss)) return;
            if (!double.TryParse(EfficiencyInput.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out Efficiency)) return;
        }

        private bool CheckData()
        {
            bool err = false;

            err = CheckIntVariable("Номер календарного месяца начала расчета", BeginningMonthInput.Text);

            err = CheckIntVariable("Количество значений притока", InflowCountInput.Text);

            err = CheckDataTable("Приток", InflowTableData, InflowCount, 1);

            err = CheckDoubleVariable("Полезный объем водохранилища", UsefullVolumeInput.Text);

            err = CheckDoubleVariable("Мертвый объем водохранилища", UselessVolumeInput.Text);

            err = CheckIntVariable("Количество точек батиграфической характеристики",
                BathygraphicCountInput.Text);

            err = CheckDataTable("Батиграфическая характеристика", BathygraphicTableData,
                BathygraphicCount, 2);

            err = CheckDataTable("Диспетчерские остатки", RemainderAccordingDispatchScheduleTableData,
                12, 1);

            err = CheckDataTable("Отбор из водохранилища", IntakeFromRreservoirTableData, 12, 1);

            err = CheckIntVariable("Количество точек характеристики нижнего бьефа",
                CharacteristicOfDownstreamCountInput.Text);

            err = CheckDataTable("Характеристика нижнего бьефа", CharacteristicOfDownstreamTableData,
                CharacteristicOfDownstreamCount, 2);

            err = CheckDoubleVariable("Гарантированный расход ГЭС", GuaranteedDischargeInput.Text);

            err = CheckDoubleVariable("Полный (максимальный) расход ГЭС", FullDischargeInput.Text);

            err = CheckDoubleVariable("Потери напора", HeadLossInput.Text);

            err = CheckDoubleVariable("Кпд гидроагрегата", EfficiencyInput.Text);

            return err;
        }

        private bool CheckDataTable(string tableName, string[,] table, int colCount, int rowCount)
        {
            bool err = false;
            for (int i = 0; i < colCount; i++)
            {
                for (int j = 0; j < rowCount; j++)
                {
                    if (!double.TryParse(table[j, i], System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out _))
                        err = true;
                }
            }
            if (err) DisplayAlertAsync("Ошибка!", $"В таблице «{tableName}» введены неверные данные.", "OK");
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

            if (!double.TryParse(variable, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double _result))
                err = true;
            if (err) DisplayAlertAsync("Ошибка!", $"«{varName}» - введены неверные данные.", "OK");
            return err;
        }


    }
}
