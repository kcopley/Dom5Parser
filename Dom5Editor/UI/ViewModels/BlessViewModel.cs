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
    /// ViewModel for Bless entities.
    /// </summary>
    public class BlessViewModel : EntityViewModel
    {
        public BlessViewModel(Bless entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Bless Bless => (Bless)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from bless_badges.json.
        /// </summary>
        protected override string EntityTypeName => "bless";

        // ========================================
        // Clear Commands
        // ========================================

        /// <summary>
        /// Gets or sets whether #clearscales is active.
        /// </summary>
        public bool HasClearScales
        {
            get => _entity.HasClearCommand(Command.CLEARSCALES);
            set
            {
                SetCommandProperty(Command.CLEARSCALES, value, nameof(HasClearScales));
                RefreshScalesBadges();
            }
        }

        /// <summary>
        /// Gets or sets whether #clearfx is active.
        /// </summary>
        public bool HasClearFx
        {
            get => _entity.HasClearCommand(Command.CLEARFX);
            set
            {
                SetCommandProperty(Command.CLEARFX, value, nameof(HasClearFx));
            }
        }

        // ========================================
        // Badge Collections
        // ========================================

        private ObservableCollection<PropertyItem> _pathBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePathBadges;
        private ObservableCollection<PropertyItem> _scalesBadges;
        private ObservableCollection<AvailablePropertyItem> _availableScalesBadges;

        public ObservableCollection<PropertyItem> PathBadges
        {
            get { if (_pathBadges == null) RefreshPathBadges(); return _pathBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePathBadges
        {
            get { if (_availablePathBadges == null) RefreshPathBadges(); return _availablePathBadges; }
        }

        public ObservableCollection<PropertyItem> ScalesBadges
        {
            get { if (_scalesBadges == null) RefreshScalesBadges(); return _scalesBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableScalesBadges
        {
            get { if (_availableScalesBadges == null) RefreshScalesBadges(); return _availableScalesBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removePathBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPathBadgeCommand;
        private RelayCommand<PropertyItem> _removeScalesBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addScalesBadgeCommand;

        public RelayCommand<PropertyItem> RemovePathBadgeCommand => _removePathBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPathBadges);
        public RelayCommand<AvailablePropertyItem> AddPathBadgeCommand => _addPathBadgeCommand ??= CreateAddBadgeCommand(RefreshPathBadges);
        public RelayCommand<PropertyItem> RemoveScalesBadgeCommand => _removeScalesBadgeCommand ??= CreateRemoveBadgeCommand(RefreshScalesBadges);
        public RelayCommand<AvailablePropertyItem> AddScalesBadgeCommand => _addScalesBadgeCommand ??= CreateAddBadgeCommand(RefreshScalesBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshPathBadges()
        {
            var (active, available) = BuildBadgesFromSection("paths", BadgeValueChangedHandler);
            _pathBadges = active;
            _availablePathBadges = available;
            OnPropertyChanged(nameof(PathBadges));
            OnPropertyChanged(nameof(AvailablePathBadges));
        }

        private void RefreshScalesBadges()
        {
            var (active, available) = BuildBadgesFromSection("scales", BadgeValueChangedHandler);
            _scalesBadges = active;
            _availableScalesBadges = available;
            OnPropertyChanged(nameof(ScalesBadges));
            OnPropertyChanged(nameof(AvailableScalesBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            // Handle clear commands
            if (command == Command.CLEARSCALES)
            {
                OnPropertyChanged(nameof(HasClearScales));
            }
            if (command == Command.CLEARFX)
            {
                OnPropertyChanged(nameof(HasClearFx));
            }

            RefreshPathBadges();
            RefreshScalesBadges();
        }
    }
}
