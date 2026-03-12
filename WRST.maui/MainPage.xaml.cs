using System.Collections.ObjectModel;

namespace WRST.maui
{
    public partial class MainPage : ContentPage
    {
        int StartPosition = 0;

        // Массивы для данных (строки x столбцы)
        string[,] table1Data;
        string[,] table2Data;
        string[,] table3Data;
        string[,] table4Data;
        string[,] table5Data;

        public MainPage()
        {
            InitializeComponent();

            BuildGrid(FixedColumn3, ScrollableGrid3, "12", 1, "T3", 1);
            BuildGrid(FixedColumn4, ScrollableGrid4, "12", 1, "T4", 1);
        }

        private void BeginningMonth_TextChanged(object sender, EventArgs e)
        {
            StartPosition = Convert.ToInt32(BeginningMonth.Text);
        }
        private void InflowButton_Clicked(object sender, EventArgs e) =>
            BuildGrid(FixedColumn1, ScrollableGrid1, InflowCount.Text, 1, "T1", 
                StartPosition);

        private void BathygraphicButton_Clicked(object sender, EventArgs e) =>
            BuildGrid(FixedColumn2, ScrollableGrid2, BathygraphicCount.Text, 2, "T2", 1);

        private void CharacteristicOfDownstreamButton_Clicked(object sender, EventArgs e) =>
            BuildGrid(FixedColumn5, ScrollableGrid5, CharacteristicOfDownstreamCount.Text, 2, "T5", 1);

        private void BuildGrid(Grid fixedGrid, Grid scrollGrid, string colInput, int dataRowCount,
            string prefix, int begin)
        {
            if (!int.TryParse(colInput, out int colCount)) return;

            // Инициализируем массив данных нужного размера
            if (prefix == "T1")      table1Data = new string[dataRowCount, colCount];
            else if (prefix == "T2") table2Data = new string[dataRowCount, colCount];
            else if (prefix == "T3") table3Data = new string[dataRowCount, colCount];
            else if (prefix == "T4") table4Data = new string[dataRowCount, colCount];
            else                     table5Data = new string[dataRowCount, colCount];

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
            var corner = CreateHeaderBorder("", Colors.LightSlateGray); // Верхняя левая ячейка - надпись
            Grid.SetRow(corner, 0);
            fixedGrid.Children.Add(corner);

            // 4. Заголовки столбцов (в прокручиваемой части)
            for (int c = 0; c < colCount; c++)
            {
                int head = c + begin;
                if(head > colCount) head -= colCount;
                var colHeader = CreateHeaderBorder((head).ToString(), Colors.DarkGray);
                Grid.SetRow(colHeader, 0);
                Grid.SetColumn(colHeader, c);
                scrollGrid.Children.Add(colHeader);
            }

            // 5. Заполнение строк
            for (int r = 0; r < dataRowCount; r++)
            {
                // Заголовок строки -> в ФИКСИРОВАННУЮ сетку
                var rowHeader = CreateHeaderBorder($"Строка {r + 1}", Colors.LightGray);
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
                        TextColor = Colors.Black
                    };

                    // Подписка на валидацию и сохранение
                    entry.TextChanged += OnNumericTextChanged;

                    var cellBorder = new Border
                    {
                        Stroke = Colors.Gray,
                        StrokeThickness = 0.5,
                        Content = new Entry { Text = $"{prefix} R{r}C{c}", HorizontalTextAlignment = TextAlignment.Center }
                    };
                    Grid.SetRow(cellBorder, r + 1);
                    Grid.SetColumn(cellBorder, c);
                    scrollGrid.Children.Add(cellBorder);
                }
            }
        }

        private void OnNumericTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
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

            if (parentGrid == ScrollableGrid1)
                table1Data[r, c] = value;
            else if (parentGrid == ScrollableGrid2)
                table2Data[r, c] = value;
            else if (parentGrid == ScrollableGrid3)
                table3Data[r, c] = value;
            else if (parentGrid == ScrollableGrid4)
                table4Data[r, c] = value;
            else table5Data[r, c] = value;
        }

        private void OnTable1TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            var coords = entry.AutomationId.Split(',');
            int r = int.Parse(coords[0]);
            int c = int.Parse(coords[1]);

            table1Data[r, c] = e.NewTextValue; // Сохраняем в массив Таблицы 1
        }

        private void OnTable2TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            var coords = entry.AutomationId.Split(',');
            int r = int.Parse(coords[0]);
            int c = int.Parse(coords[1]);

            table2Data[r, c] = e.NewTextValue; // Сохраняем в массив Таблицы 2
        }

        private void OnTable3TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            var coords = entry.AutomationId.Split(',');
            int r = int.Parse(coords[0]);
            int c = int.Parse(coords[1]);

            table3Data[r, c] = e.NewTextValue; // Сохраняем в массив Таблицы 2
        }

        private void OnTable4TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            var coords = entry.AutomationId.Split(',');
            int r = int.Parse(coords[0]);
            int c = int.Parse(coords[1]);

            table4Data[r, c] = e.NewTextValue; // Сохраняем в массив Таблицы 2
        }

        private void OnTable5TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            var coords = entry.AutomationId.Split(',');
            int r = int.Parse(coords[0]);
            int c = int.Parse(coords[1]);

            table5Data[r, c] = e.NewTextValue; // Сохраняем в массив Таблицы 2
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
        private void Save_Click(object sender, EventArgs e)
        {

        }
        private void Execute_Click(object sender, EventArgs e)
        {

        }
        private void InflowCountInput_TextChanged(object sender, EventArgs e)
        {

        }
        private void BathygraphicCountInput_TextChanged(object sender, EventArgs e)
        {

        }
        private void CharacteristicOfDownstreamCountInput_TextChanged(object sender, EventArgs e)
        {

        }


    }
}
