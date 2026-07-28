using System.Collections.Generic;

namespace DBH.SaveSystem.dto {
    public interface IDisplaySaveStats {
        List<CustomStatsDto> DisplayStats { get; }
        int Order => 0;
    }
}