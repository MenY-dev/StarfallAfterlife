using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.LogicalTree;
using Avalonia.Data;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;

namespace StarfallAfterlife.Launcher.Controls
{
    public partial class Property : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<Property, string>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<GridLength> LabelWidthProperty =
            AvaloniaProperty.Register<Property, GridLength>(nameof(LabelWidth), GridLength.Star);

        public static readonly StyledProperty<GridLength> ContentWidthProperty =
            AvaloniaProperty.Register<Property, GridLength>(nameof(ContentWidth), GridLength.Star);

        public string Text
        {
            get { return GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public GridLength LabelWidth
        {
            get { return GetValue(LabelWidthProperty); }
            set { SetValue(LabelWidthProperty, value); }
        }

        public GridLength ContentWidth
        {
            get { return GetValue(ContentWidthProperty); }
            set { SetValue(ContentWidthProperty, value); }
        }

        protected Grid Wrapper { get; set; }

        protected ColumnDefinition LabelColumn { get; set; }

        protected ColumnDefinition ContentColumn { get; set; }

        public Property()
        {
            InitializeComponent();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            Wrapper = this.GetVisualDescendants().FirstOrDefault(c => c is Grid && c.Name == "Wrapper") as Grid;

            if (Wrapper is not null)
            {
                Wrapper.ColumnDefinitions.Clear();
                Wrapper.ColumnDefinitions.Add(LabelColumn = new ColumnDefinition(LabelWidth));
                Wrapper.ColumnDefinitions.Add(ContentColumn = new ColumnDefinition(ContentWidth));
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == LabelWidthProperty &&
                LabelColumn is not null)
            {
                LabelColumn.Width = ContentWidth;
            }
            else if (change.Property == ContentWidthProperty &&
                ContentColumn is not null)
            {
                ContentColumn.Width = ContentWidth;
            }
        }
    }
}
