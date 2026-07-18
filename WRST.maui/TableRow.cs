using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WRST.maui
{
    public class TableRow : INotifyPropertyChanged
    {
        private int _index;
        private ObservableCollection<string> _cells;
        private string _rowBackgroundColor;
        private bool _isEditable;
        private bool _isVisible;

        public string RowLabel { get; set; } = "";
        public double GuaranteedLimit { get; set; }

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

        public ObservableCollection<string> Cells
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
            Cells = new ObservableCollection<string>();
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaiseCellsChanged()
        {
            OnPropertyChanged(nameof(Cells));
        }
    }

}
