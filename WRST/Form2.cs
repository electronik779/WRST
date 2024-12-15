using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WRST
{
    public partial class Form2 : Form
    {
        public Form2(DataTable tableResults, DataTable tableSecurity)
        {
            InitializeComponent();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            saveFileDialog1.Filter = "CSV файлы (*.csv)|*.csv";
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.AddExtension = true;

            dataGridView1.DataSource = tableResults;
            dataGridView2.DataSource = tableSecurity;
            int ResultCount = tableResults.Rows.Count;
            int SecurityCount = tableSecurity.Rows.Count;

            int[] x = new int[] { 0, 0, 0 };
            int[] y = new int[] { 2, 3, 4 };
            string[] list = new string[] { "Приток", "Расход ГЭС", "Сбросы" };
            string[] list2 = new string[] { "Месяц", "м3/с" };
            BuildChart(chart1, tableResults, "column", list, "left", 3, x, y, 
                1, ResultCount, 1, false, list2);
            x = new int[] { 0 };
            y = new int[] { 5 };
            list = new string[] { "Уровень ВБ" };
            list2 = new string[] { "Месяц", "м" };
            BuildChart(chart2, tableResults, "line", list, "left", 1, x, y, 
                1, ResultCount, 1, true, list2);
            x = new int[] { 0 };
            y = new int[] { 6 };
            list = new string[] { "Уровень НБ, м" };
            list2 = new string[] { "Месяц", "м" };
            BuildChart(chart3, tableResults, "line", list, "left", 1, x, y, 
                1, ResultCount, 1, true, list2);
            x = new int[] { 0 };
            y = new int[] { 7 };
            list = new string[] { "Напор" };
            list2 = new string[] { "Месяц", "м" };
            BuildChart(chart4, tableResults, "column", list, "left", 1, x, y, 
                1, ResultCount, 1, true, list2);
            x = new int[] { 0 };
            y = new int[] { 8 };
            list = new string[] { "Мощность" };
            list2 = new string[] { "Месяц", "кВт" };
            BuildChart(chart5, tableResults, "column", list, "left", 1, x, y, 
                1, ResultCount, 1, false, list2);
            x = new int[] { 0 };
            y = new int[] { 1 };
            list = new string[] { "Приток" };
            list2 = new string[] { "Обеспеченность, %", "м3/с" };
            BuildChart(chart6, tableSecurity, "line", list, "right", 1, x, y, 
                0, 100, 20, false, list2);
            x = new int[] { 0 };
            y = new int[] { 2 };
            list = new string[] { "Расход ГЭС" };
            list2 = new string[] { "Обеспеченность, %", "м3/с" };
            BuildChart(chart7, tableSecurity, "line", list, "right", 1, x, y,
                0, 100, 20, false, list2);
            x = new int[] { 0 };
            y = new int[] { 3 };
            list = new string[] { "Напор" };
            list2 = new string[] { "Обеспеченность, %", "м" };
            BuildChart(chart8, tableSecurity, "line", list, "right", 1, x, y,
                0, 100, 20, true, list2);
            x = new int[] { 0 };
            y = new int[] { 4 };
            list = new string[] { "Мощность" };
            list2 = new string[] { "Обеспеченность, %", "кВт" };
            BuildChart(chart9, tableSecurity, "line", list, "right", 1, x, y,
                0, 100, 20, false, list2);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.AllowUserToOrderColumns = false;

            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.AllowUserToOrderColumns = false;
        }

        private void BuildChart(Chart ch, DataTable data,
            string type, string[] list, string pos, int n, int[] x, int[] y, 
            int Xmin, int Xmax, int step, bool isLimit, string[] axis)
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
        //шаг подписей оси X,
        //ограничивать min - max оси Y
        //список названий осей - первая X, вторая Y.
        {
            // Создаем новый объект диаграммы
            ch.ChartAreas.Clear();
            ch.Series.Clear();

            ch.ChartAreas.Add(new ChartArea("ChartArea"));
            ch.ChartAreas[0].AxisX.Minimum = Xmin;
            ch.ChartAreas[0].AxisX.Maximum = Xmax;
            ch.ChartAreas[0].AxisX.Interval = step;
            ch.Legends[0].DockedToChartArea = "ChartArea";
            ch.Legends[0].IsDockedInsideChartArea = true;

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
                            list.Add (tmp.ToString());
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
                }
            }
        }

        private void chart6_Click(object sender, EventArgs e)
        {

        }
    }
}
