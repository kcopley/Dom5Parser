using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Dom5Edit.Validation;

namespace Dom5Editor.Ava.Views
{
    public static class ReportFiles
    {
        /// <summary>Saves a report as Markdown where the user picks (named after the mod).</summary>
        public static async Task SaveForAuthor(Window owner, ModReport.Report report)
        {
            var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save the report for the mod's author",
                DefaultExtension = "md",
                SuggestedFileName = string.Concat((report.Name + " - editor report").Split(Path.GetInvalidFileNameChars())) + ".md",
                FileTypeChoices = Hooks.Filters("Markdown (*.md)|*.md|Text files (*.txt)|*.txt"),
            });
            if (file?.TryGetLocalPath() is not string path)
                return;
            try
            {
                await File.WriteAllTextAsync(path, ModReport.Markdown(report));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                await Dialogs.Tell(owner, "Save report", "The report wasn't saved:\n\n" + ex.Message);
            }
        }
    }
}
