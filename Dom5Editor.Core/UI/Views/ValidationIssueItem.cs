using Dom5Edit.Entities;
using Dom5Edit.Validation;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// One issue Validate found, as the full issue list shows it (the WPF ValidationReportWindow,
    /// the Avalonia ValidationWindow): its severity, message, category and the entity it's about.
    /// </summary>
    public class ValidationIssueItem
    {
        private readonly ValidationIssue _issue;

        public ValidationIssueItem(ValidationIssue issue)
        {
            _issue = issue;
        }

        public ValidationSeverity Severity => _issue.Severity;
        public bool IsError => Severity == ValidationSeverity.Error;
        public bool IsWarning => Severity == ValidationSeverity.Warning;
        public bool IsInfo => Severity == ValidationSeverity.Info;
        public string Message => _issue.Message;
        public string Category => _issue.Category;

        /// <summary>The entity the issue is about, if it has a page.</summary>
        public IDEntity? Entity => CanNavigate ? _issue.Entity as IDEntity : null;

        /// <summary>
        /// Display string for the entity, e.g., "Monster #5001: MyMonster"
        /// </summary>
        public string? EntityDisplay
        {
            get
            {
                if (_issue.Entity == null)
                    return null;

                var typeName = _issue.Entity.GetType().Name;
                if (_issue.Entity is IDEntity idEntity)
                {
                    var name = idEntity.Name;
                    if (!string.IsNullOrEmpty(name))
                        return $"{typeName} #{idEntity.ID}: {name}";
                    return $"{typeName} #{idEntity.ID}";
                }
                return typeName;
            }
        }

        /// <summary>
        /// Whether this issue can be navigated to (has a valid entity with a tab).
        /// </summary>
        public bool CanNavigate
        {
            get
            {
                if (_issue.Entity == null)
                    return false;

                if (_issue.Entity is not IDEntity)
                    return false;

                // Check if entity type has a navigable tab
                var entityType = GetEntityTypeOrNull();
                return entityType.HasValue && HasTabForEntityType(entityType.Value);
            }
        }

        public EntityType EntityType => GetEntityTypeOrNull() ?? EntityType.MONSTER;

        public int EntityId
        {
            get
            {
                if (_issue.Entity is IDEntity idEntity)
                    return idEntity.ID;
                return 0;
            }
        }

        private EntityType? GetEntityTypeOrNull()
        {
            return _issue.Entity switch
            {
                Monster => EntityType.MONSTER,
                Weapon => EntityType.WEAPON,
                Armor => EntityType.ARMOR,
                Spell => EntityType.SPELL,
                Item => EntityType.ITEM,
                Site => EntityType.SITE,
                Nation => EntityType.NATION,
                Event => EntityType.EVENT,
                Mercenary => EntityType.MERCENARY,
                Poptype => EntityType.POPTYPE,
                Nametype => EntityType.NAMETYPE,
                _ => null
            };
        }

        private static bool HasTabForEntityType(EntityType type)
        {
            // These entity types have tabs in the main window
            return type switch
            {
                EntityType.MONSTER => true,
                EntityType.WEAPON => true,
                EntityType.ARMOR => true,
                EntityType.SPELL => true,
                EntityType.ITEM => true,
                EntityType.SITE => true,
                EntityType.NATION => true,
                EntityType.EVENT => true,
                EntityType.MERCENARY => true,
                EntityType.POPTYPE => true,
                EntityType.NAMETYPE => true,
                _ => false
            };
        }
    }
}
