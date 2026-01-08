using System.Windows.Controls;
using System.Windows.Input;

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
        /// Navigate to the source weapon when CopyWeapon reference is clicked.
        /// </summary>
        private void OnCopyWeaponClick(object sender, MouseButtonEventArgs e)
        {
            var id = ViewModel?.CopyWeaponId ?? 0;
            if (id != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("weapon", id));
            }
        }
    }
}
