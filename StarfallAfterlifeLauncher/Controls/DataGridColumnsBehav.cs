using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace StarfallAfterlife.Launcher.Controls
{
    public class DataGridColumnsBehav : AvaloniaObject
    {
        public static readonly AttachedProperty<ObservableCollection<DataGridColumn>> ColumnsProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumnsBehav, DataGrid, ObservableCollection<DataGridColumn>>(
            "Columns", default, false);


        static DataGridColumnsBehav()
        {
            ColumnsProperty.Changed.AddClassHandler<DataGrid>(HandleColumnsPropertyChanged);
        }

        private static void HandleColumnsPropertyChanged(DataGrid grid, AvaloniaPropertyChangedEventArgs args)
        {
            var columns = args.NewValue as ObservableCollection<DataGridColumn>;

            if (grid is null ||
                columns is null)
                return;

            grid.SetValue(ColumnsProperty, columns);
            grid.Columns.Clear();

            if (columns is null)
                return;

            foreach (var item in columns)
                grid.Columns.Add(item);

            columns.CollectionChanged += (o, e) =>
            {
                if (e.Action is NotifyCollectionChangedAction.Add)
                {
                    foreach (DataGridColumn item in e.NewItems)
                        grid.Columns.Add(item);
                }
                else if (e.Action is NotifyCollectionChangedAction.Move)
                {
                    grid.Columns.Move(e.OldStartingIndex, e.NewStartingIndex);
                }
                else if (e.Action is NotifyCollectionChangedAction.Remove)
                {
                    foreach (DataGridColumn item in e.OldItems)
                        grid.Columns.Remove(item);
                }
                else if (e.Action is NotifyCollectionChangedAction.Replace)
                {
                    grid.Columns[e.NewStartingIndex] = e.NewItems[0] as DataGridColumn;
                }
                else if (e.Action is NotifyCollectionChangedAction.Reset)
                {
                    grid.Columns.Clear();

                    foreach (DataGridColumn item in e.NewItems)
                        grid.Columns.Add(item);
                }
            };
        }

        public static void SetColumns(AvaloniaObject element, ObservableCollection<DataGridColumn> columns)
        {
            element.SetValue(ColumnsProperty, columns);
        }

        public static ObservableCollection<DataGridColumn> GetColumns(AvaloniaObject element) =>
            element.GetValue(ColumnsProperty);
    }
}
