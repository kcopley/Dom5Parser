using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        // Custom magic (random paths)
        private void OnCustomMagicAddRequested(object sender, EventArgs e) => ViewModel?.AddCustomMagicCommand?.Execute(null);
        private void OnCustomMagicRemoveRequested(object sender, CustomMagicItem item) => ViewModel?.RemoveCustomMagicCommand?.Execute(item);

        // Equipment navigation - click weapon/armor name to navigate
        private void OnEquipmentClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is EquipmentItem item)
            {
                ViewModel?.NavigateToReferenceCommand.Execute((item.EntityType, item.ID));
            }
        }

        // CopyStats navigation - click to navigate to source monster
        private void OnCopyStatsClick(object sender, MouseButtonEventArgs e)
        {
            var id = ViewModel?.CopyStatsId ?? 0;
            if (id != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("monster", id));
            }
        }

        // CopySpr navigation - click to navigate to source monster
        private void OnCopySprClick(object sender, MouseButtonEventArgs e)
        {
            var id = ViewModel?.CopySprId ?? 0;
            if (id != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("monster", id));
            }
        }
    }
}
