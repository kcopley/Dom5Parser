using System.Windows.Controls;
using System.Windows.Input;

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
        /// Navigate to the source armor when CopyArmor reference is clicked.
        /// </summary>
        private void OnCopyArmorClick(object sender, MouseButtonEventArgs e)
        {
            var id = ViewModel?.CopyArmorId ?? 0;
            if (id != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("armor", id));
            }
        }
    }
}
