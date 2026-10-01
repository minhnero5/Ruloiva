using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
        public sealed class BoardVisualPhase
        {
            public BoardVisualPhaseKind kind;
            public BoardVisualSnapshot snapshot;
            public BoardHudSnapshot hud;
            public float suggestedDuration;
            public readonly List<BoardScorePopupInfo> scorePopups = new List<BoardScorePopupInfo>();
        }
}
