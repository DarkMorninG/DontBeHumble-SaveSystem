using DBH.SaveSystem.dto;

namespace DBH.SaveSystem.writer {
    public interface IVersionUpdate {
        SemanticVersion VersionToUpdate { get; }

        void UpdateCustomTypes(SaveGameUpdateBuilder builder);
    }
}