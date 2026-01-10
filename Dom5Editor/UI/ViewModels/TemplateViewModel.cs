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
    /// ViewModel for Template entities (AI pretender design templates).
    /// </summary>
    public class TemplateViewModel : EntityViewModel
    {
        public TemplateViewModel(Template entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Template Template => (Template)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from template_badges.json.
        /// </summary>
        protected override string EntityTypeName => "template";

        /// <summary>
        /// The nation ID this template is for.
        /// </summary>
        public int NationId => Template.NationId;

        // ========================================
        // Badge Collections
        // ========================================

        private ObservableCollection<PropertyItem> _pretenderBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePretenderBadges;
        private ObservableCollection<PropertyItem> _scaleBadges;
        private ObservableCollection<AvailablePropertyItem> _availableScaleBadges;
        private ObservableCollection<PropertyItem> _researchBadges;
        private ObservableCollection<AvailablePropertyItem> _availableResearchBadges;

        public ObservableCollection<PropertyItem> PretenderBadges
        {
            get { if (_pretenderBadges == null) RefreshPretenderBadges(); return _pretenderBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePretenderBadges
        {
            get { if (_availablePretenderBadges == null) RefreshPretenderBadges(); return _availablePretenderBadges; }
        }

        public ObservableCollection<PropertyItem> ScaleBadges
        {
            get { if (_scaleBadges == null) RefreshScaleBadges(); return _scaleBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableScaleBadges
        {
            get { if (_availableScaleBadges == null) RefreshScaleBadges(); return _availableScaleBadges; }
        }

        public ObservableCollection<PropertyItem> ResearchBadges
        {
            get { if (_researchBadges == null) RefreshResearchBadges(); return _researchBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableResearchBadges
        {
            get { if (_availableResearchBadges == null) RefreshResearchBadges(); return _availableResearchBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removePretenderBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPretenderBadgeCommand;
        private RelayCommand<PropertyItem> _removeScaleBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addScaleBadgeCommand;
        private RelayCommand<PropertyItem> _removeResearchBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addResearchBadgeCommand;

        public RelayCommand<PropertyItem> RemovePretenderBadgeCommand => _removePretenderBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPretenderBadges);
        public RelayCommand<AvailablePropertyItem> AddPretenderBadgeCommand => _addPretenderBadgeCommand ??= CreateAddBadgeCommand(RefreshPretenderBadges);
        public RelayCommand<PropertyItem> RemoveScaleBadgeCommand => _removeScaleBadgeCommand ??= CreateRemoveBadgeCommand(RefreshScaleBadges);
        public RelayCommand<AvailablePropertyItem> AddScaleBadgeCommand => _addScaleBadgeCommand ??= CreateAddBadgeCommand(RefreshScaleBadges);
        public RelayCommand<PropertyItem> RemoveResearchBadgeCommand => _removeResearchBadgeCommand ??= CreateRemoveBadgeCommand(RefreshResearchBadges);
        public RelayCommand<AvailablePropertyItem> AddResearchBadgeCommand => _addResearchBadgeCommand ??= CreateAddBadgeCommand(RefreshResearchBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshPretenderBadges()
        {
            var (active, available) = BuildBadgesFromSection("pretender", BadgeValueChangedHandler);
            _pretenderBadges = active;
            _availablePretenderBadges = available;
            OnPropertyChanged(nameof(PretenderBadges));
            OnPropertyChanged(nameof(AvailablePretenderBadges));
        }

        private void RefreshScaleBadges()
        {
            var (active, available) = BuildBadgesFromSection("scales", BadgeValueChangedHandler);
            _scaleBadges = active;
            _availableScaleBadges = available;
            OnPropertyChanged(nameof(ScaleBadges));
            OnPropertyChanged(nameof(AvailableScaleBadges));
        }

        private void RefreshResearchBadges()
        {
            var (active, available) = BuildBadgesFromSection("research", BadgeValueChangedHandler);
            _researchBadges = active;
            _availableResearchBadges = available;
            OnPropertyChanged(nameof(ResearchBadges));
            OnPropertyChanged(nameof(AvailableResearchBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            RefreshPretenderBadges();
            RefreshScaleBadges();
            RefreshResearchBadges();
        }
    }
}
