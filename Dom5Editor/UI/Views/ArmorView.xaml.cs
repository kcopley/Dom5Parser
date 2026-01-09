using System.Windows;
using System.Windows.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Interaction logic for ArmorView.xaml
    /// </summary>
    public partial class ArmorView : UserControl
    {
        public ArmorView()
        {
            InitializeComponent();
        }

        private ArmorViewModel ViewModel => DataContext as ArmorViewModel;

        /// <summary>
        /// Navigate to the source armor when CopyArmor navigate button is clicked.
        /// </summary>
        private void OnCopyArmorClick(object sender, RoutedEventArgs e)
        {
            var id = ViewModel?.CopyArmorId;
            if (id.HasValue && id.Value != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("armor", id.Value));
            }
        }
    }
}
