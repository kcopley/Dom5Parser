using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// Editor control for integer properties.
    /// </summary>
    public partial class IntPropertyEditor : UserControl
    {
        private static readonly Regex _numericRegex = new Regex(@"^-?\d*$");

        public IntPropertyEditor()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(IntPropertyEditor), new PropertyMetadata("Property"));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int?), typeof(IntPropertyEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int? Value
        {
            get => (int?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty IsInheritedProperty =
            DependencyProperty.Register(nameof(IsInherited), typeof(bool), typeof(IntPropertyEditor), new PropertyMetadata(false));

        public bool IsInherited
        {
            get => (bool)GetValue(IsInheritedProperty);
            set => SetValue(IsInheritedProperty, value);
        }

        public static readonly DependencyProperty IsModifiedProperty =
            DependencyProperty.Register(nameof(IsModified), typeof(bool), typeof(IntPropertyEditor), new PropertyMetadata(false));

        public bool IsModified
        {
            get => (bool)GetValue(IsModifiedProperty);
            set => SetValue(IsModifiedProperty, value);
        }

        public static readonly DependencyProperty IsSessionEditProperty =
            DependencyProperty.Register(nameof(IsSessionEdit), typeof(bool), typeof(IntPropertyEditor), new PropertyMetadata(false));

        public bool IsSessionEdit
        {
            get => (bool)GetValue(IsSessionEditProperty);
            set => SetValue(IsSessionEditProperty, value);
        }

        /// <summary>
        /// Command to execute when reset button is clicked. Reset button only shows when IsSessionEdit is true.
        /// </summary>
        public static readonly DependencyProperty ResetCommandProperty =
            DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(IntPropertyEditor), new PropertyMetadata(null));

        public ICommand ResetCommand
        {
            get => (ICommand)GetValue(ResetCommandProperty);
            set => SetValue(ResetCommandProperty, value);
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !_numericRegex.IsMatch(newText);
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            ResetCommand?.Execute(null);
        }
    }
}
