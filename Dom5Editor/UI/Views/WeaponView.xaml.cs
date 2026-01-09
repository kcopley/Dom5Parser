using System.Windows;
using System.Windows.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Interaction logic for WeaponView.xaml
    /// </summary>
    public partial class WeaponView : UserControl
    {
        public WeaponView()
        {
            InitializeComponent();
        }

        private WeaponViewModel ViewModel => DataContext as WeaponViewModel;

        /// <summary>
        /// Navigate to the source weapon when CopyWeapon navigate button is clicked.
        /// </summary>
        private void OnCopyWeaponClick(object sender, RoutedEventArgs e)
        {
            var id = ViewModel?.CopyWeaponId;
            if (id.HasValue && id.Value != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("weapon", id.Value));
            }
        }
    }
}
