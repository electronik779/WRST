using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace WRST
{
    public partial class Form2 : Form
    {
        double QRG;
        
        public Form2(DataTable tableResults, DataTable tableSecurity, DataTable tableExtRemainder,
            double EEP, double S, double VU, double QR)
        {
            InitializeComponent();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView1_CellFormatting);
            QRG = QR;

            saveFileDialog1.Filter = "CSV файлы (*.csv)|*.csv";
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.AddExtension = true;

            dataGridView1.DataSource = tableResults;
            dataGridView2.DataSource = tableSecurity;
            dataGridView3.DataSource = tableExtRemainder;

            int ResultCount = tableResults.Rows.Count;
            int SecurityCount = tableSecurity.Rows.Count;

            int[] x = new int[] { 0, 0, 0 };
            int[] y = new int[] { 2, 3, 4 };
            string[] list = new string[] { "Приток", "Расход ГЭС", "Сбросы" };
            string[] list2 = new string[] { "#", "м³/с" };
            BuildChart(chart1, tableResults, "column", list, "left", 3, x, y,
                1, ResultCount, 0, 0, 1, 0, false, list2);
            x = new int[] { 0 };
            y = new int[] { 5 };
            list = new string[] { "Уровень ВБ" };
            list2 = new string[] { "#", "м" };
            BuildChart(chart2, tableResults, "line", list, "left", 1, x, y,
                1, ResultCount, 0, 0, 1, 0, true, list2);
            x = new int[] { 0 };
            y = new int[] { 6 };
            list = new string[] { "Уровень НБ" };
            list2 = new string[] { "#", "м" };
            BuildChart(chart3, tableResults, "column", list, "left", 1, x, y,
                1, ResultCount, 0, 0, 1, 0, true, list2);
            x = new int[] { 0 };
            y = new int[] { 7 };
            list = new string[] { "Напор" };
            list2 = new string[] { "#", "м" };
            BuildChart(chart4, tableResults, "column", list, "left", 1, x, y,
                1, ResultCount, 0, 0, 1, 0, true, list2);
            x = new int[] { 0 };
            y = new int[] { 8 };
            list = new string[] { "Мощность" };
            list2 = new string[] { "#", "кВт" };
            BuildChart(chart5, tableResults, "column", list, "left", 1, x, y,
                1, ResultCount, 0, 0, 1, 0, false, list2);
            x = new int[] { 0 };
            y = new int[] { 1 };
            list = new string[] { "Приток" };
            list2 = new string[] { "Обеспеченность, %", "м³/с" };
            BuildChart(chart6, tableSecurity, "line", list, "right", 1, x, y,
                0, 100, 0, 0, 20, 0, false, list2);
            x = new int[] { 0 };
            y = new int[] { 2 };
            list = new string[] { "Расход ГЭС" };
            list2 = new string[] { "Обеспеченность, %", "м³/с" };
            BuildChart(chart7, tableSecurity, "line", list, "right", 1, x, y,
                0, 100, 0, 0, 20, 0, false, list2);
            x = new int[] { 0 };
            y = new int[] { 3 };
            list = new string[] { "Напор" };
            list2 = new string[] { "Обеспеченность, %", "м" };
            BuildChart(chart8, tableSecurity, "line", list, "right", 1, x, y,
                0, 100, 0, 0, 20, 0, true, list2);
            x = new int[] { 0 };
            y = new int[] { 4 };
            list = new string[] { "Мощность" };
            list2 = new string[] { "Обеспеченность, %", "кВт" };
            BuildChart(chart9, tableSecurity, "line", list, "right", 1, x, y,
                0, 100, 0, 0, 20, 0, false, list2);

            x = new int[] { 0, 0 };
            y = new int[] { 2, 3 };
            list = new string[] { "Диспетчерские остатки - задано", "Диспетчерские остатки - расчет" };
            list2 = new string[] { "#", "млн.м³" };
            BuildChart(chart10, tableExtRemainder, "line", list, "left", 2, x, y,
                1, ResultCount, 0, Convert.ToInt32(VU), 1, Convert.ToInt32(VU / 8), false, list2);

            label2.Text = (Math.Round(EEP, 0)).ToString("#,#", CultureInfo.CurrentCulture);
            label4.Text = (Math.Round(S, 0)).ToString("#,#", CultureInfo.CurrentCulture);

            //Debug.WriteLine("dataGridView1.RowCount= {0}, QR= {1}", dataGridView1.RowCount, QR);
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 3 && Convert.ToDouble(e.Value) < QRG)
            {
                e.CellStyle.BackColor = Color.Coral;
            }
            if (e.ColumnIndex == 3 && Convert.ToDouble(e.Value) < 0)
            {
                e.CellStyle.BackColor = Color.Red;
            }
        }

        private void TableFormat(DataGridView table)
        {
            table.AllowUserToAddRows = false;
            table.AllowUserToDeleteRows = false;
            table.RowHeadersVisible = false;
            table.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            table.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            table.AllowUserToOrderColumns = false;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                table.Columns[i].Width = 100;
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            TableFormat(dataGridView1);
            TableFormat(dataGridView2);
            TableFormat(dataGridView3);
        }

        private void BuildChart(Chart ch, DataTable data,
            string type, string[] list, string pos, int n, int[] x, int[] y,
            int Xmin, int Xmax, int Ymin, int Ymax, int stepX, int stepY, bool isLimit, string[] axis)
        //название диаграммы,
        //название таблицы данных,
        //тип графика: column - столбчетая, остальное - линия,
        //список названий линий,
        //легенда слева - left иначе справа,
        //количество линий,
        //номер столбца DataTable с координатами X,
        //номер столбца DataTable с координатами Y,
        //минимальное значение оси X,
        //максимальное значение оси X,
        //минимальное значение оси Y,
        //максимальное значение оси Y,
        //шаг подписей оси X,
        //шаг подписей оси Y,
        //ограничивать min - max оси Y
        //список названий осей - первая X, вторая Y.
        {
            // Создаем новый объект диаграммы
            ch.ChartAreas.Clear();
            ch.Series.Clear();

            ch.ChartAreas.Add(new ChartArea("ChartArea"));
            ch.ChartAreas[0].AxisX.Minimum = Xmin;
            ch.ChartAreas[0].AxisX.Maximum = Xmax;
            ch.ChartAreas[0].AxisX.Interval = stepX;
            ch.Legends[0].DockedToChartArea = "ChartArea";
            ch.Legends[0].IsDockedInsideChartArea = true;

            if (stepY != 0) ch.ChartAreas[0].AxisY.Interval = stepY;

            if (pos == "left")
            {
                ch.Legends[0].Docking = Docking.Left;
            }

            ch.Legends.Add(new Legend("Legend"));

            if (axis[0] != null)
            {
                ch.ChartAreas[0].AxisX.Title = axis[0];
            }
            if (axis[1] != null)
            {
                ch.ChartAreas[0].AxisY.Title = axis[1];
            }

            for (int seriesNum = 0; seriesNum < n; seriesNum++)
            {
                if (isLimit)
                {
                    int MaxY = Convert.ToInt32(data.Rows[0][y[seriesNum]]);
                    int MinY = Convert.ToInt32(data.Rows[0][y[seriesNum]]);
                    //Debug.WriteLine("{0}, {1}, {2}", seriesNum, MinY, MaxY);
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        double Fig1 = Math.Floor(Convert.ToDouble(data.Rows[i][y[seriesNum]]));
                        double Fig2 = Math.Ceiling(Convert.ToDouble(data.Rows[i][y[seriesNum]]));
                        if (Fig1 < MinY)
                        { MinY = Convert.ToInt32(Fig1); }

                        if (Fig2 > MaxY)
                        { MaxY = Convert.ToInt32(Fig2); }
                    }

                    //Debug.WriteLine("{0}, {1}", MinY, MaxY);
                    ch.ChartAreas[0].AxisY.Minimum = MinY;
                    ch.ChartAreas[0].AxisY.Maximum = MaxY;
                }
                else
                {
                    if (Ymin != Ymax)
                    {
                        ch.ChartAreas[0].AxisY.Minimum = Ymin;
                        ch.ChartAreas[0].AxisY.Maximum = Ymax;
                    }
                }

                // Добавляем серию
                Series series = new Series
                {
                    //ChartType = SeriesChartType.Line,
                    //Color = GetSeriesColor(seriesNum),
                    BorderWidth = 2,
                    Name = list[seriesNum]
                };

                // Цикл по строкам DataTable
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    series.ChartType = SeriesChartType.Line;
                    if (type == "column")
                    {
                        series.ChartType = SeriesChartType.Column;
                    }
                    // Добавить точки для серии диаграммы
                    series.Points.AddXY(data.Rows[i][x[seriesNum]], data.Rows[i][y[seriesNum]]);
                }

                ch.Series.Add(series);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // получаем выбранный файл
                string filename = saveFileDialog1.FileName;
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                using (StreamWriter writer = new StreamWriter(filename, true,
                    System.Text.Encoding.GetEncoding(1251)))
                {
                    List<string> columnsNames = new List<string>()
                   { "#", "Месяц", "Приток, м3/с", "Расход ГЭС, м3/с", "Сбросы, м3/с", "Отм. ВБ, м",
                   "Отм. НБ, м", "Напор, м", "Мощность, кВт", "Избыт. объем, млн.м3"};
                    writer.WriteLine(string.Join(";", columnsNames));

                    //Debug.WriteLine("{0}", dataGridView1.RowCount);
                    //Debug.WriteLine("{0}", dataGridView1.ColumnCount);
                    for (int j = 0; j < dataGridView1.RowCount; j++)
                    {
                        List<string> list = new List<string>();
                        for (int i = 0; i < dataGridView1.ColumnCount; i++)
                        {
                            double tmp;
                            tmp = Convert.ToDouble(dataGridView1.Rows[j].Cells[i].Value);
                            //Debug.WriteLine("{0}, {1}, {2}", j, i, tmp);
                            list.Add(tmp.ToString());
                        }
                        writer.WriteLine(string.Join(';', list));
                    }

                    columnsNames = new List<string>()
                   { "Обеспеченность, %", "Приток, м3/с", "Расход ГЭС, м3/с",
                        "Напор, м", "Мощность, кВт"};
                    writer.WriteLine(string.Join(";", columnsNames));

                    for (int j = 0; j < dataGridView2.RowCount; j++)
                    {
                        List<string> list = new List<string>();
                        for (int i = 0; i < dataGridView2.ColumnCount; i++)
                        {
                            double tmp;
                            tmp = Convert.ToDouble(dataGridView2.Rows[j].Cells[i].Value);
                            list.Add(tmp.ToString());
                        }
                        writer.WriteLine(string.Join(';', list));
                    }

                    columnsNames = new List<string>()
                    { "Среднегодовая выработка, кВт ч"};
                    columnsNames.Add(label2.Text);
                    writer.WriteLine(string.Join(";", columnsNames));

                    columnsNames = new List<string>()
                    { "Суммарный объем сбросов, млн.м3"};
                    columnsNames.Add(label4.Text);
                    writer.WriteLine(string.Join(";", columnsNames)); ;
                }
            }
        }
    }
}
