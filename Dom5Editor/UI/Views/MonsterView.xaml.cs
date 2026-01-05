using System.Windows.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// View for editing Monster entities.
    /// </summary>
    public partial class MonsterView : UserControl
    {
        public MonsterView()
        {
            InitializeComponent();
        }

        private MonsterViewModel ViewModel => DataContext as MonsterViewModel;

        // Magic paths
        private void OnMagicPathAdded(object sender, (int PathId, int Level) e) => ViewModel?.AddMagicPath(e.PathId, e.Level);
        private void OnMagicPathRemoved(object sender, int pathId) => ViewModel?.RemoveMagicPath(pathId);
    }
}
