using System.Windows.Controls;
using Dom5Edit.Commands;

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

        // Type commands
        private void OnTypeCommandAdded(object sender, Command e) => ViewModel?.AddTypeCommand(e);
        private void OnTypeCommandRemoved(object sender, Command e) => ViewModel?.RemoveTypeCommand(e);

        // Leader commands
        private void OnLeaderCommandAdded(object sender, Command e) => ViewModel?.AddLeaderCommand(e);
        private void OnLeaderCommandRemoved(object sender, Command e) => ViewModel?.RemoveLeaderCommand(e);

        // Movement commands
        private void OnMovementCommandAdded(object sender, Command e) => ViewModel?.AddMovementCommand(e);
        private void OnMovementCommandRemoved(object sender, Command e) => ViewModel?.RemoveMovementCommand(e);

        // Resistances
        private void OnResistanceAdded(object sender, (Command Command, int Value) e) => ViewModel?.AddResistance(e.Command, e.Value);
        private void OnResistanceRemoved(object sender, Command e) => ViewModel?.RemoveResistance(e);

        // Combat
        private void OnCombatAdded(object sender, (Command Command, int Value) e) => ViewModel?.AddCombat(e.Command, e.Value);
        private void OnCombatRemoved(object sender, Command e) => ViewModel?.RemoveCombat(e);

        // Auras
        private void OnAuraAdded(object sender, (Command Command, int Value) e) => ViewModel?.AddAura(e.Command, e.Value);
        private void OnAuraRemoved(object sender, Command e) => ViewModel?.RemoveAura(e);

        // Special commands
        private void OnSpecialCommandAdded(object sender, Command e) => ViewModel?.AddSpecialCommand(e);
        private void OnSpecialCommandRemoved(object sender, Command e) => ViewModel?.RemoveSpecialCommand(e);

        // Magic paths
        private void OnMagicPathAdded(object sender, (int PathId, int Level) e) => ViewModel?.AddMagicPath(e.PathId, e.Level);
        private void OnMagicPathRemoved(object sender, int pathId) => ViewModel?.RemoveMagicPath(pathId);
    }
}
