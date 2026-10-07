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
            // a report for the mod's author: what goes wrong in game, lines the game ignores (with
            // "did you mean"), what's worth a look; each with its line (Dom5Edit.Validation.ModReport)
            var session = _mainViewModel.Session;
            var name = session?.Mod.ModName ?? "mod";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Markdown (*.md)|*.md|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = ".md",
                FileName = string.Concat((name + " - editor report").Split(System.IO.Path.GetInvalidFileNameChars())) + ".md",
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (session != null)
                        System.IO.File.WriteAllText(dialog.FileName, Dom5Edit.Validation.ModReport.Write(session.Mod, _validationResult, session.Events.Problems));
                    else
                        _validationResult.ExportToFile(dialog.FileName);
                    MessageBox.Show($"The report for the mod's author is saved:\n{dialog.FileName}", "Report saved", MessageBoxButton.OK, MessageBoxImage.Information);
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
}
