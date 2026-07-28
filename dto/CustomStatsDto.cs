using System;
using UnityEngine;

namespace DBH.SaveSystem.dto {
    [Serializable]
    public class CustomStatsDto {
        [SerializeField]
        private string statName;

        [SerializeField]
        private string statValue;

        public string StatName => statName;

        public string StatValue => statValue;
    }
}