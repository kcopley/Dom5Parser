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
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedEntityChanged));

        /// <summary>A selection made elsewhere (navigation) scrolls the list to it.</summary>
        private static void OnSelectedEntityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EntityListControl control && e.NewValue != null)
                control.Dispatcher.BeginInvoke(new Action(() => control.EntityListBox.ScrollIntoView(e.NewValue)),
                    System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>The "+ New" button: makes a new entity of the list's type.</summary>
        public static readonly DependencyProperty NewCommandProperty =
            DependencyProperty.Register(nameof(NewCommand), typeof(System.Windows.Input.ICommand), typeof(EntityListControl));

        public System.Windows.Input.ICommand NewCommand
        {
            get => (System.Windows.Input.ICommand)GetValue(NewCommandProperty);
            set => SetValue(NewCommandProperty, value);
        }

        /// <summary>The "Delete" button: deletes the selected mod entity (or the mod's changes to a vanilla one).</summary>
        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(System.Windows.Input.ICommand), typeof(EntityListControl));

        public System.Windows.Input.ICommand DeleteCommand
        {
            get => (System.Windows.Input.ICommand)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        public object SelectedEntity
        {
            get => GetValue(SelectedEntityProperty);
            set => SetValue(SelectedEntityProperty, value);
        }

        // ========================================
        // Filter Properties
        // ========================================

        // Dependency properties, so the list's owner can keep them (a tab remembers its filter)

        public static readonly DependencyProperty SearchTextProperty = FilterProperty(nameof(SearchText), "");
        public static readonly DependencyProperty ShowVanillaProperty = FilterProperty(nameof(ShowVanilla), true);
        public static readonly DependencyProperty ShowModifiedProperty = FilterProperty(nameof(ShowModified), true);
        public static readonly DependencyProperty ShowNewProperty = FilterProperty(nameof(ShowNew), true);

        /// <summary>The type's own filter ("Rituals", "In a chain"); "All" for none.</summary>
        public static readonly DependencyProperty FacetProperty = FilterProperty(nameof(Facet), "All");

        public string Facet
        {
            get => (string)GetValue(FacetProperty) ?? "All";
            set => SetValue(FacetProperty, value ?? "All");
        }

        /// <summary>The type's own filters, or null (no filter box).</summary>
        public static readonly DependencyProperty FacetsProperty =
            DependencyProperty.Register(nameof(Facets), typeof(IEnumerable<string>), typeof(EntityListControl), new PropertyMetadata(null));

        public IEnumerable<string>? Facets
        {
            get => (IEnumerable<string>?)GetValue(FacetsProperty);
            set => SetValue(FacetsProperty, value);
        }

        /// <summary>Sort by "ID" or "DisplayName".</summary>
        public static readonly DependencyProperty SortByProperty =
            DependencyProperty.Register(nameof(SortBy), typeof(string), typeof(EntityListControl),
                new FrameworkPropertyMetadata("ID", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((EntityListControl)d).ApplySort()));

        public string SortBy
        {
            get => (string)GetValue(SortByProperty) ?? "ID";
            set => SetValue(SortByProperty, value);
        }

        private void ApplySort()
        {
            if (_entitiesView == null)
                return;
            _entitiesView.SortDescriptions.Clear();
            _entitiesView.SortDescriptions.Add(new SortDescription(SortBy == "DisplayName" ? "DisplayName" : "SortKey", ListSortDirection.Ascending));
            RefreshFilter();
        }

        private static DependencyProperty FilterProperty(string name, object defaultValue) =>
            DependencyProperty.Register(name, defaultValue.GetType(), typeof(EntityListControl),
                new FrameworkPropertyMetadata(defaultValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) =>
                {
                    var control = (EntityListControl)d;
                    control.OnPropertyChanged(nameof(HasSearchText));
                    control.RefreshFilter();
                }));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty) ?? "";
            set => SetValue(SearchTextProperty, value ?? "");
        }

        public bool HasSearchText => !string.IsNullOrEmpty(SearchText);

        public bool ShowVanilla
        {
            get => (bool)GetValue(ShowVanillaProperty);
            set => SetValue(ShowVanillaProperty, value);
        }

        public bool ShowModified
        {
            get => (bool)GetValue(ShowModifiedProperty);
            set => SetValue(ShowModifiedProperty, value);
        }

        public bool ShowNew
        {
            get => (bool)GetValue(ShowNewProperty);
            set => SetValue(ShowNewProperty, value);
        }

        /// <summary>Puts the cursor in the search box (Ctrl+F).</summary>
        public void FocusSearch()
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
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

            _entitiesView.SortDescriptions.Clear();
            _entitiesView.SortDescriptions.Add(new SortDescription(SortBy == "DisplayName" ? "DisplayName" : "SortKey", ListSortDirection.Ascending));

            OnPropertyChanged(nameof(FilteredEntities));
            OnPropertyChanged(nameof(FilteredCount));
        }

        // the core's rules (EntityTypeTab.Matches), shared with the Mac/Linux editor
        private bool FilterEntity(object item) =>
            item is UI.ViewModels.EntityListItem row && UI.ViewModels.EntityTypeTab.Matches(row, ShowVanilla, ShowModified, ShowNew, Facet, SearchText);

        private void RefreshFilter()
        {
            _entitiesView?.Refresh();
            OnPropertyChanged(nameof(FilteredCount));
            // keep the selected entity in sight
            if (SelectedEntity != null)
                Dispatcher.BeginInvoke(new Action(() => EntityListBox.ScrollIntoView(SelectedEntity)),
                    System.Windows.Threading.DispatcherPriority.Background);
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
            if (NewCommand?.CanExecute(null) == true)
                NewCommand.Execute(null);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteCommand?.CanExecute(SelectedEntity) == true)
                DeleteCommand.Execute(SelectedEntity);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
