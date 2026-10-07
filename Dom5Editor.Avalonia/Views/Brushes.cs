using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Dom5Editor.Ava.Views
{
    /// <summary>Converters to the theme's brushes.</summary>
    public static class Brushes
    {
        private static IBrush Brush(string key) =>
            Application.Current!.TryFindResource(key, out var value) && value is IBrush brush ? brush : Avalonia.Media.Brushes.Gray;

        /// <summary>True: the error colour (something goes wrong in game); false: the warning colour.</summary>
        public static readonly IValueConverter ErrorOrWarning = new FuncValueConverter<bool, IBrush>(wrong => Brush(wrong ? "ErrorBrush" : "WarningBrush"));

        /// <summary>The report bar's mark from (something goes wrong, nothing found): error, accent, else warning (as WPF).</summary>
        public static readonly IMultiValueConverter ReportMark = new FuncMultiValueConverter<bool, IBrush>(flags =>
        {
            var f = flags.ToList();
            return Brush(f.Count > 0 && f[0] ? "ErrorBrush" : f.Count > 1 && f[1] ? "AccentPrimaryBrush" : "WarningBrush");
        });

        /// <summary>A report section's key as its dot's colour (as WPF's report).</summary>
        public static readonly IValueConverter ReportSection = new FuncValueConverter<string?, IBrush>(key => Brush(key switch
        {
            "wrong" => "ErrorBrush",
            "ignored" => "WarningBrush",
            "missing" => "AccentPrimaryBrush",
            _ => "TextMutedBrush",
        }));

        /// <summary>A list row's kind ("Vanilla", "Changed", "New") as its colour.</summary>
        public static readonly IValueConverter Source = new FuncValueConverter<string?, IBrush>(kind => Brush(kind switch
        {
            "New" => "NewBrush",
            "Changed" => "ModifiedBrush",
            _ => "VanillaBrush",
        }));
    }
}
