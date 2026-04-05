using System;
using System.Collections.Generic;

namespace DBH.SaveSystem.dto {
    [Serializable]
    public class SemVer : IComparable<SemVer> {
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public string PreRelease { get; }
        public string BuildMetadata { get; }

        public SemVer(string version) {
            Major = 0;
            Minor = 0;
            Patch = 0;
            PreRelease = string.Empty;
            BuildMetadata = string.Empty;

            if (string.IsNullOrEmpty(version)) {
                return;
            }

            // Simple parsing to avoid heavy Regex dependencies if possible,
            // but for full SemVer support, we need to handle -, + and .
            
            var buildSplit = version.Split('+');
            if (buildSplit.Length > 1) {
                BuildMetadata = buildSplit[1];
            }

            var baseAndPre = buildSplit[0];
            var preSplit = baseAndPre.Split('-');
            if (preSplit.Length > 1) {
                PreRelease = preSplit[1];
            }

            var mainVersion = preSplit[0];
            var parts = mainVersion.Split('.');
            
            if (parts.Length > 0 && int.TryParse(parts[0], out var major)) {
                Major = major;
            }

            if (parts.Length > 1 && int.TryParse(parts[1], out var minor)) {
                Minor = minor;
            }

            if (parts.Length > 2 && int.TryParse(parts[2], out var patch)) {
                Patch = patch;
            }
        }

        public SemVer(int major, int minor, int patch, string preRelease = "", string buildMetadata = "") {
            Major = major;
            Minor = minor;
            Patch = patch;
            PreRelease = preRelease ?? string.Empty;
            BuildMetadata = buildMetadata ?? string.Empty;
        }

        public override string ToString() {
            var version = $"{Major}.{Minor}.{Patch}";
            if (!string.IsNullOrEmpty(PreRelease)) {
                version += $"-{PreRelease}";
            }
            if (!string.IsNullOrEmpty(BuildMetadata)) {
                version += $"+{BuildMetadata}";
            }
            return version;
        }

        public int CompareTo(SemVer other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            
            var majorComparison = Major.CompareTo(other.Major);
            if (majorComparison != 0) return majorComparison;
            
            var minorComparison = Minor.CompareTo(other.Minor);
            if (minorComparison != 0) return minorComparison;
            
            var patchComparison = Patch.CompareTo(other.Patch);
            if (patchComparison != 0) return patchComparison;

            // SemVer rule: Pre-release versions have lower precedence than the associated normal version.
            // "1.2.3-alpha" < "1.2.3"
            if (string.IsNullOrEmpty(PreRelease) && !string.IsNullOrEmpty(other.PreRelease)) return 1;
            if (!string.IsNullOrEmpty(PreRelease) && string.IsNullOrEmpty(other.PreRelease)) return -1;
            
            // If both have pre-release, compare them (simplified string comparison for now)
            return string.Compare(PreRelease, other.PreRelease, StringComparison.Ordinal);
        }

        protected bool Equals(SemVer other) {
            return Major == other.Major && 
                   Minor == other.Minor && 
                   Patch == other.Patch && 
                   PreRelease == other.PreRelease && 
                   BuildMetadata == other.BuildMetadata;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SemVer)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Major;
                hashCode = (hashCode * 397) ^ Minor;
                hashCode = (hashCode * 397) ^ Patch;
                hashCode = (hashCode * 397) ^ (PreRelease != null ? PreRelease.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BuildMetadata != null ? BuildMetadata.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(SemVer left, SemVer right) {
            return Equals(left, right);
        }

        public static bool operator !=(SemVer left, SemVer right) {
            return !Equals(left, right);
        }

        public static bool operator <(SemVer left, SemVer right) {
            return Comparer<SemVer>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(SemVer left, SemVer right) {
            return Comparer<SemVer>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(SemVer left, SemVer right) {
            return Comparer<SemVer>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(SemVer left, SemVer right) {
            return Comparer<SemVer>.Default.Compare(left, right) >= 0;
        }
    }
}