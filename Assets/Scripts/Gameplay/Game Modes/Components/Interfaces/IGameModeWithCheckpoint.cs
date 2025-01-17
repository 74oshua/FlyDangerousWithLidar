﻿namespace Gameplay.Game_Modes.Components.Interfaces {
    public interface IGameModeWithCheckpoints {
        public IGameModeCheckpoints GameModeCheckpoints { set; }
        public void OnCheckpointHit(Checkpoint checkpoint, float hitTimeSeconds);
        public void OnLastCheckpointHit(float hitTimeSeconds);
    }
}