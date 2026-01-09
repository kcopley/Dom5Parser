using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dom5Editor.UI.Controls;

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
        private void OnCopyStatsClick(object sender, RoutedEventArgs e)
        {
            var id = ViewModel?.CopyStatsId;
            if (id.HasValue && id.Value != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("monster", id.Value));
            }
        }

        // CopySpr navigation - click to navigate to source monster
        private void OnCopySprClick(object sender, RoutedEventArgs e)
        {
            var id = ViewModel?.CopySprId;
            if (id.HasValue && id.Value != 0)
            {
                ViewModel?.NavigateToReferenceCommand.Execute(("monster", id.Value));
            }
        }

        // Equipment add - auto-add when selection is made
        private void OnWeaponSelectionChanged(object sender, ReferenceSelectionChangedEventArgs e)
        {
            if (e.NewId > 0)
            {
                ViewModel?.AddWeaponByIdCommand?.Execute(e.NewId);
                // Clear the selection so user can add another
                if (sender is SearchableReferenceComboBox combo)
                {
                    combo.SetSelectedIdSilent(null);
                }
            }
        }

        private void OnArmorSelectionChanged(object sender, ReferenceSelectionChangedEventArgs e)
        {
            if (e.NewId > 0)
            {
                ViewModel?.AddArmorByIdCommand?.Execute(e.NewId);
                // Clear the selection so user can add another
                if (sender is SearchableReferenceComboBox combo)
                {
                    combo.SetSelectedIdSilent(null);
                }
            }
        }
    }
}
