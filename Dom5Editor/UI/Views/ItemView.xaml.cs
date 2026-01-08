using System;
using System.Windows.Controls;
using System.Windows.Input;

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
        /// Navigate to the source item when CopyItem reference is clicked.
        /// </summary>
        private void OnCopyItemClick(object sender, MouseButtonEventArgs e)
        {
            var id = ViewModel?.CopyItemId ?? 0;
            if (id != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("item", id));
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
