using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
        public enum BoardPendingEndReason
        {
            None = 0,
            ObjectivesCompleted = 1,
            NoMovesRemaining = 2,
            TimeExpired = 3
        }
}
