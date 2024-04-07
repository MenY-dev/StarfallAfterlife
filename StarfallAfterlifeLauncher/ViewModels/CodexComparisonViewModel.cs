using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using StarfallAfterlife.Bridge.Codex;
using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Launcher.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class CodexComparisonViewModel : ViewModelBase
    {
        public enum SortingDirection : int
        {
            None = 0,
            MinToMax = 1,
            MaxToMin = -1,
        }

        public class DataRow : ViewModelBase
        {
            public DataRow(string key, string name, List<CodexItemPropertyViewModel> props)
            {
                Key = key;
                Name = name;
                Props = props;
            }

            public string Key { get; set; }

            public string Name { get; set; }

            public List<CodexItemPropertyViewModel> Props { get; set; }

            public SortingDirection SortingDirection
            {
                get => _sortingDirection;
                set
                {
                    SetAndRaise(ref _sortingDirection, value);
                    RaisePropertyUpdate(IsMinToMaxSorting, nameof(IsMinToMaxSorting));
                    RaisePropertyUpdate(IsMaxToMinSorting, nameof(IsMaxToMinSorting));
                }
            }

            public bool IsMinToMaxSorting => SortingDirection == SortingDirection.MinToMax;

            public bool IsMaxToMinSorting => SortingDirection == SortingDirection.MaxToMin;


            private SortingDirection _sortingDirection;
        }

        public ObservableCollection<DataGridColumn> Columns { get; } = new();

        public ObservableCollection<DataRow> Rows { get; } = new();

        protected List<string> SortingOrder { get; } = new();

        public bool IsShiftPressed { get; set; }

        public CodexComparisonViewModel() { }

        public CodexComparisonViewModel(IEnumerable<CodexEntryViewModel> entries, CodexViewModel codexVM)
        {
            var codex = codexVM?.Codex;

            if (entries is null ||
                codex is null)
                return;

            var toCompare = entries?.ToList()
                .Select(e => codex.GetItem(e?.Id ?? 0))
                .Where(i => i is not null)
                .Select(i =>
                {
                    try { return new CodexItemViewModel(codexVM, i); }
                    catch { return null; }
                })
                .Where(i => i is not null)
                .ToList();

            foreach (var item in toCompare)
            {
                foreach (var prop in item.Properties)
                {
                    var row = Rows.FirstOrDefault(i => i.Key == prop.Key);

                    if (row is null)
                    {
                        row = new DataRow(prop.Key, prop.Name, new());
                        Rows.Add(row);
                    }

                    row.Props.Add(prop);
                }
            }

            foreach (var row in Rows)
            {
                var props = row.Props;

                if (props.Count < 1)
                    continue;

                if (props.MaxBy(i => i?.Value as IComparable) is CodexItemPropertyViewModel maxProp)
                {
                    maxProp.IsMaxValue = true;

                    foreach (var item in props)
                    {
                        if (maxProp.Value?.Equals(item?.Value) == true)
                            item.IsMaxValue = true;
                    }
                }
            }

            Rows.SortBy(i =>
            {
                if (SfCodex.GetPropertyInfo(i.Key)?.Flags is SfCodexPropertyFlags flags)
                {
                    return flags.HasFlag(SfCodexPropertyFlags.MainInfo) ? -2 :
                           flags.HasFlag(SfCodexPropertyFlags.SecondaryInfo) ? -1 :
                           flags.HasFlag(SfCodexPropertyFlags.AdditionalInfo) ? 1 : 0;
                }

                return 0;
            } );

            Columns.Add(new DataGridTemplateColumn()
            {
                MinWidth = 100,
                MaxWidth = 200,
                Header = App.GetString("s_window_codex_compare_name_lbl"),
                Tag = "PropsHeader",
                CellTemplate = new FuncDataTemplate<DataRow>((d, n) =>
                {
                    var btn = new Button() { Name = "PropertyHeader", };
                    var wrapper = new Grid()
                    {
                        Name = "PropertyHeaderWrapper",
                        ColumnDefinitions = new()
                        {
                            new(){ Width = GridLength.Star },
                            new(){ Width = GridLength.Auto },
                        }
                    };

                    wrapper.Children.Add(new TextBlock()
                    {
                        Text = d.Name,
                        Name = "PropertyName",
                        [Grid.ColumnProperty] = 0,
                    });

                    var sortingLabel = new TextBlock()
                    {
                        Name = "SortingLabel",
                        [Grid.ColumnProperty] = 1,
                    };

                    void SetSortingClasses(Control control, DataRow row)
                    {
                        control.Classes.Set("mintomax", row.IsMinToMaxSorting);
                        control.Classes.Set("maxtomin", row.IsMaxToMinSorting);
                    }

                    SetSortingClasses(sortingLabel, d);

                    d.PropertyChanged += (o, e) =>
                    {
                        if (e.PropertyName == nameof(DataRow.SortingDirection))
                            SetSortingClasses(sortingLabel, d);
                    };

                    wrapper.Children.Add(sortingLabel);

                    btn.Content = wrapper;
                    btn.Click += (o, e) =>
                    {
                        if (o is Button { DataContext: DataRow row })
                        {
                            SetSorting(
                                row.Key,
                                row.SortingDirection is SortingDirection.None ? SortingDirection.MinToMax :
                                row.SortingDirection is SortingDirection.MinToMax ? SortingDirection.MaxToMin :
                                SortingDirection.None,
                                IsShiftPressed);
                        }

                        e.Handled = true;
                    };

                    return btn;
                }),
            });

            foreach (var item in toCompare)
            {
                Columns.Add(new DataGridTemplateColumn()
                {
                    Header = item.Name,
                    MinWidth = 100,
                    MaxWidth = 200,
                    Tag = item,
                    CellTemplate = new FuncDataTemplate<DataRow>((d, n) =>
                    {
                        return new CodexItemValuePresenter()
                        {
                            DataContext = d.Props.FirstOrDefault(p => p.Item?.Id == item.Id),
                        };
                    }),
                });
            }

            UpdateColumnsSorting();
        }

        public void SetSorting(string key, SortingDirection direction = SortingDirection.MinToMax, bool addToCurrent = false)
        {
            var row = Rows?.FirstOrDefault(p => p.Key == key);

            if (addToCurrent == false)
            {
                foreach (var item in Rows)
                {
                    if (item == row)
                        continue;

                    item.SortingDirection = SortingDirection.None;
                    SortingOrder.Remove(item.Key);
                }
            }

            if (row is null)
                return;

            SortingOrder.Remove(key);
            row.SortingDirection = direction;

            if (direction != SortingDirection.None)
                SortingOrder.Add(key);

            UpdateColumnsSorting();
        }

        private void UpdateColumnsSorting()
        {
            var comparer = Comparer.Default;
            var toSorting = Rows
                .Where(r => r.SortingDirection is not SortingDirection.None)
                .OrderBy(r => SortingOrder.IndexOf(r.Key))
                .ToArray();

            if (toSorting.Length < 1)
            {
                int index = 0;
                foreach (var item in Columns)
                {
                    item.DisplayIndex = index;
                    index++;
                }

                return;
            }

            var columnsOrder = Columns.ToList();

            columnsOrder.Sort(Comparer<DataGridColumn>.Create((x, y) =>
            {
                var xItem = x.Tag as CodexItemViewModel;
                var yItem = y.Tag as CodexItemViewModel;

                if (xItem is null)
                {
                    if (yItem is null) return 0;
                    else return -1;
                }

                if (yItem is null)
                    return 1;

                foreach (var row in toSorting)
                {
                    var xVar = row.Props.FirstOrDefault(p => p.Item?.Id == xItem.Id)?.Value as IComparable;
                    var yVar = row.Props.FirstOrDefault(p => p.Item?.Id == yItem.Id)?.Value as IComparable;
                    var result = comparer.Compare(xVar, yVar) * (int)row.SortingDirection;

                    if (result != 0)
                        return result;
                }

                return 0;
            }));

            foreach (var item in Columns)
            {
                item.DisplayIndex = Math.Max(0, columnsOrder.IndexOf(item));
            }
        }

        public void ShiftStateChanged(object context)
        {
            if (context is bool state)
                IsShiftPressed = state;
        }
    }
}
