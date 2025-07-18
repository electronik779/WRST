﻿using System.Data;
using System.Globalization;

namespace WRST
{
    public partial class Form1 : Form
    {
        //Таблицы по которым строим tableGridView
        DataTable tableTributary = new DataTable();   // Приток
        DataTable tableUpstream = new DataTable();    // Параметры вдхр
        DataTable tableDownstream = new DataTable();  // Батиграфия НБ
        DataTable tableRemainder = new DataTable();   // Диспетчерские остатки
        DataTable tableSelections = new DataTable();  // Отборы из вдхр
        DataTable tableResults = new DataTable();
        DataTable tableSecurity = new DataTable();

        DataTable tableExtRemainder = new DataTable();

        public Form1()
        {
            InitializeComponent();

            openFileDialog1.Filter = "CSV файлы (*.csv)|*.csv";
            saveFileDialog1.Filter = "CSV файлы (*.csv)|*.csv";
            saveFileDialog1.DefaultExt = "csv";
            saveFileDialog1.AddExtension = true;
        }

        private void TableFormat(DataGridView table, bool HeaderVisible)
        {
            table.AllowUserToAddRows = false;
            table.AllowUserToDeleteRows = false;
            table.RowHeadersVisible = false;
            table.ColumnHeadersVisible = HeaderVisible;
            table.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            table.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            table.AllowUserToOrderColumns = false;
        }

        private void TableInit(DataTable table, DataGridView tableG, int rows)
        {
            table.Columns.Add(new DataColumn("0", typeof(string))); //Создаем столбец
            for (int i = 0; i < rows; i++)
            {
                DataRow row = table.NewRow(); //Добавляем строку
                row[0] = 0; //Задаем данные (номер столбца - 0) 
                table.Rows.Add(row); //Добавляем данные в таблицу
            }
            tableG.DataSource = table; //Привязываем таблицу к tableGridView
        }

        private void TableHeader(DataTable table, DataGridView tableG)
        {
            for (int i = 0; i < 12; i++)
            {
                if (i < 9)
                {
                    table.Columns.Add(new DataColumn("0" + (i + 1).ToString(), typeof(string)));
                }
                else
                {
                    table.Columns.Add(new DataColumn((i + 1).ToString(), typeof(string)));
                }
            }
            DataRow row = table.NewRow();
            for (int i = 0; i < 12; i++)
            {
                row[i] = 0;
            }
            table.Rows.Add(row);
            tableG.DataSource = table;
            for (int i = 0; i < 12; i++)
            {
                tableG.Columns[i].Width = 100;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Оформление таблиц

            TableFormat(dataGridView1, true);
            TableFormat(dataGridView2, false);
            TableFormat(dataGridView3, false);
            TableFormat(dataGridView4, true);
            TableFormat(dataGridView5, true);

            //Начальная инициализация
            TableInit(tableTributary, dataGridView1, 1);
            TableInit(tableUpstream, dataGridView2, 2);
            TableInit(tableDownstream, dataGridView3, 2);

            textBox1.Text = "0";
            textBox2.Text = "0";
            textBox3.Text = "0";
            textBox4.Text = "0";
            textBox5.Text = "0";
            textBox6.Text = "0";
            textBox7.Text = "0";
            textBox8.Text = "0";
            textBox9.Text = "0";
            textBox10.Text = "0";

            TableHeader(tableRemainder, dataGridView4);
            TableHeader(tableSelections, dataGridView5);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            label20.Text = "Коэффициент потерь при Qгэс²";
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            label20.Text = "Потери напора, м";
        }

        private void TableClear(DataTable table)
        {
            table.Clear();
            for (int i = table.Columns.Count - 1; i >= 0; i--)
            {
                table.Columns.RemoveAt(i);
            }
        }

        private void TableCreate(DataTable table, DataGridView tableG, int cols, int rows)
        {
            for (int i = 0; i < cols; i++)
            {
                table.Columns.Add(new DataColumn((i + 1).ToString(), typeof(string)));
            }
            //Заполняем строку нулями
            for (int j = 0; j < rows; j++)
            {
                DataRow row = table.NewRow();
                for (int i = 0; i < cols; i++)
                {
                    row[i] = 0;
                }
                table.Rows.Add(row);
            }
            tableG.DataSource = table;
            for (int i = 0; i < cols; i++)
            {
                tableG.Columns[i].Width = 100;
            }
        }

        private void button1_Click(object sender, EventArgs e) // Создать табл. приток
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox8.Text))
                return;

            int m;
            try
            {
                m = Convert.ToInt32(textBox1.Text);
                if (m > 12) { LimitMsg("12"); m = 12; textBox1.Text = "12"; }//Месяцев в году 12
            }
            //catch (Exception ex)
            catch
            {
                //textBox1.BackColor = Color.Red;
                //MessageBox.Show("Необходимо ввести целое число. \n\n" + ex, "Внимание!",
                //    MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                NotInteger(textBox1);
                return;
            }

            if (m == 0)
            {
                MessageBox.Show("Необходимо задать номер календарного месяца, с которого начинается расчетный ряд.", "Внимание!",
                    MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                return;
            }

            int n;
            try
            {
                n = Convert.ToInt32(textBox8.Text);
                if (n > 600) { LimitMsg("600"); n = 600; textBox8.Text = "600"; }//Кол-во эл-тов массива Q
            }
            catch
            {
                NotInteger(textBox8);
                return;
            }
            while (n % 12 != 0) { n++; }
            textBox8.Text = n.ToString();
            //int years = n / 12;

            button1.Enabled = false;

            //Очищаем таблицу

            TableClear(tableTributary);

            //Создаем таблицу по заданному количеству столбцов

            int years = n / 12;
            for (int i = 0; i < n; i++)
            {
                if (m > years * 12) { m = 1; }
                tableTributary.Columns.Add(new DataColumn(m.ToString(), typeof(string)));
                m++;
            }
            //Заполняем строку нулями

            DataRow row = tableTributary.NewRow();
            for (int i = 0; i < n; i++)
            {
                row[i] = 0;
            }
            tableTributary.Rows.Add(row);
            dataGridView1.DataSource = tableTributary;
            for (int i = 0; i < n; i++)
            {
                dataGridView1.Columns[i].Width = 100;
            }
        }

        private void button2_Click(object sender, EventArgs e)  // Создать табл. вдхр
        {
            if (string.IsNullOrEmpty(textBox9.Text))
                return;

            button2.Enabled = false;

            int n;
            try
            {
                n = Convert.ToInt32(textBox9.Text);
                if (n > 20) { LimitMsg("20"); n = 20; textBox9.Text = "20"; }//Кол-во эл-тов массивов VV и ZUU
            }
            catch
            {
                NotInteger(textBox9);
                return;
            }

            TableClear(tableUpstream);

            TableCreate(tableUpstream, dataGridView2, n, 2);
        }

        private void button3_Click(object sender, EventArgs e)  // Создать табл. НБ
        {
            if (string.IsNullOrEmpty(textBox10.Text))
                return;

            button3.Enabled = false;

            int n;
            try
            {
                n = Convert.ToInt32(textBox10.Text);
                if (n > 20) { LimitMsg("20"); n = 20; textBox10.Text = "20"; }//Кол-во эл-тов массива QLL и ZLL
            }
            catch
            {
                NotInteger(textBox10);
                return;
            }

            TableClear(tableDownstream);

            TableCreate(tableDownstream, dataGridView3, n, 2);
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // сброс цвета фона textBox
            if (textBox1.BackColor == Color.Red) { textBox1.BackColor = SystemColors.Window; }
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // сброс цвета фона textBox
            if (textBox2.BackColor == Color.Red) { textBox2.BackColor = SystemColors.Window; }
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // сброс цвета фона textBox
            if (textBox3.BackColor == Color.Red) { textBox3.BackColor = SystemColors.Window; }
        }
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // сброс цвета фона textBox
            if (textBox4.BackColor == Color.Red) { textBox4.BackColor = SystemColors.Window; }
        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            // сброс цвета фона textBox
            if (textBox5.BackColor == Color.Red) { textBox5.BackColor = SystemColors.Window; }
        }
        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            // сброс цвета фона textBox
            if (textBox6.BackColor == Color.Red) { textBox6.BackColor = SystemColors.Window; }
        }
        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            // сброс цвета фона textBox
            if (textBox7.BackColor == Color.Red) { textBox7.BackColor = SystemColors.Window; }
        }
        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
            if (textBox8.BackColor == Color.Red) { textBox8.BackColor = SystemColors.Window; }
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
            if (textBox9.BackColor == Color.Red) { textBox9.BackColor = SystemColors.Window; }
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
            if (textBox10.BackColor == Color.Red) { textBox10.BackColor = SystemColors.Window; }
        }

        private void toolStripButton1_Click(object sender, EventArgs e) //Сохранение
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // получаем выбранный файл
                string filename = saveFileDialog1.FileName;
                //если существует - удаляем
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                List<string> block1 = new List<string>();
                block1.Add(textBox1.Text);
                if (radioButton1.Checked == true)
                { block1.Add("0"); }
                else
                { block1.Add("1"); }
                block1.Add(textBox2.Text);
                block1.Add(textBox3.Text);
                block1.Add(textBox4.Text);
                block1.Add(textBox5.Text);
                block1.Add(textBox6.Text);
                block1.Add(textBox7.Text);

                List<string> block2 = new List<string>();
                block2.Add(textBox8.Text);
                double tmp;
                for (int i = 0; i < Convert.ToInt32(textBox8.Text); i++)
                {
                    tmp = GetDouble((string)dataGridView1.Rows[0].Cells[i].Value, 0d);
                    block2.Add(Convert.ToString(tmp));
                }

                List<string> block3 = new List<string>();
                block3.Add(textBox9.Text);
                for (int i = 0; i < Convert.ToInt32(textBox9.Text); i++)
                {
                    tmp = GetDouble((string)dataGridView2.Rows[0].Cells[i].Value, 0d);
                    block3.Add(Convert.ToString(tmp));
                }
                for (int i = 0; i < Convert.ToInt32(textBox9.Text); i++)
                {
                    tmp = GetDouble((string)dataGridView2.Rows[1].Cells[i].Value, 0d);
                    block3.Add(Convert.ToString(tmp));
                }

                List<string> block4 = new List<string>();
                block4.Add(textBox10.Text);
                for (int i = 0; i < Convert.ToInt32(textBox10.Text); i++)
                {
                    tmp = GetDouble((string)dataGridView3.Rows[0].Cells[i].Value, 0d);
                    block4.Add(Convert.ToString(tmp));
                }
                for (int i = 0; i < Convert.ToInt32(textBox10.Text); i++)
                {
                    tmp = GetDouble((string)dataGridView3.Rows[1].Cells[i].Value, 0d);
                    block4.Add(Convert.ToString(tmp));
                }

                List<string> block5 = new List<string>();
                for (int i = 0; i < 12; i++)
                {
                    tmp = GetDouble((string)dataGridView4.Rows[0].Cells[i].Value, 0d);
                    block5.Add(Convert.ToString(tmp));
                }

                List<string> block6 = new List<string>();
                for (int i = 0; i < 12; i++)
                {
                    tmp = GetDouble((string)dataGridView5.Rows[0].Cells[i].Value, 0d);
                    block6.Add(Convert.ToString(tmp));
                }
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.WriteLine(string.Join(";", block1));
                    writer.WriteLine(string.Join(";", block2));
                    writer.WriteLine(string.Join(";", block3));
                    writer.WriteLine(string.Join(";", block4));
                    writer.WriteLine(string.Join(";", block5));
                    writer.WriteLine(string.Join(";", block6));
                }
            }
        }

        private void TableFill(DataTable table, DataGridView tableG, List<string> block, int cols, int rows, int shift)
        {
            for (int i = 0; i < cols; i++)
            {
                table.Rows[0][i] = block?.ElementAtOrDefault(i + shift) ?? string.Empty;
            }
            if (rows == 2)
            {
                for (int i = cols; i < cols * 2; i++)
                {
                    table.Rows[1][i - cols] = block?.ElementAtOrDefault(i + shift) ?? string.Empty;
                }
            }
            tableG.DataSource = table;
            for (int i = 0; i < cols; i++)
            {
                tableG.Columns[i].Width = 100;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e) // Загрузка
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // получаем выбранный файл
                string filename = openFileDialog1.FileName;

                List<List<string>> blocks = new List<List<string>>();

                using (StreamReader reader = new StreamReader(filename))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        List<string> row = line.Split(';').ToList();
                        blocks.Add(row);
                    }
                }
                List<string> block1 = blocks.ElementAtOrDefault(0);
                List<string> block2 = blocks.ElementAtOrDefault(1);
                List<string> block3 = blocks.ElementAtOrDefault(2);
                List<string> block4 = blocks.ElementAtOrDefault(3);
                List<string> block5 = blocks.ElementAtOrDefault(4);
                List<string> block6 = blocks.ElementAtOrDefault(5);

                try
                {
                    textBox1.Text = block1?.ElementAtOrDefault(0) ?? string.Empty;
                    string tmp;
                    tmp = block1?.ElementAtOrDefault(1) ?? string.Empty;
                    if (tmp == "0") { radioButton1.Checked = true; }
                    else { radioButton2.Checked = true; }
                    textBox2.Text = block1?.ElementAtOrDefault(2) ?? string.Empty;
                    textBox3.Text = block1?.ElementAtOrDefault(3) ?? string.Empty;
                    textBox4.Text = block1?.ElementAtOrDefault(4) ?? string.Empty;
                    textBox5.Text = block1?.ElementAtOrDefault(5) ?? string.Empty;
                    textBox6.Text = block1?.ElementAtOrDefault(6) ?? string.Empty;
                    textBox7.Text = block1?.ElementAtOrDefault(7) ?? string.Empty;

                    textBox8.Text = block2?.ElementAtOrDefault(0) ?? string.Empty;
                    int n = Convert.ToInt32(textBox8.Text);
                    int m = Convert.ToInt32(textBox1.Text);
                    int years = n / 12;
                    if (dataGridView1.ColumnCount != n)
                    {
                        //Очищаем таблицу
                        TableClear(tableTributary);
                        //Создаем таблицу по заданному количеству столбцов
                        for (int i = 0; i < n; i++)
                        {
                            if (m > years * 12) { m = 1; }
                            tableTributary.Columns.Add(new DataColumn(m.ToString(), typeof(string)));
                            m++;
                        }
                    }
                    //Debug.WriteLine("1 {0}", dataGridView1.RowCount);
                    if (dataGridView1.RowCount != 1)
                    {
                        DataRow rowTributary = tableTributary.NewRow();
                        for (int i = 0; i < 1; i++)
                        {
                            rowTributary[i] = 0;
                        }
                        tableTributary.Rows.Add(rowTributary);
                        dataGridView1.DataSource = tableTributary;
                    }

                    //Debug.WriteLine("2 {0}", dataGridView1.RowCount);

                    TableFill(tableTributary, dataGridView1, block2, n, 1, 1);

                    textBox9.Text = block3?.ElementAtOrDefault(0) ?? string.Empty;
                    n = Convert.ToInt32(textBox9.Text);
                    if (dataGridView2.ColumnCount != n)
                    {
                        TableClear(tableUpstream);

                        TableCreate(tableUpstream, dataGridView2, n, 2);
                    }
                    TableFill(tableUpstream, dataGridView2, block3, n, 2, 1);


                    textBox10.Text = block4?.ElementAtOrDefault(0) ?? string.Empty;
                    n = Convert.ToInt32(textBox10.Text);
                    if (dataGridView3.ColumnCount != n)
                    {
                        TableClear(tableDownstream);

                        TableCreate(tableDownstream, dataGridView3, n, 2);
                    }
                    TableFill(tableDownstream, dataGridView3, block4, n, 2, 1);

                    TableFill(tableRemainder, dataGridView4, block5, 12, 1, 0);

                    TableFill(tableSelections, dataGridView5, block6, 12, 1, 0);
                }
                catch (Exception ex)
                {
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    textBox3.Text = "0";
                    textBox4.Text = "0";
                    textBox5.Text = "0";
                    textBox6.Text = "0";
                    textBox7.Text = "0";
                    textBox8.Text = "0";
                    textBox9.Text = "0";
                    textBox10.Text = "0";

                    MessageBox.Show("Неверный формат файла исходных данных " +
                        "/ файл исходных данных повреждён \n\n" + ex, "Внимание!",
                    MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            // Проверяем, существует ли Form2
            if (Application.OpenForms["Form2"] != null)
            {
                // Если Form2 открыта, закрываем ее
                ((Form)Application.OpenForms["Form2"]).Close();
            }

            int MF = 0;
            int M1 = 0;
            int NF = 0;
            int JF = 0;
            bool LA = false;
            double[] Q = new double[600];
            double[] VV = new double[20];
            double[] ZUU = new double[20];
            double[] QLL = new double[20];
            double[] ZLL = new double[20];
            double VU = 0;
            double VR = 0;
            double QR = 0;
            double QPF = 0;
            double DK = 0;
            double EFF = 0;
            double[] VD = new double[12];
            double[] QU = new double[12];

            try
            {
                MF = Convert.ToInt32(textBox8.Text);
                if (MF == 0) { ZeroMsg(textBox8, "Приток"); }
                //if (MF > 600) { LimitMsg("600"); MF = 600; textBox8.Text = "600"; }
            }
            catch
            {
                NotInteger(textBox8);
                return;
            }
            try
            {
                M1 = Convert.ToInt32(textBox1.Text) - 1;
                if (M1 < 0) { ZeroMsg(textBox1, "Общие данные"); }
                if (M1 > 11) { LimitMsg("12"); M1 = 11; textBox1.Text = "12"; }//Кол-во месяцев в году
            }
            catch
            {
                NotInteger(textBox1);
                return;
            }
            try
            {
                NF = Convert.ToInt32(textBox9.Text);
                if (NF == 0) { ZeroMsg(textBox9, "Параметры вдхр."); }
                //if (NF > 20) { LimitMsg("20"); NF = 20; textBox9.Text = "20"; }
            }
            catch
            {
                NotInteger(textBox9);
                return;
            }
            try
            {
                JF = Convert.ToInt32(textBox10.Text);
                if (JF == 0) { ZeroMsg(textBox10, "Параметры НБ"); }
                //if (JF > 20) { LimitMsg("20"); JF = 20; textBox10.Text = "20"; }
            }
            catch
            {
                NotInteger(textBox10);
                return;
            }

            LA = false;
            if (radioButton2.Checked == true) { LA = true; }

            for (int i = 0; i < MF; i++)
            {
                try
                {
                    Q[i] = GetDouble((string)dataGridView1.Rows[0].Cells[i].Value, 0d);
                }
                catch (Exception ex)
                {
                    NotDouble(ex, "Вкладка Приток: \nГде-то в таблице введено не число.");
                    return;
                }
            }

            for (int i = 0; i < NF; i++)
            {
                try
                {
                    VV[i] = GetDouble((string)dataGridView2.Rows[0].Cells[i].Value, 0d);
                    ZUU[i] = GetDouble((string)dataGridView2.Rows[1].Cells[i].Value, 0d);
                    //Debug.WriteLine("{0}, {1}", i, ZUU[i]);
                }
                catch (Exception ex)
                {
                    NotDouble(ex, "Вкладка Параметры вдхр.: \nГде-то в таблице введено не число.");
                    return;
                }
            }
            if (!CheckArrayOrder(VV, NF))
            {
                TableErr("Параметры вдхр.\nОбъемы");
                return;
            }

            for (int i = 0; i < JF; i++)
            {
                try
                {
                    QLL[i] = GetDouble((string)dataGridView3.Rows[0].Cells[i].Value, 0d);
                    ZLL[i] = GetDouble((string)dataGridView3.Rows[1].Cells[i].Value, 0d);
                }
                catch (Exception ex)
                {
                    NotDouble(ex, "Вкладка Батиграфия НБ: \nГде-то в таблице введено не число.");
                    return;
                }
            }
            if (!CheckArrayOrder(QLL, JF))
            {
                TableErr("Параметры НБ.\nРасходы");
                return;
            }

            try
            {
                VU = GetDouble(textBox2.Text, 0d);
                if (VU == 0) { ZeroMsg(textBox2, "Общие данные"); }
            }
            catch (Exception ex)
            {
                textBox2.BackColor = Color.Red;
                NotDouble(ex, "");
                return;
            }
            try
            {
                VR = GetDouble(textBox3.Text, 0d);
                if (VR == 0) { ZeroMsg(textBox3, "Общие данные"); }
            }
            catch (Exception ex)
            {
                textBox3.BackColor = Color.Red;
                NotDouble(ex, "");
                return;
            }
            try
            {
                QR = GetDouble(textBox4.Text, 0d);
                if (QR == 0) { ZeroMsg(textBox4, "Общие данные"); }
            }
            catch (Exception ex)
            {
                textBox4.BackColor = Color.Red;
                NotDouble(ex, "");
                return;
            }
            try
            {
                QPF = GetDouble(textBox5.Text, 0d);
                if (QPF == 0) { ZeroMsg(textBox5, "Общие данные"); }
            }
            catch (Exception ex)
            {
                textBox5.BackColor = Color.Red;
                NotDouble(ex, "");
                return;
            }
            try
            {
                DK = GetDouble(textBox6.Text, 0d);
            }
            catch (Exception ex)
            {
                textBox6.BackColor = Color.Red;
                NotDouble(ex, "");
                return;
            }
            try
            {
                EFF = GetDouble(textBox7.Text, 0d);
                if (EFF == 0) { ZeroMsg(textBox7, "Общие данные"); }
            }
            catch (Exception ex)
            {
                textBox7.BackColor = Color.Red;
                NotDouble(ex, "");
                return;
            }
            VD = new double[12];
            QU = new double[12];
            for (int i = 0; i < 12; i++)
            {
                try
                {
                    VD[i] = GetDouble((string)dataGridView4.Rows[0].Cells[i].Value, 0d);
                }
                catch (Exception ex)
                {
                    NotDouble(ex, "Вкладка Диспетчерские остатки: \nГде-то в таблице введено не число.");
                    return;
                }
                try
                {
                    QU[i] = GetDouble((string)dataGridView5.Rows[0].Cells[i].Value, 0d);
                }
                catch (Exception ex)
                {
                    NotDouble(ex, "Вкладка Отбор из вдхр.: \nГде-то в таблице введено не число.");
                    return;
                }
            }

            double[] DVM = new double[600];
            double QP1;
            double QS1;
            double DV1;
            double VM1;
            double VD1;
            double[] MDK = new double[600];
            double[] QP = new double[600];
            double[] QS = new double[600];
            double[] ZU = new double[600];
            double[] ZL = new double[600];
            double[] PH = new double[600];
            double[] PN = new double[600];
            double PHL;
            double VM11;
            double ZU1;
            double QL1;
            double ZL1;
            double PH1;
            double PN1;

            double[] B_Q = new double[600];
            double[] B_QP = new double[600];
            double[] B_PH = new double[600];
            double[] B_PN = new double[600]; ;

            int M = 0;
            int MD = M1 - 1;
            if (MD <= -1) { MD = 11; }
            double VM = VD[MD];
            double DV = 0;

            while (M <= MF)
            {
                QP1 = QR + DV / 2.63;
                //Debug.WriteLine("Start[{0}]. QR={1}, DV={2}, QP1={3}", M, QR, DV, QP1);
                if (QP1 > QPF) { QP1 = QPF; }
                QS1 = 0;
                MD++;
                if (MD > 11) { MD = 0; }
                DV1 = (Q[M] - QP1 - QS1 - QU[MD]) * 2.63;
                //Debug.WriteLine("Start[{0}]. DV1={1}, Q[M]={2}, QP1={3}, QS1={4} QU[MD]={5}, MD={6}",
                //    M, DV1, Q[M], QP1, QS1, QU[MD], MD);
                VM1 = VM + DV1;
                //Debug.WriteLine("VM={0}, DV1={1}, VM1={2}", VM, DV1, VM1);
                VD1 = VD[MD];
                //Debug.WriteLine("VD[MD]={0}, MD={1}", VD1, MD);
                //Debug.WriteLine("IF VU={0}, VM1={1}, VD1={2}", VU, VM1, VD1);
                if (VM1 > VU | VM1 < VD1)
                {
                    if (VM1 > VU)
                    {
                        QP1 = QP1 + (VM1 - VU) / 2.63;
                        //Debug.WriteLine("VM1>VU[{0}]. VM1={1}, VU={2}, QP1={3}",M, VM1, VU, QP1);
                        if (QP1 <= QPF)
                        {
                            DV1 = (Q[M] - QP1 - QS1 - QU[MD]) * 2.63;
                            //Debug.WriteLine("VM1 > VU . QP1 <= QPF[{0}]. DV1={1}", M, DV);
                            VM1 = VM + DV1;
                        }
                        else
                        {
                            QS1 = QP1 - QPF;
                            QP1 = QPF;
                            //Debug.WriteLine("VM1>VU, QP1>QPF[{0}]", M);
                            VM1 = VU;
                        }
                    }
                    else
                    {
                        QP1 = QP1 + (VM1 - VD1) / 2.63;
                        //Debug.WriteLine("VM1<=VU[{0}]. VM1={1}, VD1={2}, QP1={3}", M, VM1, VD1, QP1);
                        DV1 = (Q[M] - QP1 - QS1 - QU[MD]) * 2.63;
                        //Debug.WriteLine("VM1 <= VU");
                        VM1 = VM + DV1;
                    }
                }
                MDK[M] = MD;
                QP[M] = QP1;
                QS[M] = QS1;
                DV = VM1 - VD1;
                //Debug.WriteLine("VM1={0}, VD1={1}, DV={2}",VM1, VD1, DV);
                DVM[M] = DV;
                VM11 = VM1 + VR;
                //Debug.WriteLine("BB");
                ZU1 = Lag11(VM11, NF, VV, ZUU);
                //Debug.WriteLine("BB[{0}]. VM1={1}, VR={2}, VM11={3}, ZU1={4}", M, VM1, VR, VM11, ZU1);
                QL1 = QP1 + QS1;
                //Debug.WriteLine("HB");
                ZL1 = Lag11(QL1, JF, QLL, ZLL);
                //Debug.WriteLine("HB[{0}]. QP1={1}, QS1={2}, QL1={3}, ZL1={4}", M, QP1, QS1, QL1, ZL1);
                PH1 = ZU1 - ZL1;
                ZU[M] = ZU1;
                ZL[M] = ZL1;
                PH[M] = PH1;
                if (LA == true)
                {
                    PHL = DK * QP1 * QP1;
                }
                else
                {
                    PHL = DK;
                }
                PN1 = QP1 * (PH1 - PHL) * 9.81 * EFF;
                PN[M] = PN1;
                VM = VM1;
                M++;
            }
            double S = 0;
            double S1;
            for (int j = 0; j < MF; j++)
            {
                S1 = QS[j] * 2.63;
                S = S + S1;
            }
            double EP = 0;
            double EP1;
            for (int j = 0; j < MF; j++)
            {
                EP1 = PN[j] * 720;
                EP = EP + EP1;
            }
            double EEP = EP * 12 / MF;

            List<string> columnsNamesResult = new List<string>()
            { "#", "Месяц", "Приток, м³/с", "Расход ГЭС, м³/с", "Сбросы, м³/с", "Отм. ВБ, м",
                "Отм. НБ, м", "Напор, м", "Мощность, кВт", "Избыт. объем, млн.м³"};

            TableClear(tableResults);

            foreach (string colName in columnsNamesResult)
            {
                tableResults.Columns.Add(new DataColumn(colName, typeof(double)));
            }
            for (int i = 0; i < MF; i++)
            {
                DataRow dr = tableResults.NewRow();
                dr[0] = i + 1;
                dr[1] = MDK[i] + 1;
                if (!double.IsNaN(Q[i])) { dr[2] = Math.Round(Q[i], 1); } else { ErrorMsg(); return; }
                if (!double.IsNaN(QP[i])) { dr[3] = Math.Round(QP[i], 1); } else { ErrorMsg(); return; }
                if (!double.IsNaN(QS[i])) { dr[4] = Math.Round(QS[i], 1); } else { ErrorMsg(); return; }
                if (!double.IsNaN(ZU[i])) { dr[5] = Math.Round(ZU[i], 1); } else { ErrorMsg(); return; }
                if (!double.IsNaN(ZL[i])) { dr[6] = Math.Round(ZL[i], 1); } else { ErrorMsg(); return; }
                if (!double.IsNaN(PH[i])) { dr[7] = Math.Round(PH[i], 2); } else { ErrorMsg(); return; }
                if (!double.IsNaN(PN[i])) { dr[8] = Math.Round(PN[i], 0); } else { ErrorMsg(); return; }
                if (!double.IsNaN(DVM[i])) { dr[9] = Math.Round(DVM[i], 1); } else { ErrorMsg(); return; }

                tableResults.Rows.Add(dr);
            }
            //dataGridView6.DataSource = tableResults;

            List<string> ColSecurity = new List<string>() { "Обеспеченность, %", "Расход бытовой, м³/с",
            "Расход ГЭС, м³/с", "Напор, м", "Мощность ГЭС, среднесуточная, кВт"};

            TableClear(tableSecurity);

            foreach (string str in ColSecurity)
            {
                tableSecurity.Columns.Add(new DataColumn(str, typeof(double)));
            }

            B_Q = Rank(MF, Q);
            B_QP = Rank(MF, QP);
            B_PH = Rank(MF, PH);
            B_PN = Rank(MF, PN);

            for (int i = 0; i < MF; i++)
            {
                DataRow dr = tableSecurity.NewRow();
                dr[0] = Math.Round(100 - ((double)i + 1) / ((double)MF + 1) * 100, 2);
                dr[1] = Math.Round(B_Q[i], 1);
                dr[2] = Math.Round(B_QP[i], 1);
                dr[3] = Math.Round(B_PH[i], 2);
                dr[4] = Math.Round(B_PN[i], 0);
                tableSecurity.Rows.Add(dr);
                //Debug.WriteLine("{0}, {1}, {2}, {3}, {4}", dr[0], dr[1], dr[2], dr[3], dr[4]);
            }

            List<string> columnsExtRemainder = new List<string>()
            { "#", "Месяц", "Дисп. - задан., млн.м³", "Дисп. - расч., млн.м³"};

            TableClear(tableExtRemainder);

            foreach (string colName in columnsExtRemainder)
            {
                tableExtRemainder.Columns.Add(new DataColumn(colName, typeof(double)));
            }
            M = M1;
            if (M > 11) M = 0;
            for (int i = 0; i < MF; i++)
            {
                DataRow dr = tableExtRemainder.NewRow();
                dr[0] = i + 1;
                dr[1] = M + 1;
                dr[2] = VD[M];
                dr[3] = Math.Round(DVM[i] + VD[M], 1);
                M++;
                if (M > 11) M = 0;

                tableExtRemainder.Rows.Add(dr);
            }

            Form2 form2 = new Form2(tableResults, tableSecurity, tableExtRemainder, EEP, S, VU, QR);
            form2.Show();
        }

        private double Lag11(double D, int N, double[] X, double[] Y)
        {
            double V = 0;
            int i1;
            double DX;
            double DYDX;
            for (int i = 1; i < N; i++)
            {
                DX = X[N - 1] - X[N - 2];
                DYDX = (Y[N - 1] - Y[N - 2]) / DX;
                V = (Y[N - 1] * (D - X[N - 2]) - Y[N - 2] * (D - X[N - 1])) / DX;
                //Debug.WriteLine("i={0}, i1={1}, X[i]={2}, D={3}, X[i1]={4}, Y[i]={5}," +
                //    " Y[i1]={6}, DX={7}, DYDX={8}, V={9}", N - 1, N - 2, X[N - 1], D, X[N - 2],
                //    Y[N - 1], Y[N - 2], DX, DYDX, V);
                //Debug.WriteLine("i={0}, D={1}, X[i]={2}, D-X[i]={3}", i, D, X[i], D - X[i]);
                if (D - X[i] <= 0)
                {
                    i1 = i - 1;
                    DX = X[i] - X[i1];
                    DYDX = (Y[i] - Y[i1]) / DX;
                    V = (Y[i] * (D - X[i1]) - Y[i1] * (D - X[i])) / DX;
                    //Debug.WriteLine("i={0}, i1={1}, X[i]={2}, D={3}, X[i1]={4}, Y[i]={5}," +
                    //    " Y[i1]={6}, DX={7}, DYDX={8}, V={9}", i, i1, X[i], D, X[i1],
                    //    Y[i], Y[i1], DX, DYDX, V);
                    break;
                }
            }
            return V;
        }

        private double[] Rank(int _MF, double[] _A)
        {
            int K = 0;
            int N = _MF;
            int L = 0;
            double[] A = new double[600];
            //double[] B = new double[400];
            double[] AR = new double[600];

            for (int i = 0; i < N; i++)
            {
                A[i] = _A[i];
            }

            while (N > 0)
            {
                double Amin = 1000000000;
                for (int i = 0; i < N; i++)
                {
                    if (A[i] < Amin)
                    {
                        Amin = A[i];
                        K = i;
                    }
                }
                //Debug.WriteLine("K={0}, Amin={1}", K, Amin);
                if (K != N)
                {
                    int N1 = N - 1;
                    for (int i = K; i <= N1; i++)
                    {
                        A[i] = A[i + 1];
                    }
                }
                AR[L] = Amin;
                //Debug.WriteLine("N={0}, L={1}, AR[L]={2}", N, L, AR[L]);
                L++;
                N--;
            }
            //ARmin = AR[1];

            //for (int i = 0; i < 20; i++)
            //{
            //    L = i * (_MF - 1) / 19;
            //    B[i] = AR[L];
            //    //Debug.WriteLine("L={0}, i={1}, MF={2}", L, i, _MF);
            //}
            return AR;
        }

        private void ErrorMsg()
        {
            MessageBox.Show($"Расчет не выполнен.\nПроверьте корректность исходных данных.", "Внимание!",
                MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
        }

        private void TableErr(string str)
        {
            MessageBox.Show($"{str} необходимо задавать по возрастанию.", "Внимание!",
                MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
        }

        private bool CheckArrayOrder(double[] A, int N)
        {
            for (int i = 0; i < N - 1; i++)
            {
                //Debug.WriteLine("A.L={0}, i={1}, A={2}, A+1={3}", N, i, A[i], A[i + 1]);
                if (A[i] >= A[i + 1])
                {
                    return false;
                }
            }
            return true;
        }

        private double GetDouble(string str, double defaultValue)
        {
            double result;
            //Try parsing in the current culture
            if (!double.TryParse(str, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                //Then try in US english
                !double.TryParse(str, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                //Then in neutral language
                !double.TryParse(str, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = defaultValue;
                throw new ArgumentException("Необходимо ввести число.");
            }

            return result;
        }

        private void NotInteger(TextBox textBox)
        {
            textBox.BackColor = Color.Red;
            MessageBox.Show("Необходимо ввести целое число.", "Внимание!",
                MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
        }
        private void NotDouble(Exception ex, string tab)
        {
            //MessageBox.Show($"{tab} {ex.Message}", "Внимание!",
            MessageBox.Show($"{tab}", "Внимание!",
                MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
        }

        private void ZeroMsg(TextBox textBox, string tab)
        {
            textBox.BackColor = Color.Red;
            MessageBox.Show($"Вкладка {tab}. Значение не может быть равно нулю.", "Внимание!",
                MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
        }

        private void LimitMsg(string str)
        {
            MessageBox.Show($"Значение не должно превышать {str}.", "Внимание!",
                MessageBoxButtons.OK, icon: MessageBoxIcon.Information);
        }

        private void textBox8_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, e);
                e.Handled = true;
            }
        }
        private void textBox9_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button2_Click(sender, e);
                e.Handled = true;
            }
        }
        private void textBox10_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button3_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
