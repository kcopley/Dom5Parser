using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dom5Editor.UI;
using Dom5Editor.UI.Views;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// Editor control for CUSTOMMAGIC entries (random magic path configurations).
    /// Displays a list of CustomMagicItem entries with path toggles and chance input.
    /// </summary>
    public partial class CustomMagicEditor : UserControl, INotifyPropertyChanged
    {
        public CustomMagicEditor()
        {
            InitializeComponent();

            AddCommand = new RelayCommand(OnAdd);
            RemoveCommand = new RelayCommand<CustomMagicItem>(OnRemove);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // ========================================
        // Dependency Properties
        // ========================================

        public static readonly DependencyProperty CustomMagicItemsProperty =
            DependencyProperty.Register(
                nameof(CustomMagicItems),
                typeof(ObservableCollection<CustomMagicItem>),
                typeof(CustomMagicEditor),
                new PropertyMetadata(null, OnCustomMagicItemsChanged));

        public ObservableCollection<CustomMagicItem> CustomMagicItems
        {
            get => (ObservableCollection<CustomMagicItem>)GetValue(CustomMagicItemsProperty);
            set => SetValue(CustomMagicItemsProperty, value);
        }

        private static void OnCustomMagicItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomMagicEditor editor)
            {
                editor.OnPropertyChanged(nameof(HasNoItems));
            }
        }

        // ========================================
        // Computed Properties
        // ========================================

        public bool HasNoItems => CustomMagicItems == null || CustomMagicItems.Count == 0;

        // ========================================
        // Commands
        // ========================================

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }

        // ========================================
        // Events
        // ========================================

        /// <summary>
        /// Raised when a new custom magic entry should be added.
        /// </summary>
        public event EventHandler AddRequested;

        /// <summary>
        /// Raised when a custom magic entry should be removed.
        /// </summary>
        public event EventHandler<CustomMagicItem> RemoveRequested;

        /// <summary>
        /// Raised when a custom magic entry has been modified.
        /// </summary>
        public event EventHandler<CustomMagicItem> ItemChanged;

        // ========================================
        // Command Handlers
        // ========================================

        private void OnAddButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            OnAdd();
        }

        private void OnAdd()
        {
            AddRequested?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(HasNoItems));
        }

        private void OnRemove(CustomMagicItem item)
        {
            if (item != null && !item.IsInherited)
            {
                RemoveRequested?.Invoke(this, item);
                OnPropertyChanged(nameof(HasNoItems));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
