﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace OperationTrident.Common.AI
{
    public class AIAgent : MonoBehaviour
    {
        public ScriptableObject AIFSMAsset = null;
        protected AIFSM FSM = new AIFSM();
        Transform _target;
        Vector3 _targetPosition;

        public virtual Vector3[] PatrolLocations { get; set; }
        public virtual NavMeshAgent PathfindingAgent { get; set; }
        public virtual AICamera Camera { get; set; }
        public virtual float CameraHorizontalFOV { get; set; }
        public virtual float CameraVerticalFOV { get; set; }
        public virtual float CameraSightDistance { get; set; }
        public virtual float AttackPrecisionAngle { get; set; }
        public virtual float AttackPrecisionRadius { get; set; }

        AIFSMData FSMData
        {
            get
            {
                if (AIFSMAsset != null)
                    return AIFSMAsset as AIFSMData;
                Debug.Log("没有设置AIFSM");
                throw new System.NotImplementedException();
            }
        }

        public Transform Target
        {
            get
            {
                return _target;
            }

            set
            {
                _target = value;
            }
        }

        public Vector3 TargetPosition
        {
            get
            {
                return _targetPosition;
            }

            set
            {
                _targetPosition = value;
            }
        }

        // Use this for initialization
        protected void Start()
        {
            FSM.FSMData = FSMData;
            FSM.Init(this.gameObject);
        }

        // Update is called once per frame
        protected void Update()
        {
            FSM.Update();
        }
    }
}
