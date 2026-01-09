using System;
using System.Collections.ObjectModel;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Editor.Data;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// ViewModel for Poptype entities.
    /// </summary>
    public class PoptypeViewModel : EntityViewModel
    {
        public PoptypeViewModel(Poptype entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Poptype Poptype => (Poptype)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from poptype_badges.json.
        /// </summary>
        protected override string EntityTypeName => "poptype";

        // ========================================
        // Clear Command
        // ========================================

        /// <summary>
        /// Gets or sets whether #cleardef is active.
        /// </summary>
        public bool HasClearDef
        {
            get => _entity.HasClearCommand(Command.CLEARDEF);
            set
            {
                SetCommandProperty(Command.CLEARDEF, value, nameof(HasClearDef));
                RefreshRecruitmentBadges();
                RefreshDefenseBadges();
            }
        }

        // ========================================
        // Badge Collections
        // ========================================

        private ObservableCollection<PropertyItem> _recruitmentBadges;
        private ObservableCollection<AvailablePropertyItem> _availableRecruitmentBadges;
        private ObservableCollection<PropertyItem> _defenseBadges;
        private ObservableCollection<AvailablePropertyItem> _availableDefenseBadges;

        public ObservableCollection<PropertyItem> RecruitmentBadges
        {
            get { if (_recruitmentBadges == null) RefreshRecruitmentBadges(); return _recruitmentBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableRecruitmentBadges
        {
            get { if (_availableRecruitmentBadges == null) RefreshRecruitmentBadges(); return _availableRecruitmentBadges; }
        }

        public ObservableCollection<PropertyItem> DefenseBadges
        {
            get { if (_defenseBadges == null) RefreshDefenseBadges(); return _defenseBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableDefenseBadges
        {
            get { if (_availableDefenseBadges == null) RefreshDefenseBadges(); return _availableDefenseBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removeRecruitmentBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addRecruitmentBadgeCommand;
        private RelayCommand<PropertyItem> _removeDefenseBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addDefenseBadgeCommand;

        public RelayCommand<PropertyItem> RemoveRecruitmentBadgeCommand => _removeRecruitmentBadgeCommand ??= CreateRemoveBadgeCommand(RefreshRecruitmentBadges);
        public RelayCommand<AvailablePropertyItem> AddRecruitmentBadgeCommand => _addRecruitmentBadgeCommand ??= CreateAddBadgeCommand(RefreshRecruitmentBadges);
        public RelayCommand<PropertyItem> RemoveDefenseBadgeCommand => _removeDefenseBadgeCommand ??= CreateRemoveBadgeCommand(RefreshDefenseBadges);
        public RelayCommand<AvailablePropertyItem> AddDefenseBadgeCommand => _addDefenseBadgeCommand ??= CreateAddBadgeCommand(RefreshDefenseBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshRecruitmentBadges()
        {
            var (active, available) = BuildBadgesFromSection("recruitment", BadgeValueChangedHandler);
            _recruitmentBadges = active;
            _availableRecruitmentBadges = available;
            OnPropertyChanged(nameof(RecruitmentBadges));
            OnPropertyChanged(nameof(AvailableRecruitmentBadges));
        }

        private void RefreshDefenseBadges()
        {
            var (active, available) = BuildBadgesFromSection("defense", BadgeValueChangedHandler);
            _defenseBadges = active;
            _availableDefenseBadges = available;
            OnPropertyChanged(nameof(DefenseBadges));
            OnPropertyChanged(nameof(AvailableDefenseBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            // Handle clear command
            if (command == Command.CLEARDEF)
            {
                OnPropertyChanged(nameof(HasClearDef));
            }

            RefreshRecruitmentBadges();
            RefreshDefenseBadges();
        }
    }
}
