using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dom5Edit.Props;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Represents a CUSTOMMAGIC entry for display and editing.
    /// Bitmask values: F=128, A=256, W=512, E=1024, S=2048, D=4096, N=8192, G=16384, B=32768, H=65536
    /// </summary>
    public class CustomMagicItem : INotifyPropertyChanged
    {
        // Path bitmask constants (from game data)
        public const ulong FIRE_MASK = 128;      // 1 << 7
        public const ulong AIR_MASK = 256;       // 1 << 8
        public const ulong WATER_MASK = 512;     // 1 << 9
        public const ulong EARTH_MASK = 1024;    // 1 << 10
        public const ulong ASTRAL_MASK = 2048;   // 1 << 11
        public const ulong DEATH_MASK = 4096;    // 1 << 12
        public const ulong NATURE_MASK = 8192;   // 1 << 13
        public const ulong GLAMOUR_MASK = 16384; // 1 << 14
        public const ulong BLOOD_MASK = 32768;   // 1 << 15
        public const ulong HOLY_MASK = 65536;    // 1 << 16

        private ulong _bitmask;
        private int _chance;
        private bool _isInherited;
        private bool _isModified;
        private bool _isSessionEdit;

        public ulong Bitmask
        {
            get => _bitmask;
            set
            {
                if (_bitmask != value)
                {
                    _bitmask = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PathsDisplay));
                    OnPropertyChanged(nameof(Display));
                    NotifyPathsChanged();
                }
            }
        }

        public int Chance
        {
            get => _chance;
            set
            {
                if (_chance != value)
                {
                    _chance = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Display));
                    OnPropertyChanged(nameof(Levels));
                    OnPropertyChanged(nameof(RandomChance));
                }
            }
        }

        public BitmaskChanceProperty Property { get; set; }

        public bool IsInherited
        {
            get => _isInherited;
            set { _isInherited = value; OnPropertyChanged(); }
        }

        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(); }
        }

        public bool IsSessionEdit
        {
            get => _isSessionEdit;
            set { _isSessionEdit = value; OnPropertyChanged(); }
        }

        // Path helpers for UI binding - using correct bitmask values
        public bool HasFire
        {
            get => (Bitmask & FIRE_MASK) != 0;
            set => SetPath(FIRE_MASK, value);
        }

        public bool HasAir
        {
            get => (Bitmask & AIR_MASK) != 0;
            set => SetPath(AIR_MASK, value);
        }

        public bool HasWater
        {
            get => (Bitmask & WATER_MASK) != 0;
            set => SetPath(WATER_MASK, value);
        }

        public bool HasEarth
        {
            get => (Bitmask & EARTH_MASK) != 0;
            set => SetPath(EARTH_MASK, value);
        }

        public bool HasAstral
        {
            get => (Bitmask & ASTRAL_MASK) != 0;
            set => SetPath(ASTRAL_MASK, value);
        }

        public bool HasDeath
        {
            get => (Bitmask & DEATH_MASK) != 0;
            set => SetPath(DEATH_MASK, value);
        }

        public bool HasNature
        {
            get => (Bitmask & NATURE_MASK) != 0;
            set => SetPath(NATURE_MASK, value);
        }

        public bool HasGlamour
        {
            get => (Bitmask & GLAMOUR_MASK) != 0;
            set => SetPath(GLAMOUR_MASK, value);
        }

        public bool HasBlood
        {
            get => (Bitmask & BLOOD_MASK) != 0;
            set => SetPath(BLOOD_MASK, value);
        }

        public bool HasHoly
        {
            get => (Bitmask & HOLY_MASK) != 0;
            set => SetPath(HOLY_MASK, value);
        }

        private void SetPath(ulong mask, bool enabled)
        {
            var newBitmask = enabled ? (Bitmask | mask) : (Bitmask & ~mask);
            if (newBitmask != Bitmask)
            {
                Bitmask = newBitmask;
            }
        }

        private void NotifyPathsChanged()
        {
            OnPropertyChanged(nameof(HasFire));
            OnPropertyChanged(nameof(HasAir));
            OnPropertyChanged(nameof(HasWater));
            OnPropertyChanged(nameof(HasEarth));
            OnPropertyChanged(nameof(HasAstral));
            OnPropertyChanged(nameof(HasDeath));
            OnPropertyChanged(nameof(HasNature));
            OnPropertyChanged(nameof(HasGlamour));
            OnPropertyChanged(nameof(HasBlood));
            OnPropertyChanged(nameof(HasHoly));
        }

        public string PathsDisplay
        {
            get
            {
                var paths = new List<string>();
                if (HasFire) paths.Add("F");
                if (HasAir) paths.Add("A");
                if (HasWater) paths.Add("W");
                if (HasEarth) paths.Add("E");
                if (HasAstral) paths.Add("S");
                if (HasDeath) paths.Add("D");
                if (HasNature) paths.Add("N");
                if (HasGlamour) paths.Add("G");
                if (HasBlood) paths.Add("B");
                if (HasHoly) paths.Add("H");
                return paths.Count > 0 ? string.Join("", paths) : "(none)";
            }
        }

        public string Display => $"{PathsDisplay} @ {Chance}%";

        /// <summary>
        /// Returns the number of magic levels this entry provides.
        /// Chance values: 100 = 1 level, 200 = 2 levels, etc.
        /// </summary>
        public int Levels => Chance / 100;

        /// <summary>
        /// Returns the actual random chance percentage (Chance mod 100).
        /// Example: Chance=150 means 50% chance for an additional level.
        /// </summary>
        public int RandomChance => Chance % 100;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
