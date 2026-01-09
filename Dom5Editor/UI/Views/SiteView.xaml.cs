using System.Windows;
using System.Windows.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Interaction logic for SiteView.xaml
    /// </summary>
    public partial class SiteView : UserControl
    {
        public SiteView()
        {
            InitializeComponent();
        }

        private SiteViewModel ViewModel => DataContext as SiteViewModel;

        /// <summary>
        /// Navigate to the source site when CopySite navigate button is clicked.
        /// </summary>
        private void OnCopySiteClick(object sender, RoutedEventArgs e)
        {
            var id = ViewModel?.CopySiteId;
            if (id.HasValue && id.Value != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("site", id.Value));
            }
        }
    }
}
