using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dom5Edit;
using Dom5Edit.Entities;
using Dom5Edit.Validation;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Validation report dialog that displays validation issues with filtering and navigation.
    /// </summary>
    public partial class ValidationReportWindow : Window
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly List<ValidationIssueItem> _allIssues;
        private readonly Dom5Edit.Validation.ValidationResult _validationResult;
        private ICollectionView _filteredView;

        public ValidationReportWindow(Dom5Edit.Validation.ValidationResult result, MainWindowViewModel mainViewModel)
        {
            _validationResult = result;
            InitializeComponent();
            _mainViewModel = mainViewModel;

            // Wrap issues in UI-friendly items
            _allIssues = result.Issues.Select(i => new ValidationIssueItem(i)).ToList();

            // Set up filtered view
            _filteredView = CollectionViewSource.GetDefaultView(_allIssues);
            _filteredView.Filter = FilterIssue;
            IssuesListBox.ItemsSource = _filteredView;

            // Update summary counts
            ErrorCountText.Text = $"{result.ErrorCount} Error{(result.ErrorCount == 1 ? "" : "s")}";
            WarningCountText.Text = $"{result.WarningCount} Warning{(result.WarningCount == 1 ? "" : "s")}";
            InfoCountText.Text = $"{result.InfoCount} Info";

            UpdateFilteredCount();
        }

        private bool FilterIssue(object item)
        {
            if (item is not ValidationIssueItem issue)
                return false;

            // Filter by severity
            bool showBySeverity = issue.Severity switch
            {
                ValidationSeverity.Error => ShowErrorsCheck.IsChecked == true,
                ValidationSeverity.Warning => ShowWarningsCheck.IsChecked == true,
                ValidationSeverity.Info => ShowInfoCheck.IsChecked == true,
                _ => true
            };

            if (!showBySeverity)
                return false;

            // Filter by search text
            var searchText = SearchBox.Text?.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                var lowerSearch = searchText.ToLowerInvariant();
                return issue.Message?.ToLowerInvariant().Contains(lowerSearch) == true ||
                       issue.Category?.ToLowerInvariant().Contains(lowerSearch) == true ||
                       issue.EntityDisplay?.ToLowerInvariant().Contains(lowerSearch) == true;
            }

            return true;
        }

        private void UpdateFilteredCount()
        {
            if (_filteredView == null)
                return;
            var count = _filteredView.Cast<object>().Count();
            FilteredCountText.Text = $"{count} issue{(count == 1 ? "" : "s")}";
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (_filteredView == null)
                return;
            _filteredView.Refresh();
            UpdateFilteredCount();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_filteredView == null)
                return;
            _filteredView.Refresh();
            UpdateFilteredCount();
        }

        private void NavigateToIssue(ValidationIssueItem issue)
        {
            if (issue == null || !issue.CanNavigate)
                return;

            _mainViewModel.NavigateToEntity(issue.EntityType, issue.EntityId);
            DialogResult = true;
            Close();
        }

        private void IssuesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IssuesListBox.SelectedItem is ValidationIssueItem issue)
            {
                NavigateToIssue(issue);
            }
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ValidationIssueItem issue)
            {
                NavigateToIssue(issue);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|Markdown Files (*.md)|*.md|All Files (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = "validation_report.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _validationResult.ExportToFile(dialog.FileName);
                    MessageBox.Show($"Validation report exported to:\n{dialog.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export report:\n{ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }

    /// <summary>
    /// Wrapper around ValidationIssue that adds UI-specific properties for binding.
    /// </summary>
    public class ValidationIssueItem
    {
        private readonly ValidationIssue _issue;

        public ValidationIssueItem(ValidationIssue issue)
        {
            _issue = issue;
        }

        public ValidationSeverity Severity => _issue.Severity;
        public string Message => _issue.Message;
        public string Category => _issue.Category;

        /// <summary>
        /// Display string for the entity, e.g., "Monster #5001: MyMonster"
        /// </summary>
        public string EntityDisplay
        {
            get
            {
                if (_issue.Entity == null)
                    return null;

                var typeName = _issue.Entity.GetType().Name;
                if (_issue.Entity is IDEntity idEntity)
                {
                    var name = idEntity.Name;
                    if (!string.IsNullOrEmpty(name))
                        return $"{typeName} #{idEntity.ID}: {name}";
                    return $"{typeName} #{idEntity.ID}";
                }
                return typeName;
            }
        }

        /// <summary>
        /// Whether this issue can be navigated to (has a valid entity with a tab).
        /// </summary>
        public bool CanNavigate
        {
            get
            {
                if (_issue.Entity == null)
                    return false;

                if (_issue.Entity is not IDEntity)
                    return false;

                // Check if entity type has a navigable tab
                var entityType = GetEntityTypeOrNull();
                return entityType.HasValue && HasTabForEntityType(entityType.Value);
            }
        }

        public EntityType EntityType => GetEntityTypeOrNull() ?? EntityType.MONSTER;

        public int EntityId
        {
            get
            {
                if (_issue.Entity is IDEntity idEntity)
                    return idEntity.ID;
                return 0;
            }
        }

        private EntityType? GetEntityTypeOrNull()
        {
            return _issue.Entity switch
            {
                Monster => EntityType.MONSTER,
                Weapon => EntityType.WEAPON,
                Armor => EntityType.ARMOR,
                Spell => EntityType.SPELL,
                Item => EntityType.ITEM,
                Site => EntityType.SITE,
                Nation => EntityType.NATION,
                Event => EntityType.EVENT,
                Mercenary => EntityType.MERCENARY,
                Poptype => EntityType.POPTYPE,
                Nametype => EntityType.NAMETYPE,
                _ => null
            };
        }

        private static bool HasTabForEntityType(EntityType type)
        {
            // These entity types have tabs in the main window
            return type switch
            {
                EntityType.MONSTER => true,
                EntityType.WEAPON => true,
                EntityType.ARMOR => true,
                EntityType.SPELL => true,
                EntityType.ITEM => true,
                EntityType.SITE => true,
                EntityType.NATION => true,
                EntityType.EVENT => true,
                EntityType.MERCENARY => true,
                EntityType.POPTYPE => true,
                EntityType.NAMETYPE => true,
                _ => false
            };
        }
    }
}
