
namespace DBH.SaveSystem.writer {
    public interface IVersionUpdate {
        dto.SemVer VersionToUpdate { get; }

        void UpdateCustomTypes(SaveGameUpdateBuilder builder);
    }
}