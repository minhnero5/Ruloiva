using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace JellyQuestUnity
{
        [Serializable]
        public sealed class ObjectiveProgressState
        {
            public ObjectiveDefinition definition;
            public int currentCount;

            public bool IsComplete => currentCount >= definition.targetCount;
        }
}
