using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dom5Editor.UI.Controls
{
    public partial class EntityListControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ICollectionView _entitiesView;

        public EntityListControl()
        {
            InitializeComponent();
        }

        // ========================================
        // Dependency Properties
        // ========================================

        public static readonly DependencyProperty EntityTypeProperty =
            DependencyProperty.Register(nameof(EntityType), typeof(string), typeof(EntityListControl),
                new PropertyMetadata("Entity"));

        public string EntityType
        {
            get => (string)GetValue(EntityTypeProperty);
            set => SetValue(EntityTypeProperty, value);
        }

        public static readonly DependencyProperty EntitiesProperty =
            DependencyProperty.Register(nameof(Entities), typeof(IEnumerable), typeof(EntityListControl),
                new PropertyMetadata(null, OnEntitiesChanged));

        public IEnumerable Entities
        {
            get => (IEnumerable)GetValue(EntitiesProperty);
            set => SetValue(EntitiesProperty, value);
        }

        public static readonly DependencyProperty SelectedEntityProperty =
            DependencyProperty.Register(nameof(SelectedEntity), typeof(object), typeof(EntityListControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public object SelectedEntity
        {
            get => GetValue(SelectedEntityProperty);
            set => SetValue(SelectedEntityProperty, value);
        }

        // ========================================
        // Filter Properties
        // ========================================

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? "";
                OnPropertyChanged(nameof(SearchText));
                OnPropertyChanged(nameof(HasSearchText));
                RefreshFilter();
            }
        }

        public bool HasSearchText => !string.IsNullOrEmpty(_searchText);

        private bool _showVanilla = true;
        public bool ShowVanilla
        {
            get => _showVanilla;
            set
            {
                _showVanilla = value;
                OnPropertyChanged(nameof(ShowVanilla));
                RefreshFilter();
            }
        }

        private bool _showModified = true;
        public bool ShowModified
        {
            get => _showModified;
            set
            {
                _showModified = value;
                OnPropertyChanged(nameof(ShowModified));
                RefreshFilter();
            }
        }

        private bool _showNew = true;
        public bool ShowNew
        {
            get => _showNew;
            set
            {
                _showNew = value;
                OnPropertyChanged(nameof(ShowNew));
                RefreshFilter();
            }
        }

        // ========================================
        // Filtered View
        // ========================================

        public ICollectionView FilteredEntities => _entitiesView;

        public int FilteredCount => _entitiesView?.Cast<object>().Count() ?? 0;

        private static void OnEntitiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EntityListControl control)
            {
                control.SetupCollectionView();
            }
        }

        private void SetupCollectionView()
        {
            if (Entities == null)
            {
                _entitiesView = null;
                OnPropertyChanged(nameof(FilteredEntities));
                OnPropertyChanged(nameof(FilteredCount));
                return;
            }

            _entitiesView = CollectionViewSource.GetDefaultView(Entities);
            _entitiesView.Filter = FilterEntity;

            // Sort by ID by default
            _entitiesView.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));

            OnPropertyChanged(nameof(FilteredEntities));
            OnPropertyChanged(nameof(FilteredCount));
        }

        private bool FilterEntity(object item)
        {
            if (item == null) return false;

            // Get properties via reflection (ViewModels should have these)
            var type = item.GetType();
            var displayNameProp = type.GetProperty("DisplayName");
            var idProp = type.GetProperty("ID");
            var isVanillaProp = type.GetProperty("IsVanilla");
            var isModifiedProp = type.GetProperty("IsModified");
            var isNewProp = type.GetProperty("IsNew");

            // Filter by state
            bool isVanilla = isVanillaProp?.GetValue(item) as bool? ?? false;
            bool isModified = isModifiedProp?.GetValue(item) as bool? ?? false;
            bool isNew = isNewProp?.GetValue(item) as bool? ?? false;

            // If it's vanilla and not modified, check ShowVanilla
            if (isVanilla && !isModified && !ShowVanilla) return false;

            // If it's modified, check ShowModified
            if (isModified && !ShowModified) return false;

            // If it's new, check ShowNew
            if (isNew && !ShowNew) return false;

            // Filter by search text
            if (!string.IsNullOrEmpty(_searchText))
            {
                string displayName = displayNameProp?.GetValue(item) as string ?? "";
                int? id = idProp?.GetValue(item) as int?;

                bool matchesName = displayName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                bool matchesId = id?.ToString().Contains(_searchText) ?? false;

                if (!matchesName && !matchesId) return false;
            }

            return true;
        }

        private void RefreshFilter()
        {
            _entitiesView?.Refresh();
            OnPropertyChanged(nameof(FilteredCount));
        }

        // ========================================
        // Event Handlers
        // ========================================

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchText = "";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Raise event or command to add new entity
            MessageBox.Show($"Add new {EntityType} - not yet implemented", "Add Entity",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
