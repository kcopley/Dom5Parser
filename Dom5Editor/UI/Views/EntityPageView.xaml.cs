using System.Windows.Controls;
using System.Windows.Input;

namespace Dom5Editor.UI.Views
{
    public partial class EntityPageView : UserControl
    {
        public EntityPageView()
        {
            InitializeComponent();
        }

        /// <summary>Enter in a text box saves it (as leaving it does).</summary>
        private void CommitOnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox box)
            {
                box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                e.Handled = true;
            }
        }
    }
}
