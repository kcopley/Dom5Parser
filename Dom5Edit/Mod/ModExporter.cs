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
            if (UsesSourceOrder(mod))
            {
                WriteInSourceOrder(mod, writer);
                return;
            }
            WriteHeader(mod, writer);
            writer.WriteLine();
            WriteEntities(mod, writer);
        }

        /// <summary>
        /// Whether to save in the parsed file's block order (docs/SAVE_FLOW.md): the default for a
        /// mod read from a file. Not for one built in the editor, or with disabled entities (merge).
        /// </summary>
        private static bool UsesSourceOrder(Mod mod) =>
            mod.PreserveSourceOrder && mod.SourceBlocks.Count > 0 && !mod.Database.Values.Any(s => s.HasDisabled);

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
        private void WriteInSourceOrder(Mod mod, StreamWriter writer)
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

            // (by reference: CommandProperty compares by value)
            var held = new HashSet<IDEntity>(mod.Database.Values.SelectMany(s => s.GetFullList()), ReferenceEqualityComparer.Instance);
            var blocksOf = new Dictionary<IDEntity, List<SourceBlock>>(ReferenceEqualityComparer.Instance);
            foreach (var block in mod.SourceBlocks)
            {
                if (!blocksOf.TryGetValue(block.Entity, out var list))
                    blocksOf[block.Entity] = list = new List<SourceBlock>();
                list.Add(block);
            }

            // Live properties no block has (added in the session): a replacement goes where the
            // removed property of the same command was; anything else at the end of a block.
            var live = new Dictionary<IDEntity, HashSet<Property>>(ReferenceEqualityComparer.Instance);
            var inSlot = new Dictionary<Property, List<Property>>(ReferenceEqualityComparer.Instance);
            var atEnd = new Dictionary<SourceBlock, List<Property>>(ReferenceEqualityComparer.Instance);
            foreach (var (entity, blocks) in blocksOf)
            {
                if (!held.Contains(entity))
                    continue;
                var liveNow = live[entity] = new HashSet<Property>(entity.Properties, ReferenceEqualityComparer.Instance);
                var inBlocks = new HashSet<Property>(blocks.SelectMany(b => b.Properties), ReferenceEqualityComparer.Instance);
                var removed = blocks.SelectMany(b => b.Properties)
                    .Where(p => mod.PropertiesAfterParse.Contains(p) && !liveNow.Contains(p)).ToList();
                foreach (var p in entity.Properties.Where(p => !inBlocks.Contains(p)))
                {
                    var gap = removed.FirstOrDefault(r => r.Command == p.Command);
                    if (gap != null)
                        Add(inSlot, gap, p);
                    else
                        Add(atEnd, PlacementBlock(entity, blocks, p), p);
                }
            }

            foreach (var block in mod.SourceBlocks)
            {
                var entity = block.Entity;
                if (!held.Contains(entity))
                    continue; // deleted in the session (its comments go with it)
                foreach (var text in block.LeadingTrivia)
                    writer.WriteLine(text);
                writer.WriteLine(keep && block.RawHeader != null && entity.ID == block.IdAtParse ? block.RawHeader : Header(block));
                int t = 0;
                for (int i = 0; i < block.Properties.Count; i++)
                {
                    for (; t < block.Trivia.Count && block.Trivia[t].Before <= i; t++)
                        writer.WriteLine(block.Trivia[t].Text);
                    var p = block.Properties[i];
                    // live now: write it; live after parse but not now: removed by an edit;
                    // not live after parse: a later clear or copy in the file took it out,
                    // and the game still reads it here
                    if (live[entity].Contains(p) || !mod.PropertiesAfterParse.Contains(p))
                        writer.WriteLine(Text(p));
                    if (inSlot.TryGetValue(p, out var replacements))
                        foreach (var r in replacements)
                            writer.WriteLine(Text(r));
                }
                for (; t < block.Trivia.Count; t++)
                    writer.WriteLine(block.Trivia[t].Text);
                if (atEnd.TryGetValue(block, out var added))
                    foreach (var p in added)
                        writer.WriteLine(Text(p));
                if (keep && block.RawEnd != null)
                    writer.WriteLine(block.RawEnd);
                else if (CommandsMap.TryGetString(Command.END, out var end))
                    writer.WriteLine(end);
            }
            foreach (var text in mod.TrailingTrivia)
                writer.WriteLine(text);

            // entities with no block: new in the session, or vanilla entities first edited in it
            bool first = true;
            foreach (var set in mod.Database.Values)
                set.Export(writer, e =>
                {
                    if (blocksOf.ContainsKey(e))
                        return false;
                    if (first)
                        writer.WriteLine();
                    first = false;
                    return true;
                });
        }

        /// <summary>
        /// The block an added property goes at the end of: the entity's first block, so copies
        /// made after it carry the value (rule C), unless a later block of the entity sets the same
        /// command, clears its group or copies over it; then the last block, so it takes effect.
        /// </summary>
        private static SourceBlock PlacementBlock(IDEntity entity, List<SourceBlock> blocks, Property p)
        {
            var group = entity.GetPropertyGroup(p.Command);
            bool overriddenLater = blocks.Skip(1).SelectMany(b => b.Properties).Any(q =>
                q.Command == p.Command
                || PropertyGroupMap.GetGroupClearedBy(q.Command) is PropertyGroup cleared
                    && (cleared == PropertyGroup.All || cleared == group)
                || PropertyGroupMap.IsFullCopyCommand(q.Command)
                    && PropertyGroupMap.GetGroupsOverwrittenByCopy(q.Command) is var copied
                    && (copied.Contains(PropertyGroup.All) || copied.Contains(group)));
            return overriddenLater ? blocks[^1] : blocks[0];
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

        private static void Add<TKey>(Dictionary<TKey, List<Property>> map, TKey key, Property p) where TKey : notnull
        {
            if (!map.TryGetValue(key, out var list))
                map[key] = list = new List<Property>();
            list.Add(p);
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
