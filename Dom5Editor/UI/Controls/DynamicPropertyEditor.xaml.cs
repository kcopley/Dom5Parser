using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// A dynamic property editor that can switch between different editor types based on a mode.
    /// Supports: int (integer input), ref (entity reference selector), readonly (display only), string (text input).
    /// </summary>
    public partial class DynamicPropertyEditor : UserControl, INotifyPropertyChanged
    {
        private static readonly Regex _numericRegex = new Regex(@"^-?\d*$");
        private readonly DispatcherTimer _debounceTimer;
        private readonly DispatcherTimer _stringDebounceTimer;
        private const int DebounceDelayMs = 400;

        public DynamicPropertyEditor()
        {
            InitializeComponent();

            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DebounceDelayMs)
            };
            _debounceTimer.Tick += OnDebounceTimerTick;

            _stringDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(DebounceDelayMs)
            };
            _stringDebounceTimer.Tick += OnStringDebounceTimerTick;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Editor Mode

        /// <summary>
        /// The editor mode determines which control is displayed.
        /// Valid values: "int", "ref", "readonly", "string"
        /// </summary>
        public static readonly DependencyProperty EditorModeProperty =
            DependencyProperty.Register(nameof(EditorMode), typeof(string), typeof(DynamicPropertyEditor),
                new PropertyMetadata("int", OnEditorModeChanged));

        public string EditorMode
        {
            get => (string)GetValue(EditorModeProperty);
            set => SetValue(EditorModeProperty, value);
        }

        private static void OnEditorModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DynamicPropertyEditor)d;
            control.OnPropertyChanged(nameof(IsIntMode));
            control.OnPropertyChanged(nameof(IsRefMode));
            control.OnPropertyChanged(nameof(IsReadOnlyMode));
            control.OnPropertyChanged(nameof(IsStringMode));
        }

        /// <summary>
        /// True when EditorMode is "int" (integer input mode).
        /// </summary>
        public bool IsIntMode => string.Equals(EditorMode, "int", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// True when EditorMode is "ref" (reference selector mode).
        /// </summary>
        public bool IsRefMode => string.Equals(EditorMode, "ref", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// True when EditorMode is "readonly" (display only mode).
        /// </summary>
        public bool IsReadOnlyMode => string.Equals(EditorMode, "readonly", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// True when EditorMode is "string" (string input mode).
        /// </summary>
        public bool IsStringMode => string.Equals(EditorMode, "string", StringComparison.OrdinalIgnoreCase);

        #endregion

        #region Common Properties

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(DynamicPropertyEditor), new PropertyMetadata("Property"));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty IsInheritedProperty =
            DependencyProperty.Register(nameof(IsInherited), typeof(bool), typeof(DynamicPropertyEditor), new PropertyMetadata(false));

        public bool IsInherited
        {
            get => (bool)GetValue(IsInheritedProperty);
            set => SetValue(IsInheritedProperty, value);
        }

        public static readonly DependencyProperty IsModifiedProperty =
            DependencyProperty.Register(nameof(IsModified), typeof(bool), typeof(DynamicPropertyEditor), new PropertyMetadata(false));

        public bool IsModified
        {
            get => (bool)GetValue(IsModifiedProperty);
            set => SetValue(IsModifiedProperty, value);
        }

        public static readonly DependencyProperty IsSessionEditProperty =
            DependencyProperty.Register(nameof(IsSessionEdit), typeof(bool), typeof(DynamicPropertyEditor), new PropertyMetadata(false));

        public bool IsSessionEdit
        {
            get => (bool)GetValue(IsSessionEditProperty);
            set => SetValue(IsSessionEditProperty, value);
        }

        public static readonly DependencyProperty ResetCommandProperty =
            DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(DynamicPropertyEditor), new PropertyMetadata(null));

        public ICommand ResetCommand
        {
            get => (ICommand)GetValue(ResetCommandProperty);
            set => SetValue(ResetCommandProperty, value);
        }

        #endregion

        #region Int Mode Properties

        public static readonly DependencyProperty IntValueProperty =
            DependencyProperty.Register(nameof(IntValue), typeof(int?), typeof(DynamicPropertyEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIntValueChanged));

        public int? IntValue
        {
            get => (int?)GetValue(IntValueProperty);
            set => SetValue(IntValueProperty, value);
        }

        private static void OnIntValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Update the textbox if value changed externally
            var control = (DynamicPropertyEditor)d;
            if (control.IntValueTextBox != null && !control.IntValueTextBox.IsFocused)
            {
                var newValue = e.NewValue as int?;
                control.IntValueTextBox.Text = newValue?.ToString() ?? "";
            }
        }

        #endregion

        #region Ref Mode Properties

        public static readonly DependencyProperty RefSelectedIdProperty =
            DependencyProperty.Register(nameof(RefSelectedId), typeof(int?), typeof(DynamicPropertyEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int? RefSelectedId
        {
            get => (int?)GetValue(RefSelectedIdProperty);
            set => SetValue(RefSelectedIdProperty, value);
        }

        public static readonly DependencyProperty RefItemsSourceProperty =
            DependencyProperty.Register(nameof(RefItemsSource), typeof(IEnumerable<ReferenceItem>), typeof(DynamicPropertyEditor),
                new PropertyMetadata(null));

        public IEnumerable<ReferenceItem> RefItemsSource
        {
            get => (IEnumerable<ReferenceItem>)GetValue(RefItemsSourceProperty);
            set => SetValue(RefItemsSourceProperty, value);
        }

        /// <summary>
        /// Event raised when the reference selection changes (in ref mode).
        /// </summary>
        public event EventHandler<ReferenceSelectionChangedEventArgs> RefSelectionChanged;

        #endregion

        #region Readonly Mode Properties

        public static readonly DependencyProperty ReadOnlyTextProperty =
            DependencyProperty.Register(nameof(ReadOnlyText), typeof(string), typeof(DynamicPropertyEditor),
                new PropertyMetadata(""));

        public string ReadOnlyText
        {
            get => (string)GetValue(ReadOnlyTextProperty);
            set => SetValue(ReadOnlyTextProperty, value);
        }

        #endregion

        #region String Mode Properties

        public static readonly DependencyProperty StringValueProperty =
            DependencyProperty.Register(nameof(StringValue), typeof(string), typeof(DynamicPropertyEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnStringValueChanged));

        public string StringValue
        {
            get => (string)GetValue(StringValueProperty);
            set => SetValue(StringValueProperty, value);
        }

        private static void OnStringValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DynamicPropertyEditor)d;
            if (control.StringValueTextBox != null && !control.StringValueTextBox.IsFocused)
            {
                control.StringValueTextBox.Text = e.NewValue as string ?? "";
            }
        }

        #endregion

        #region Secondary Display Properties

        public static readonly DependencyProperty SecondaryDisplayTextProperty =
            DependencyProperty.Register(nameof(SecondaryDisplayText), typeof(string), typeof(DynamicPropertyEditor),
                new PropertyMetadata(null, OnSecondaryDisplayChanged));

        public string SecondaryDisplayText
        {
            get => (string)GetValue(SecondaryDisplayTextProperty);
            set => SetValue(SecondaryDisplayTextProperty, value);
        }

        public static readonly DependencyProperty SecondaryDisplayColorProperty =
            DependencyProperty.Register(nameof(SecondaryDisplayColor), typeof(string), typeof(DynamicPropertyEditor),
                new PropertyMetadata("#00BFFF", OnSecondaryDisplayChanged));

        public string SecondaryDisplayColor
        {
            get => (string)GetValue(SecondaryDisplayColorProperty);
            set => SetValue(SecondaryDisplayColorProperty, value);
        }

        private static void OnSecondaryDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DynamicPropertyEditor)d;
            control.OnPropertyChanged(nameof(HasSecondaryDisplay));
            control.OnPropertyChanged(nameof(SecondaryDisplayForeground));
        }

        public bool HasSecondaryDisplay => !string.IsNullOrEmpty(SecondaryDisplayText);

        public Brush SecondaryDisplayForeground
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(SecondaryDisplayColor ?? "#00BFFF"));
                }
                catch
                {
                    return new SolidColorBrush(Color.FromRgb(0, 191, 255)); // Default cyan
                }
            }
        }

        #endregion

        #region Int Mode Event Handlers

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !_numericRegex.IsMatch(newText);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            CommitIntValue();
        }

        private void OnDebounceTimerTick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();
            CommitIntValue();
        }

        private void CommitIntValue()
        {
            _debounceTimer.Stop();

            var binding = IntValueTextBox.GetBindingExpression(TextBox.TextProperty);
            if (binding == null)
                return;

            var currentSourceValue = IntValue?.ToString() ?? "";
            var currentTextValue = IntValueTextBox.Text ?? "";

            if (currentTextValue != currentSourceValue)
            {
                binding.UpdateSource();
            }
        }

        #endregion

        #region String Mode Event Handlers

        private void OnStringTextChanged(object sender, TextChangedEventArgs e)
        {
            _stringDebounceTimer.Stop();
            _stringDebounceTimer.Start();
        }

        private void OnStringLostFocus(object sender, RoutedEventArgs e)
        {
            CommitStringValue();
        }

        private void OnStringDebounceTimerTick(object sender, EventArgs e)
        {
            _stringDebounceTimer.Stop();
            CommitStringValue();
        }

        private void CommitStringValue()
        {
            _stringDebounceTimer.Stop();

            var binding = StringValueTextBox.GetBindingExpression(TextBox.TextProperty);
            if (binding == null)
                return;

            var currentSourceValue = StringValue ?? "";
            var currentTextValue = StringValueTextBox.Text ?? "";

            if (currentTextValue != currentSourceValue)
            {
                binding.UpdateSource();
            }
        }

        #endregion

        #region Ref Mode Event Handlers

        private void OnRefSelectionChanged(object sender, ReferenceSelectionChangedEventArgs e)
        {
            RefSelectionChanged?.Invoke(this, e);
        }

        #endregion

        #region Common Event Handlers

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            ResetCommand?.Execute(null);
        }

        #endregion

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
