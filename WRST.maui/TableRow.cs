using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WRST.maui
{
    public class TableRow : INotifyPropertyChanged
    {
        private int _index;
        private List<string> _cells;
        private string _rowBackgroundColor;
        private bool _isEditable;
        private bool _isVisible;

        public string RowLabel { get; set; } = "";

        public int Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<string> Cells
        {
            get => _cells;
            set
            {
                if (_cells != value)
                {
                    _cells = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RowBackgroundColor
        {
            get => _rowBackgroundColor;
            set
            {
                if (_rowBackgroundColor != value)
                {
                    _rowBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                if (_isEditable != value)
                {
                    _isEditable = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public TableRow()
        {
            Cells = new List<string>();
            RowBackgroundColor = "White";
            IsEditable = true;
            IsVisible = true;
            RowLabel = "";
        }

        public void InitializeCells(int count, string defaultValue)
        {
            Cells.Clear();
            for (int i = 0; i < count; i++)
            {
                Cells.Add(defaultValue);
            }
        }

        public void SetCell(int index, string value)
        {
            if (index >= 0 && index < Cells.Count)
            {
                Cells[index] = value;
                //OnPropertyChanged(nameof(Cells));
                var temp = Cells;
                Cells = null;
                Cells = temp;
            }
        }

        public string GetCell(int index)
        {
            if (index >= 0 && index < Cells.Count)
            {
                return Cells[index];
            }
            return string.Empty;
        }

        // Генерация заголовков для горизонтальной таблицы
        public void GenerateHeaderCells(int count, int startValue = 1)
        {
            InitializeCells(count, "");
            Cells[0] = "#"; // Первый столбец всегда "#"
            for (int i = 1; i < count; i++)
            {
                Cells[i] = (startValue + i - 1).ToString();
            }
        }

        // Генерация строк данных для ввода
        public void GenerateInputCells(int count, string defaultValue = "0")
        {
            InitializeCells(count, defaultValue);
            Cells[0] = "Приток"; // Название строки
        }

        // Генерация вертикальной таблицы
        public void GenerateVerticalHeader(List<string> headers)
        {
            InitializeCells(headers.Count, "");
            for (int i = 0; i < headers.Count; i++)
            {
                Cells[i] = headers[i];
            }
        }

        // Генерация вертикальных значений
        public void GenerateVerticalValues(List<string> values)
        {
            if (values.Count > Cells.Count)
            {
                while (Cells.Count < values.Count)
                {
                    Cells.Add("");
                }
            }

            for (int i = 0; i < values.Count; i++)
            {
                Cells[i] = values[i];
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
