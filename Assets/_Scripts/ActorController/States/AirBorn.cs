using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using actorController.displace;
using actorController.controller;
using System;

namespace actorController.state
{
    [RequireComponent(typeof(ActorController))]
    public class AirBorn : MonoBehaviour, IActorState
    {
        ActorController actorController;
        IDisplace locomotion;
        PlayerAnimator animator;

        [SerializeField] float maxSpeed = 20f;
        [SerializeField] float gravity = 9f;
        [Range(0f, 1f)][SerializeField] float accelerationTime;

        float currentVelocitySmoother = 0f;

        private void OnEnable()
        {
            animator = GetComponent<PlayerAnimator>();
        }

        public void StateInitial(ActorController controller)
        {
            animator.SetFallingAnimation(true);
            // Debug.Log("Initial of: " + this.name);
            this.actorController = controller;
            locomotion = controller.AllDisplacements[typeof(Locomotion)];
        }

        public void OnStateChange()
        {
            // Debug.Log("OnStateChange of: " + this.name);
        }

        public void StateUpdate()
        {
            if (IsGrounded())
            {
                actorController.ChangeState(actorController.States[typeof(Grouned)]);
            }
            locomotion.AddDisplacement();
        }

        private bool IsGrounded()
        {
            return
                actorController.CollisionDetection.CollisionInfo.vertical[0].collider != null
                && actorController.CollisionDetection.CollisionInfo.direction.vertical == -1;
        }

        public Vector2 CalculateVelocity(List<IDisplace> displaces)
        {
            Vector2 velocity = actorController.CurrentVelocity;
            Vector2 targetVelocity = SumOfAllDisplacments(displaces);

            velocity.x = targetVelocity.x;
            velocity.y += targetVelocity.y;

            velocity.y -= gravity * Time.deltaTime * 0.1f;

            velocity = ClampVelocity(velocity);

            return velocity;

        }


        private Vector2 ClampVelocity(Vector2 velocity)
        {
            if (IsOverMaxSpeed(ref velocity))
                velocity = maxSpeed * velocity.normalized;
            return velocity;
        }

        private bool IsOverMaxSpeed(ref Vector2 velocity)
        {
            return velocity.magnitude > maxSpeed;
        }

        private Vector2 SumOfAllDisplacments(List<IDisplace> displaces)
        {
            Vector2 targetVelocity = new Vector2();
            foreach (var i in displaces)
            {
                if (i.GetType() == typeof(Jump))
                    continue;

                if (i.GetType() == typeof(Locomotion))
                {
                    targetVelocity.x += ApplyMovementSmoothing(actorController.CurrentVelocity, i.GetCurrentDisplacement()) * Time.deltaTime;
                    continue;
                }

                targetVelocity += i.GetCurrentDisplacement();
            }

            return targetVelocity;
        }

        private float ApplyMovementSmoothing(Vector2 velocity, Vector2 targetVelocity)
        {
            return Mathf.SmoothDamp(velocity.x, targetVelocity.x, ref currentVelocitySmoother, accelerationTime, maxSpeed);
        }

    }
}
