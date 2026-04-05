
namespace DBH.SaveSystem.writer {
    public interface IVersionUpdate {
        dto.SemanticVersion VersionToUpdate { get; }

        void UpdateCustomTypes(SaveGameUpdateBuilder builder);
    }
}