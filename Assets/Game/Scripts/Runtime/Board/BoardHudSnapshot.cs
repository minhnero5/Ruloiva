using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
        public sealed class BoardHudObjectiveSnapshot
        {
            public ObjectiveType type;
            public int currentCount;
            public int targetCount;
            public bool isComplete;
        }

        public sealed class BoardHudSnapshot
        {
            public int score;
            public int coins;
            public int movesRemaining;
            public int timeRemaining;
            public int stars;
            public BoardEndState endState;
            public BoardPendingEndReason pendingEndReason;
            public BoardHudObjectiveSnapshot[] objectives;
        }
}
