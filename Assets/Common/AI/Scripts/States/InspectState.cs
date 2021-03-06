﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OperationTrident.Common.AI
{
    [DisallowMultipleComponent]
    public class InspectState : AIState
    {
        public static readonly string STATE_NAME = "Inspect";

        public class Conditions
        {
            public static readonly string SIGHT_PLAYER = "Sight Player";
            public static readonly string FINISH_INSPECTION = "Finish Inspection";
        }

        float _duration;

        public override void Init()
        {
            _agent.ActionController.RPC(_agent.ActionController.FindTarget, true);
            _duration = 8f;
        }

        public override string Execute()
        {
            _agent.Target = Utility.DetectAllPlayersWithCamera(_agent.Camera);
            if (_agent.Target != null)
            {
                return Conditions.SIGHT_PLAYER;
            }

            _duration -= (_agent.fsmUpdateTime + Time.deltaTime);
            if (_duration < 0)
            {
                return Conditions.FINISH_INSPECTION;
            }

            return null;
        }

        public override void Exit()
        {
            _agent.ActionController.RPC(_agent.ActionController.FindTarget, false);
        }
    }
}
