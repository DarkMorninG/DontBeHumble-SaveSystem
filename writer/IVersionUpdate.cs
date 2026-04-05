
namespace DBH.SaveSystem.writer {
    public interface IVersionUpdate {
        SemVer VersionToUpdate { get; }

        void UpdateCustomTypes(SaveGameUpdateBuilder builder);
    }
}