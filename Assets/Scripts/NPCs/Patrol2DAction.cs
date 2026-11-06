using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Patrol 2D",
        description: "Moves a 2D GameObject between waypoints using Rigidbody2D.",
        category: "Action",
        story: "[Agent] patrols along [Waypoints]",
        id: "b82f76a9e43c4d9e91f0a12b7c63d501"
    )]
    internal partial class Patrol2DAction : Action
    {
        [SerializeReference]
        public BlackboardVariable<GameObject> Agent;

        [SerializeReference]
        public BlackboardVariable<List<GameObject>> Waypoints;

        [SerializeReference]
        public BlackboardVariable<float> Speed = new(3f);

        [SerializeReference]
        public BlackboardVariable<float> WaypointWaitTime = new(1f);

        [SerializeReference]
        public BlackboardVariable<float> DistanceThreshold = new(0.2f);

        [SerializeReference]
        public BlackboardVariable<string> AnimatorSpeedParam =
            new("SpeedMagnitude");

        [Tooltip("Should patrol restart from the latest point?")]
        [SerializeReference]
        public BlackboardVariable<bool> PreserveLatestPatrolPoint =
            new(false);

        private Rigidbody2D m_Rigidbody;
        private Animator m_Animator;
        private SpriteRenderer m_SpriteRenderer;

        [CreateProperty]
        private Vector2 m_CurrentTarget;

        [CreateProperty]
        private float m_WaypointWaitTimer;

        [CreateProperty]
        private int m_CurrentPatrolPoint = 0;

        [CreateProperty]
        private bool m_Waiting;

        private float m_CurrentSpeed;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }

            if (Waypoints.Value == null || Waypoints.Value.Count == 0)
            {
                LogFailure("No waypoints assigned.");
                return Status.Failure;
            }

            m_Rigidbody = Agent.Value.GetComponent<Rigidbody2D>();

            if (m_Rigidbody == null)
            {
                LogFailure("Agent requires a Rigidbody2D.");
                return Status.Failure;
            }

            m_Animator = Agent.Value.GetComponentInChildren<Animator>();
            m_SpriteRenderer =
                Agent.Value.GetComponentInChildren<SpriteRenderer>();

            // Make sure every waypoint is assigned.
            foreach (GameObject waypoint in Waypoints.Value)
            {
                if (waypoint == null)
                {
                    LogFailure("A waypoint is missing.");
                    return Status.Failure;
                }
            }

            if (!PreserveLatestPatrolPoint.Value)
                m_CurrentPatrolPoint = -1;
            else
                m_CurrentPatrolPoint =
                    (m_CurrentPatrolPoint - 1 + Waypoints.Value.Count)
                    % Waypoints.Value.Count;

            m_Waiting = false;
            m_WaypointWaitTimer = 0f;

            MoveToNextWaypoint();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Agent.Value == null ||
                Waypoints.Value == null ||
                m_Rigidbody == null)
            {
                return Status.Failure;
            }

            // Handle waiting at a waypoint.
            if (m_Waiting)
            {
                m_WaypointWaitTimer -= Time.deltaTime;

                if (m_WaypointWaitTimer <= 0f)
                {
                    m_WaypointWaitTimer = 0f;
                    m_Waiting = false;

                    MoveToNextWaypoint();
                }

                return Status.Running;
            }

            // Calculate horizontal distance to waypoint.
            float distance = Mathf.Abs(
                m_CurrentTarget.x - m_Rigidbody.position.x
            );

            // Check if waypoint has been reached.
            if (distance <= DistanceThreshold.Value)
            {
                StopHorizontalMovement();

                m_WaypointWaitTimer = WaypointWaitTime.Value;
                m_Waiting = true;

                return Status.Running;
            }

            // Move horizontally toward the waypoint.
            float direction = Mathf.Sign(
                m_CurrentTarget.x - m_Rigidbody.position.x
            );

            m_CurrentSpeed = Speed.Value;

            m_Rigidbody.linearVelocity = new Vector2(
                direction * Speed.Value,
                m_Rigidbody.linearVelocity.y
            );

            // Flip sprite based on movement direction.
            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.flipX = direction < 0;
            }

            UpdateAnimatorSpeed();

            return Status.Running;
        }

        protected override void OnEnd()
        {
            StopHorizontalMovement();
            UpdateAnimatorSpeed(0f);
        }

        private void MoveToNextWaypoint()
        {
            m_CurrentPatrolPoint =
                (m_CurrentPatrolPoint + 1) % Waypoints.Value.Count;

            m_CurrentTarget =
                Waypoints.Value[m_CurrentPatrolPoint].transform.position;
        }

        private void StopHorizontalMovement()
        {
            if (m_Rigidbody != null)
            {
                m_Rigidbody.linearVelocity = new Vector2(
                    0f,
                    m_Rigidbody.linearVelocity.y
                );
            }

            m_CurrentSpeed = 0f;
            UpdateAnimatorSpeed(0f);
        }

        private void UpdateAnimatorSpeed(float explicitSpeed = -1f)
        {
            if (m_Animator == null ||
                string.IsNullOrEmpty(AnimatorSpeedParam.Value))
                return;

            float speed = explicitSpeed >= 0f
                ? explicitSpeed
                : m_CurrentSpeed;

            m_Animator.SetFloat(
                AnimatorSpeedParam.Value,
                speed
            );
        }
    }
}

