using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
        public sealed class BoardActionReport
        {
            public bool valid;
            public bool boardChanged;
            public bool shuffled;
            public int destroyedCount;
            public int scoreGained;
            public int coinsGained;
            public int stars;
            public BoardEndState endState;
            public BoardPendingEndReason pendingEndReason;
            public bool settleGuardTriggered;
            public string settleGuardMessage;
            public readonly List<BoardVisualPhase> phases = new List<BoardVisualPhase>();
        }
}
