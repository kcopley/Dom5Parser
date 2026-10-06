using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit
{
    /// <summary>
    /// Handles exporting mod data to .dm file format.
    /// </summary>
    public class ModExporter
    {
        /// <summary>
        /// Exports a mod to a file.
        /// </summary>
        /// <param name="mod">The mod to export.</param>
        /// <param name="filePath">The file path to write to.</param>
        /// <param name="overwrite">Whether to overwrite an existing file.</param>
        public void Export(Mod mod, string filePath, bool overwrite = true)
        {
            if (File.Exists(filePath) && !overwrite)
            {
                return;
            }

            // temp file, then swap; the previous file is kept as .bak
            SafeFile.Write(filePath, writer => Export(mod, writer));
        }

        /// <summary>
        /// Exports a mod to a stream.
        /// </summary>
        /// <param name="mod">The mod to export.</param>
        /// <param name="writer">The stream writer to write to.</param>
        public void Export(Mod mod, StreamWriter writer)
        {
            // in the parsed file's block order (docs/SAVE_FLOW.md): the default for a mod read from
            // a file; not for one built in the editor, or with disabled entities (merge)
            var plan = new SavePlan(mod);
            if (plan.InSourceOrder)
            {
                WriteInSourceOrder(mod, plan, writer);
                return;
            }
            WriteHeader(mod, writer);
            writer.WriteLine();
            WriteEntities(mod, writer);
        }

        /// <summary>
        /// Writes the mod header (name, version, description, etc.).
        /// </summary>
        protected virtual void WriteHeader(Mod mod, StreamWriter writer)
        {
            writer.WriteLine(CommandsMap.Format(Command.MODNAME, mod.ModName, true));

            if (!string.IsNullOrEmpty(mod.Version))
                writer.WriteLine(CommandsMap.Format(Command.VERSION, mod.Version));

            if (!string.IsNullOrEmpty(mod.DomVersion))
                writer.WriteLine(CommandsMap.Format(Command.DOMVERSION, mod.DomVersion));

            if (!string.IsNullOrEmpty(mod.Icon))
                writer.WriteLine(CommandsMap.Format(Command.ICON, mod.Icon, true));

            if (!string.IsNullOrEmpty(mod.Description))
                writer.WriteLine(CommandsMap.Format(Command.DESCRIPTION, mod.Description, true));
        }

        /// <summary>
        /// Writes all entities from the mod's database.
        /// </summary>
        protected virtual void WriteEntities(Mod mod, StreamWriter writer)
        {
            foreach (var kvp in mod.Database)
            {
                kvp.Value.Export(writer);
            }
        }

        /// <summary>
        /// Writes the parsed file's blocks in file order, with the session's edits applied, then
        /// entities that have no block. Unedited lines are written as read. See docs/SAVE_FLOW.md
        /// for the rules and why.
        /// </summary>
        private void WriteInSourceOrder(Mod mod, SavePlan plan, StreamWriter writer)
        {
            // a property's line: as read if unedited (unless the mod asks for regenerated text)
            string Text(Property p) => mod.KeepOriginalText ? p.SaveText() : p.ToExportString();
            bool keep = mod.KeepOriginalText;

            // preamble: as read while the header fields are unchanged, else a regenerated header
            // and the preamble's other lines
            if (keep && mod.HeaderUnchanged)
            {
                foreach (var (text, _) in mod.Preamble)
                    writer.WriteLine(text);
            }
            else
            {
                WriteHeader(mod, writer);
                foreach (var (text, isHeader) in mod.Preamble)
                    if (!isHeader)
                        writer.WriteLine(text);
            }

            foreach (var block in mod.SourceBlocks)
            {
                var entity = block.Entity;
                if (!plan.Holds(entity))
                    continue; // deleted in the session (its comments go with it)
                foreach (var text in block.LeadingTrivia)
                    writer.WriteLine(text);
                writer.WriteLine(keep && block.RawHeader != null && entity.ID == block.IdAtParse ? block.RawHeader : Header(block));
                foreach (var p in plan.AddedAtStart(block))
                {
                    writer.WriteLine(Text(p));
                    foreach (var f in plan.Followers(p))
                        writer.WriteLine(Text(f));
                }
                int t = 0;
                for (int i = 0; i < block.Properties.Count; i++)
                {
                    for (; t < block.Trivia.Count && block.Trivia[t].Before <= i; t++)
                        writer.WriteLine(block.Trivia[t].Text);
                    var p = block.Properties[i];
                    // live now: write it; live after parse but not now: removed by an edit;
                    // not live after parse: a later clear or copy in the file took it out,
                    // and the game still reads it here
                    if (plan.Writes(block, p))
                        writer.WriteLine(Text(p));
                    foreach (var r in plan.Followers(p))
                        writer.WriteLine(Text(r));
                }
                for (; t < block.Trivia.Count; t++)
                    writer.WriteLine(block.Trivia[t].Text);
                foreach (var p in plan.AddedAtEnd(block))
                {
                    writer.WriteLine(Text(p));
                    foreach (var f in plan.Followers(p))
                        writer.WriteLine(Text(f));
                }
                if (keep && block.RawEnd != null)
                    writer.WriteLine(block.RawEnd);
                else if (CommandsMap.TryGetString(Command.END, out var end))
                    writer.WriteLine(end);
                // entities made in the editor to come right after this one (an event's delayed follow-up)
                foreach (var placed in plan.PlacedAfterBlock(block))
                {
                    writer.WriteLine();
                    placed.Export(writer);
                }
            }
            foreach (var text in mod.TrailingTrivia)
                writer.WriteLine(text);

            // entities with no block: new in the session, or vanilla entities first edited in it
            bool first = true;
            var inline = new HashSet<IDEntity>(mod.SourceBlocks.SelectMany(plan.PlacedAfterBlock), ReferenceEqualityComparer.Instance);
            foreach (var set in mod.Database.Values)
                set.Export(writer, e =>
                {
                    if (plan.HasBlocks(e) || inline.Contains(e))
                        return false;
                    if (first)
                        writer.WriteLine();
                    first = false;
                    return true;
                });
        }

        /// <summary>
        /// The lines a save writes for one entity (all its blocks, in order: header, lines, #end), as
        /// they'd be in the file; for showing in the editor. Lines without a command (comments) are
        /// left out.
        /// </summary>
        public static IReadOnlyList<string> EntityLines(Mod mod, IDEntity entity)
        {
            var plan = new SavePlan(mod);
            var lines = new List<string>();
            if (!plan.Holds(entity))
                return lines;
            string Text(Property p) => mod.KeepOriginalText ? p.SaveText() : p.ToExportString();
            foreach (var (_, block) in plan.BlocksOf(entity))
            {
                if (lines.Count > 0)
                    lines.Add("");
                if (block.Source != null)
                    lines.Add(mod.KeepOriginalText && block.Source.RawHeader != null && entity.ID == block.Source.IdAtParse ? block.Source.RawHeader : Header(block.Source));
                else
                {
                    using var sw = new StringWriter();
                    var w = new StreamWriter(new MemoryStream());
                    CommandsMap.TryGetString(entity.Selected ? entity.GetSelectCommand() : entity.GetNewCommand(), out var command);
                    lines.Add(entity.Named ? $"{command} \"{entity.HeaderName}\"" : entity.ID > 0 ? $"{command} {entity.ID}" : command);
                }
                lines.AddRange(block.Lines.Select(Text));
                lines.Add(block.Source?.RawEnd != null && mod.KeepOriginalText ? block.Source.RawEnd : "#end");
            }
            return lines;
        }

        /// <summary>The block's #new.../#select... line as parsed (a numeric ID as the entity's current ID).</summary>
        private static string Header(SourceBlock block)
        {
            var entity = block.Entity;
            CommandsMap.TryGetString(block.Selected ? entity.GetSelectCommand() : entity.GetNewCommand(), out var command);
            string arg = block.Header;
            if (int.TryParse(arg, out _))
                arg = entity.ID != -1 ? entity.ID.ToString() : arg;
            else if (arg.Length > 0)
                arg = "\"" + arg + "\"";
            string line = arg.Length > 0 ? command + " " + arg : command;
            return block.HeaderComment.Length > 0 ? line + " -- " + block.HeaderComment : line;
        }

        /// <summary>
        /// Exports only entities associated with specific nations.
        /// </summary>
        /// <param name="mod">The mod to export from.</param>
        /// <param name="writer">The stream writer to write to.</param>
        /// <param name="nations">The nations to filter by.</param>
        /// <param name="inclusive">If true, includes entities used by any of the nations. If false, only entities exclusive to those nations.</param>
        public void ExportForNations(Mod mod, StreamWriter writer, HashSet<Nation> nations, bool inclusive = false)
        {
            WriteHeader(mod, writer);
            writer.WriteLine();

            Func<IDEntity, bool> filter = inclusive
                ? entity => (entity.AssociatedNations.IsSubsetOf(nations) && entity.AssociatedNations.Count > 0)
                         || entity.AssociatedNations.IsSupersetOf(nations)
                : entity => entity.AssociatedNations.IsSubsetOf(nations) && entity.AssociatedNations.Count > 0;

            foreach (var kvp in mod.Database)
            {
                foreach (var entity in kvp.Value.GetFullList().Where(filter))
                {
                    entity.Export(writer);
                    writer.WriteLine();
                }
            }
        }
    }
}
