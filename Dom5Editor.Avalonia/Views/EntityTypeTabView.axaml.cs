using System.ComponentModel;
using Avalonia.Controls;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.Ava.Views
{
    public partial class EntityTypeTabView : UserControl
    {
        private EntityTypeTab? _tab;

        public EntityTypeTabView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => Attach(DataContext as EntityTypeTab);
            Sort.SelectionChanged += (s, e) =>
            {
                if (_tab != null)
                    _tab.SortBy = Sort.SelectedIndex == 1 ? "DisplayName" : "ID";
            };
        }

        private void Attach(EntityTypeTab? tab)
        {
            if (_tab != null)
            {
                _tab.PropertyChanged -= OnTabChanged;
                _tab.Items.CollectionChanged -= OnItemsChanged;
            }
            _tab = tab;
            if (tab == null)
                return;
            tab.PropertyChanged += OnTabChanged;
            tab.Items.CollectionChanged += OnItemsChanged; // (an entity made or deleted)
            Sort.SelectedIndex = tab.SortBy == "DisplayName" ? 1 : 0;
            Refresh();
        }

        // the list again when a filter changes (the core's rules: EntityTypeTab.Shown)
        private void OnTabChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(EntityTypeTab.SearchText) or nameof(EntityTypeTab.ShowVanilla) or nameof(EntityTypeTab.ShowModified)
                or nameof(EntityTypeTab.ShowNew) or nameof(EntityTypeTab.Facet) or nameof(EntityTypeTab.SortBy) or nameof(EntityTypeTab.Items))
                Refresh();
            else if (e.PropertyName == nameof(EntityTypeTab.SelectedItem) && _tab?.SelectedItem != null)
                List.ScrollIntoView(_tab.SelectedItem);
        }

        private void OnItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Refresh();

        private void Refresh()
        {
            if (_tab == null)
                return;
            var shown = _tab.Shown().ToList();
            var selected = _tab.SelectedItem;
            List.ItemsSource = shown;
            if (selected != null && shown.Contains(selected))
                List.SelectedItem = selected;
            Count.Text = $"{shown.Count} items";
        }
    }
}
