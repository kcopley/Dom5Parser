using Dom5Edit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace Dom5Editor
{
    public class MergerMenuVM
    {
        private ModViewModel _modVM;

        private ModSet _mods = null;
        private ModSet Imported { get; }
        private bool Log { get; set; }

        private string folderPath;
        private string _modName = "merged-mod";

        public CheckBox Logging;
        public CheckBox ToggleEA;
        public CheckBox ToggleMA;
        public CheckBox ToggleLA;
        public CheckBox ToggleAllMods;
        
        public List<CheckBox> Mods { get; set; } = new List<CheckBox>();

        List<string> _eaNations = new List<string>()
        {
            "EA Arcosephale",
            "EA Ermor",
            "EA Ulm",
            "EA Marverni",
            "EA Sauromatia",
            "EA T'ien Ch'i",
            "EA Machaka",
            "EA Mictlan",
            "EA Abysia",
            "EA Caelum",
            "EA C'tis",
            "EA Pangaea",
            "EA Agartha",
            "EA Tir na n'Og",
            "EA Fomoria",
            "EA Vanheim",
            "EA Helheim",
            "EA Niefelheim",
            "EA Rus",
            "EA Kailasa",
            "EA Lanka",
            "EA Yomi",
            "EA Hinnom",
            "EA Ur",
            "EA Berytos",
            "EA Xibalba",
            "EA Mekone",
            "EA Ubar",
            "EA Atlantis",
            "EA R'lyeh",
            "EA Pelagia",
            "EA Oceania",
            "EA Therodos"
        };
        List<string> _maNations = new List<string>()
        {
            "MA Arcosephale",
            "MA Ermor",
            "MA Sceleria",
            "MA Pythium",
            "MA Man",
            "MA Eriu",
            "MA Ulm",
            "MA Marignon",
            "MA Mictlan",
            "MA T'ien Ch'i",
            "MA Machaka",
            "MA Agartha",
            "MA Abysia",
            "MA Caelum",
            "MA C'tis",
            "MA Pangaea",
            "MA Asphodel",
            "MA Vanheim",
            "MA Jotunheim",
            "MA Vanarus",
            "MA Bandar Log",
            "MA Shinuyama",
            "MA Ashdod",
            "MA Uruk",
            "MA Nazca",
            "MA Xibalba",
            "MA Phlegra",
            "MA Phaecia",
            "MA Ind",
            "MA Na'Ba",
            "MA Atlantis",
            "MA R'lyeh",
            "MA Pelagia",
            "MA Oceania",
            "MA Ys"
        };
        List<string> _laNations = new List<string>()
        {
            "LA Arcosephale",
            "LA Pythium",
            "LA Lemuria",
            "LA Man",
            "LA Ulm",
            "LA Marignon",
            "LA Mictlan",
            "LA T'ien Ch'i",
            "LA Jomon",
            "LA Agartha",
            "LA Abysia",
            "LA Caelum",
            "LA C'tis",
            "LA Pangaea",
            "LA Midgard",
            "LA Utgard",
            "LA Bogarus",
            "LA Patala",
            "LA Gath",
            "LA Ragha",
            "LA Xibalba",
            "LA Phlegra",
            "LA Vaettiheim",
            "LA Atlantis",
            "LA R'lyeh",
            "LA Erytheia"
        };

        public List<CheckBox> EANations = new List<CheckBox>();
        public List<CheckBox> MANations = new List<CheckBox>();
        public List<CheckBox> LANations = new List<CheckBox>();

        public List<string> ImportedMods { get; set; } = new List<string>();

        public MergerMenuVM()
        {
            foreach (var e in _eaNations)
            {
                EANations.Add(new CheckBox() { Content = e });
            }
            foreach (var e in _maNations)
            {
                MANations.Add(new CheckBox() { Content = e });
            }
            foreach (var e in _laNations)
            {
                LANations.Add(new CheckBox() { Content = e });
            }
        }

        public void UpdateModName(string name)
        {
            _modName = name;
        }

        public string FolderPath
        {
            get
            {
                return folderPath;
            }
            set
            {
                folderPath = value;
                UpdateModList();
            }
        }

        public void SetModFolderPath()
        {
            var folder = GetDefaultFolderPath();
            FolderPath = folder;
        }

        public static string GetDefaultFolderPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Dominions5\EditorTesting");
        }

        public void UpdateModList()
        {
            if (!Directory.Exists(FolderPath)) return;
            string[] dmFiles = Directory.GetFiles(FolderPath, "*.dm");
            Mods.Clear();
            foreach (string s in dmFiles)
            {
                if (Path.GetFileName(s).StartsWith(_modName)) continue;
                Mods.Add(new CheckBox() { Content = Path.GetFileName(s) });
            }
        }

        public List<string> DisabledNations = new List<string>();
        public Mod MergeAndExport()
        {
            Log = Logging.IsChecked.GetValueOrDefault();

            List<string> files = new List<string>();
            foreach (var modFile in Mods)
            {
                if (modFile.IsChecked.GetValueOrDefault() || ToggleAllMods.IsChecked.GetValueOrDefault()) files.Add(modFile.Content.ToString());
            }
            var mods = ModSet.Import(FolderPath, files);

            DisabledNations.Clear();
            foreach (var c in EANations)
            {
                if (c.IsChecked.GetValueOrDefault() || ToggleEA.IsChecked.GetValueOrDefault())
                {
                    DisabledNations.Add(c.Content.ToString());
                }
            }
            foreach (var c in MANations)
            {
                if (c.IsChecked.GetValueOrDefault() || ToggleMA.IsChecked.GetValueOrDefault())
                {
                    DisabledNations.Add(c.Content.ToString());
                }
            }
            foreach (var c in LANations)
            {
                if (c.IsChecked.GetValueOrDefault() || ToggleLA.IsChecked.GetValueOrDefault())
                {
                    DisabledNations.Add(c.Content.ToString());
                }
            }
            foreach (var mod in mods)
            {
                mod.DisableMages(DisabledNations);
            }
            Mod final = mods.MergeAll("merged-mod");

            return final;
        }

        public void ToggleMods(bool on)
        {
            foreach (var c in this.Mods)
            {
                c.IsChecked = on;
            }
        }

        public void ToggleEANations(bool on)
        {
            foreach (var c in this.EANations)
            {
                c.IsChecked = on;
            }
        }

        public void ToggleMANations(bool on)
        {
            foreach (var c in this.MANations)
            {
                c.IsChecked = on;
            }
        }

        public void ToggleLANations(bool on)
        {
            foreach (var c in this.LANations)
            {
                c.IsChecked = on;
            }
        }
    }
}
