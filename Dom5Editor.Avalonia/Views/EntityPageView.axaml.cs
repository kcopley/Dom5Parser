using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.Ava.Views
{
    public partial class EntityPageView : UserControl
    {
        public EntityPageView()
        {
            InitializeComponent();
            PageParts.ItemTemplate = new PartSelector(this);
        }

        /// <summary>
        /// The template for each part of a page: the page's own parts by kind (the resources
        /// "Part.Header", ...), badge sections and panels by their type (the view's data templates;
        /// a panel without one yet shows its name).
        /// </summary>
        private sealed class PartSelector : IDataTemplate
        {
            private readonly EntityPageView _view;
            public PartSelector(EntityPageView view) => _view = view;

            public bool Match(object? data) => data != null;

            public Control? Build(object? data)
            {
                if (data is PagePart part)
                {
                    if (_view.TryFindResource("Part." + part.Kind, out var found) && found is IDataTemplate template)
                    {
                        var control = template.Build(part.Page);
                        if (control != null)
                            control.DataContext = part.Page;
                        return control;
                    }
                    return null; // a part this view doesn't show yet
                }
                foreach (var template in _view.DataTemplates)
                    if (template.Match(data))
                        return template.Build(data);
                // a panel not ported yet: its name, so the page shows what's missing
                return new TextBlock
                {
                    Text = data?.GetType().Name ?? "",
                    Foreground = Avalonia.Media.Brushes.Gray,
                    FontStyle = FontStyle.Italic,
                    Margin = new Avalonia.Thickness(0, 0, 0, 6),
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
            }
        }
    }
}
