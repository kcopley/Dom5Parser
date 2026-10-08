using System.Text.Json;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Tests
{
    /// <summary>
    /// Dom5Tests refaudit [HINTS.json]: which commands point at another entity (a unit, weapon,
    /// spell, site, nation, enchantment, code, ...) by their argument as the manuals give it
    /// (Data/command_hints.json), and whether the core reads them as a reference (an object it
    /// points at, which follows a renumbering) or as a plain number (which a merge would leave
    /// pointing at the old number). docs/MERGING.md, step 1.
    /// </summary>
    internal static class RefAudit
    {
        // words in a manual's argument that name what the number is
        private static readonly (string Word, string Kind)[] Kinds =
        {
            ("monster", "monster"), ("unit", "monster"), ("commander", "monster"), ("shape", "monster"),
            ("weapon", "weapon"), ("armor", "armor"), ("spell", "spell"), ("item", "item"),
            ("site", "site"), ("nation", "nation"), ("ench", "enchantment"), ("event", "event"),
            ("code", "code"), ("montag", "montag"), ("monster tag", "montag"), ("nametype", "nametype"),
            ("name type", "nametype"), ("poptype", "poptype"), ("pop type", "poptype"), ("variable", "variable"),
            ("merc", "mercenary"), ("bless", "bless"),
        };

        public static void Run(string basePath, string[] args)
        {
            var hintsPath = args.Length > 1 ? args[1] : Path.Combine(basePath, "Dom5Editor.Core", "Data", "command_hints.json");
            var hints = JsonDocument.Parse(File.ReadAllText(hintsPath)).RootElement.GetProperty("types");
            var mod = new Mod();
            int plain = 0, refs = 0;
            var rows = new List<string>();
            foreach (EntityType type in Enum.GetValues<EntityType>())
            {
                Type? cls;
                try { cls = mod.TypeOf(type); }
                catch (Exception) { continue; }
                if (cls == null || !typeof(IDEntity).IsAssignableFrom(cls) || cls.IsAbstract)
                    continue;
                Dictionary<Command, Func<Property>> map;
                try { map = ((IDEntity)Activator.CreateInstance(cls)!).GetPropertyMap(); }
                catch (Exception) { continue; }
                var typeName = type.ToString().ToLowerInvariant() switch { "mercenary" => "merc", var t => t };
                if (!hints.TryGetProperty(typeName, out var typeHints))
                    continue;
                foreach (var (command, create) in map)
                {
                    if (!CommandsMap.TryGetString(command, out var text))
                        continue;
                    var name = text.TrimStart('#');
                    if (!typeHints.TryGetProperty(name, out var hint) || !hint.TryGetProperty("args", out var argEl))
                        continue;
                    var argText = argEl.GetString() ?? "";
                    var lower = argText.ToLowerInvariant();
                    var kind = Kinds.FirstOrDefault(k => lower.Contains(k.Word)).Kind;
                    if (kind == null)
                        continue;
                    Property p;
                    try { p = create(); }
                    catch (Exception) { continue; }
                    bool isRef = p is Reference;
                    if (isRef) refs++; else plain++;
                    rows.Add($"{(isRef ? "ref  " : "PLAIN")} {typeName,-9} {text,-24} {kind,-11} {p.GetType().Name,-22} {argText}");
                }
            }
            foreach (var r in rows.OrderBy(r => r))
                Console.WriteLine(r);
            Console.WriteLine($"\n{refs} commands read as references, {plain} as plain values though their argument names another entity");
        }
    }
}
