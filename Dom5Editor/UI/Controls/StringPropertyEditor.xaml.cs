using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dom5Editor.UI.Controls
{
    public partial class StringPropertyEditor : UserControl
    {
        public StringPropertyEditor() => InitializeComponent();

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(StringPropertyEditor), new PropertyMetadata("Property"));
        public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(StringPropertyEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

        public static readonly DependencyProperty IsMultilineProperty =
            DependencyProperty.Register(nameof(IsMultiline), typeof(bool), typeof(StringPropertyEditor), new PropertyMetadata(false));
        public bool IsMultiline { get => (bool)GetValue(IsMultilineProperty); set => SetValue(IsMultilineProperty, value); }

        public static readonly DependencyProperty IsInheritedProperty =
            DependencyProperty.Register(nameof(IsInherited), typeof(bool), typeof(StringPropertyEditor), new PropertyMetadata(false));
        public bool IsInherited { get => (bool)GetValue(IsInheritedProperty); set => SetValue(IsInheritedProperty, value); }

        public static readonly DependencyProperty IsModifiedProperty =
            DependencyProperty.Register(nameof(IsModified), typeof(bool), typeof(StringPropertyEditor), new PropertyMetadata(false));
        public bool IsModified { get => (bool)GetValue(IsModifiedProperty); set => SetValue(IsModifiedProperty, value); }

        public static readonly DependencyProperty IsSessionEditProperty =
            DependencyProperty.Register(nameof(IsSessionEdit), typeof(bool), typeof(StringPropertyEditor), new PropertyMetadata(false));
        public bool IsSessionEdit { get => (bool)GetValue(IsSessionEditProperty); set => SetValue(IsSessionEditProperty, value); }

        public static readonly DependencyProperty ResetCommandProperty =
            DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(StringPropertyEditor), new PropertyMetadata(null));
        public ICommand ResetCommand { get => (ICommand)GetValue(ResetCommandProperty); set => SetValue(ResetCommandProperty, value); }

        private void OnResetClick(object sender, RoutedEventArgs e) => ResetCommand?.Execute(null);
    }
}
