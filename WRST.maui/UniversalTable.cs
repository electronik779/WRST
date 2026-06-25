using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace WRST.maui
{
    public class UniversalTable : INotifyPropertyChanged
    {
        private List<TableRow> _rows;
        private TableType _tableType;
        private string _headerText;

        public List<TableRow> Rows
        {
            get => _rows;
            set
            {
                if (_rows != value)
                {
                    _rows = value;
                    OnPropertyChanged();
                }
            }
        }

        public TableType tableType
        {
            get => _tableType;
            set
            {
                if (_tableType != value)
                {
                    _tableType = value;
                    OnPropertyChanged();
                }
            }
        }

        public string HeaderText
        {
            get => _headerText;
            set
            {
                if (_headerText != value)
                {
                    _headerText = value;
                    OnPropertyChanged();
                }
            }
        }

        public UniversalTable()
        {
            Rows = new List<TableRow>();
            tableType = TableType.HorizontalTwoRows;
            HeaderText = "";
        }

        public enum TableType
        {
            HorizontalTwoRows,      // Горизонтальная с 2 строками
            VerticalOutput,         // Вертикальная для вывода результатов
            HorizontalMultiRows     // Горизонтальная с множеством строк
        }

        // Создание горизонтальной таблицы 2 строки
        public void CreateHorizontalTwoRows(int columnCount, int startingMonth = 1, string headerText = "")
        {
            Rows.Clear();
            HeaderText = headerText;

            // Первая строка - заголовки
            var headerRow = new TableRow();
            headerRow.Index = 0;
            headerRow.IsEditable = false;
            headerRow.RowBackgroundColor = "LightGray";
            headerRow.GenerateHeaderCells(columnCount, startingMonth);
            Rows.Add(headerRow);

            // Вторая строка - ввод данных
            var inputRow = new TableRow();
            inputRow.Index = 1;
            inputRow.IsEditable = true;
            inputRow.GenerateInputCells(columnCount, "0");
            Rows.Add(inputRow);
        }

        // Создание вертикальной таблицы для вывода
        public void CreateVerticalOutput(List<string> headers, List<string> values, string headerText = "")
        {
            Rows.Clear();
            tableType = TableType.VerticalOutput;
            HeaderText = headerText;

            var headerRow = new TableRow();
            headerRow.Index = 0;
            headerRow.IsEditable = false;
            headerRow.RowBackgroundColor = "LightGray";
            headerRow.GenerateVerticalHeader(headers);
            Rows.Add(headerRow);

            var valueRow = new TableRow();
            valueRow.Index = 1;
            valueRow.IsEditable = false;
            valueRow.GenerateVerticalValues(values);
            Rows.Add(valueRow);
        }

        // Создание горизонтальной таблицы с множеством строк
        public void CreateHorizontalMultiRows(int rowCount, int columnCount, string headerText = "")
        {
            Rows.Clear();
            tableType = TableType.HorizontalMultiRows;
            HeaderText = headerText;

            for (int i = 0; i < rowCount; i++)
            {
                var row = new TableRow();
                row.Index = i;
                row.IsEditable = i > 0; // Только вторая и последующие строки редактируемы
                row.InitializeCells(columnCount, "0");

                if (i == 0)
                {
                    row.IsEditable = false;
                    row.RowBackgroundColor = "LightGray";
                    row.GenerateHeaderCells(columnCount);
                }

                Rows.Add(row);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
