using System;
using System.Collections.ObjectModel;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Editor.Data;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.Views
{
    public class NametypeViewModel : EntityViewModel
    {
        public NametypeViewModel(Nametype entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Nametype Nametype => (Nametype)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from nametype_badges.json.
        /// </summary>
        protected override string EntityTypeName => "nametype";

        // ========================================
        // Badge Collections
        // ========================================

        private ObservableCollection<PropertyItem> _namesBadges;
        private ObservableCollection<AvailablePropertyItem> _availableNamesBadges;

        public ObservableCollection<PropertyItem> NamesBadges
        {
            get { if (_namesBadges == null) RefreshNamesBadges(); return _namesBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableNamesBadges
        {
            get { if (_availableNamesBadges == null) RefreshNamesBadges(); return _availableNamesBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removeNamesBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addNamesBadgeCommand;

        public RelayCommand<PropertyItem> RemoveNamesBadgeCommand => _removeNamesBadgeCommand ??= CreateRemoveBadgeCommand(RefreshNamesBadges);
        public RelayCommand<AvailablePropertyItem> AddNamesBadgeCommand => _addNamesBadgeCommand ??= CreateAddBadgeCommand(RefreshNamesBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshNamesBadges()
        {
            var (active, available) = BuildBadgesFromSection("names", BadgeValueChangedHandler);
            _namesBadges = active;
            _availableNamesBadges = available;
            OnPropertyChanged(nameof(NamesBadges));
            OnPropertyChanged(nameof(AvailableNamesBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            RefreshNamesBadges();
        }
    }
}
