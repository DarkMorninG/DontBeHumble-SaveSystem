using System.Collections.Generic;

namespace DBH.SaveSystem.dto {
    public interface ICustomStatData {
        List<CustomStatsDto> DisplayStats { get; }
        int Order => 0;
    }
}