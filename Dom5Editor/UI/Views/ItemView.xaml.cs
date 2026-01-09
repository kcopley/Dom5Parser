using System;
using System.Windows;
using System.Windows.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Interaction logic for ItemView.xaml
    /// </summary>
    public partial class ItemView : UserControl
    {
        public ItemView()
        {
            InitializeComponent();
        }

        private ItemViewModel ViewModel => DataContext as ItemViewModel;

        /// <summary>
        /// Navigate to the source item when CopyItem navigate button is clicked.
        /// </summary>
        private void OnCopyItemClick(object sender, RoutedEventArgs e)
        {
            var id = ViewModel?.CopyItemId;
            if (id.HasValue && id.Value != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("item", id.Value));
            }
        }

        /// <summary>
        /// Clear the copy item reference.
        /// </summary>
        private void OnClearCopyItemClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.CopyItemId = null;
            }
        }

        private void PrimaryPath_Changed(object sender, int pathId)
        {
            // Path binding handles the update; refresh display property
            ViewModel?.RefreshPathDisplay();
        }

        private void PrimaryLevel_Changed(object sender, int level)
        {
            // Level binding handles the update; refresh gem cost
            ViewModel?.RefreshPathDisplay();
        }

        private void SecondaryPath_Changed(object sender, int pathId)
        {
            // Path binding handles the update; refresh display property
            ViewModel?.RefreshPathDisplay();
        }

        private void SecondaryLevel_Changed(object sender, int level)
        {
            // Level binding handles the update; refresh gem cost
            ViewModel?.RefreshPathDisplay();
        }
    }
}
