using System;
using UnityEngine;

namespace DBH.SaveSystem.dto {
    [Serializable]
    public class CustomStatsDto {
        public CustomStatsDto(string statName, string statValue) {
            this.statName = statName;
            this.statValue = statValue;
        }

        [SerializeField]
        private string statName;

        [SerializeField]
        private string statValue;

        public string StatName => statName;

        public string StatValue => statValue;
    }
}