using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dom5Editor.UI.Controls
{
    public partial class CommandPropertyEditor : UserControl
    {
        public CommandPropertyEditor() => InitializeComponent();

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(CommandPropertyEditor), new PropertyMetadata("Command"));
        public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(CommandPropertyEditor),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public bool IsChecked { get => (bool)GetValue(IsCheckedProperty); set => SetValue(IsCheckedProperty, value); }

        public static readonly DependencyProperty IsInheritedProperty =
            DependencyProperty.Register(nameof(IsInherited), typeof(bool), typeof(CommandPropertyEditor), new PropertyMetadata(false));
        public bool IsInherited { get => (bool)GetValue(IsInheritedProperty); set => SetValue(IsInheritedProperty, value); }

        public static readonly DependencyProperty IsModifiedProperty =
            DependencyProperty.Register(nameof(IsModified), typeof(bool), typeof(CommandPropertyEditor), new PropertyMetadata(false));
        public bool IsModified { get => (bool)GetValue(IsModifiedProperty); set => SetValue(IsModifiedProperty, value); }

        public static readonly DependencyProperty IsSessionEditProperty =
            DependencyProperty.Register(nameof(IsSessionEdit), typeof(bool), typeof(CommandPropertyEditor), new PropertyMetadata(false));
        public bool IsSessionEdit { get => (bool)GetValue(IsSessionEditProperty); set => SetValue(IsSessionEditProperty, value); }

        public static readonly DependencyProperty ResetCommandProperty =
            DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(CommandPropertyEditor), new PropertyMetadata(null));
        public ICommand ResetCommand { get => (ICommand)GetValue(ResetCommandProperty); set => SetValue(ResetCommandProperty, value); }

        private void OnResetClick(object sender, RoutedEventArgs e) => ResetCommand?.Execute(null);
    }
}
