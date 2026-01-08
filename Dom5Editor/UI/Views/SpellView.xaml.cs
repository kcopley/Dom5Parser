using System.Windows.Controls;
using System.Windows.Input;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Interaction logic for SpellView.xaml
    /// </summary>
    public partial class SpellView : UserControl
    {
        public SpellView()
        {
            InitializeComponent();
        }

        private SpellViewModel ViewModel => DataContext as SpellViewModel;

        /// <summary>
        /// Navigate to the source spell when CopySpell reference is clicked.
        /// </summary>
        private void OnCopySpellClick(object sender, MouseButtonEventArgs e)
        {
            var id = ViewModel?.CopySpellId ?? 0;
            if (id != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("spell", id));
            }
        }

        /// <summary>
        /// Navigate to the next spell in chain when NextSpell reference is clicked.
        /// </summary>
        private void OnNextSpellClick(object sender, MouseButtonEventArgs e)
        {
            var id = ViewModel?.NextSpellId ?? 0;
            if (id != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("spell", id));
            }
        }
    }
}
