﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace OperationTrident.Common.AI
{
    [DisallowMultipleComponent]
    public class PatrolState : AIState
    {
        public static readonly string STATE_NAME = "Patrol";

        public class Conditions
        {
            public static readonly string ARRIVE_AT_LOCATION = "Arrive at location";
            public static readonly string SIGHT_PLAYER = "Sight Player";
        }

        // 巡逻点列表，AI会在这些地点中循环移动
        Vector3[] _patrolLocations;
        // 记录下一个巡逻点
        int _nextPatrolLocationIndex = 0;
        // 到下一个巡逻点的距离
        float _remainDistance = Mathf.Infinity;

        public override void Init()
        {
            if (IsFirstInit)
            {
                _patrolLocations = _agent.PatrolLocations;
                if (_patrolLocations == null)
                    Debug.Log("没有设置巡逻路径");
                _nextPatrolLocationIndex = _agent.PatrolStartLocationIndex;
            }
            _agent.PathfindingAgent.SetDestination(_patrolLocations[_nextPatrolLocationIndex]);
            _agent.PathfindingAgent.isStopped = false;
            _agent.ActionController.RPC(_agent.ActionController.Move, true);
        }

        public override string Execute()
        {
            // 当AI不在Link上时才检测玩家，不然会在半空中发现玩家改变状态
            if (!_agent.PathfindingAgent.isOnOffMeshLink)
            {
                Transform target = Utility.DetectAllPlayersWithCamera(_agent.Camera);
                if (target != null)
                {
                    _agent.Target = target;
                    return Conditions.SIGHT_PLAYER;
                }
            }

            if (!_agent.PathfindingAgent.pathPending)
            {
                _remainDistance = _agent.PathfindingAgent.remainingDistance;
                if (_remainDistance != Mathf.Infinity && _remainDistance - _agent.PathfindingAgent.stoppingDistance <= float.Epsilon
                && _agent.PathfindingAgent.pathStatus == NavMeshPathStatus.PathComplete || _agent.PathfindingAgent.velocity == Vector3.zero)
                {
                    // 当到达一个巡逻点时，设置下一个巡逻点，并返回已满足ARRIVE_AT_LOCATION条件
                    _nextPatrolLocationIndex = (_nextPatrolLocationIndex + 1) % _patrolLocations.Length;
                    return Conditions.ARRIVE_AT_LOCATION;
                }
            }

            return null;
        }

        public override void Exit()
        {
            _agent.PathfindingAgent.isStopped = true;
            _agent.ActionController.RPC(_agent.ActionController.Move, false);
        }
    }
}
