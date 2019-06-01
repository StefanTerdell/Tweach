using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "CameraSettings", menuName = "CameraSettings")]
public class CameraScriptSettings : ScriptableObject
{
    public TargetSettings target;
    public TimeSettings time;
    public OffsetAndFollowSettings offsetAndFollow;
    public ObstructionAvoidance obstruction;
    public TurnWithPlayerSettings turnWithPlayer;
    public FovSettings fov;
    public InputSettings input;

    [Serializable]
    public class TargetSettings
    {
        [Range(0, 1)]
        public float EnemyLookWeight = .5f;

        public float LookPointLerp = 10;
    }

    [Serializable]
    public class TimeSettings
    {
        public TimeSetting Alive;
        public TimeSetting Dead;

        [Serializable]
        public class TimeSetting
        {
            public UpdateTime updateTime;
            public bool unscaledTime;

            [Serializable]
            public enum UpdateTime
            {
                Update,
                LateUpdate,
                FixedUpdate
            }
        }
    }

    [Serializable]
    public class OffsetAndFollowSettings
    {
        public float FollowDistance = 3;
        public Vector3 TargetOffset = Vector3.up * 2;
        public float CombatIdleSwitchSpeed = 5;
        public bool ReadNumpadControls;
        public float VisceralAngle = 90;
        public XAngle xAngle;

        [Serializable]
        public class XAngle
        {
            public float Max = 70, Min = -70, Idle = 30, Combat = 15, WaitingTimeBeforeReturn = 2;
        }
    }

    [Serializable]
    public class ObstructionAvoidance
    {
        public Spherecast spherecast;
        public LayerMask obstructingLayers;
        [Range(1, 2)]
        public float power = 1.2f;
        [Range(0.1f, 5)]
        public float returnSpeed = 2;
        public bool returnOnPlayerInputOnly = true;

        public bool DrawDebugRays;

        [Serializable]
        public class Spherecast
        {
            [Range(.1f, 2)]
            public float radius = .75f;
            [Range(1, 100)]
            public float granularity = 50;
        }
    }

    [Serializable]
    public class TurnWithPlayerSettings
    {
        public float ForwardDeadZone = 10;
        public float ToCameraDeadZone = 10;
        [Tooltip("The size of the ease curve across the 360 spectrum")]
        public float SmoothingDelta = 35;
        // public EaseFunctions SmoothingFunction;
        public float TimeToWaitBeforeTurningWithPlayer = 1;
        [Range(0, 1)]
        public float Power = 1;
    }

    [Serializable]
    public class FovSettings
    {
        public float Landscape = 50;
        public float Portrait = 70;
        public float VisceralMultiplier = .7f;
    }

    [Serializable]
    public class InputSettings
    {
        public Vector2 inputMultiplier = Vector2.one;
        public CombatOffsetSettings CombatOffsetInput;

        [Serializable]
        public class CombatOffsetSettings
        {
            public float clamp = 1;
            public float multiplier = .5f;
            public float lerp = 5;
        }
    }
}