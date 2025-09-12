using System;
using DBH.Attributes;
using Application = UnityEngine.Application;

namespace DBH.SaveSystem.Beans {
    [Bean]
    public class GameplayTimer {

        private DateTime _gamePlayStart;

        public void StartGameplayTimer() {
            if (Application.isPlaying) {
                _gamePlayStart = DateTime.Now;
            }
        }

        public int SecondsPlayedSinceStart() {
            var timePlayedTillNow = (DateTime.Now - _gamePlayStart).Seconds;
            _gamePlayStart = DateTime.Now;
            return timePlayedTillNow;
        }

    }
}