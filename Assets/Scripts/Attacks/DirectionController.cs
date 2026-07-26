using UnityEngine;

namespace Attacks
{
    public class DirectionController
    {
        public float direction;
        private readonly float speed;

        public DirectionController(float direction, float speed)
        {
            this.direction = direction;
            this.speed = speed;
        }

        public void Update(Vector2 delta, float deltaTime)
        {
            if (delta.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float desiredDirection =
                -Vector2.SignedAngle(delta, Vector2.up);
            float maxDelta =
                Mathf.Max(0f, speed) * Mathf.Max(0f, deltaTime);

            direction = Mathf.Repeat(
                Mathf.MoveTowardsAngle(direction, desiredDirection, maxDelta),
                360f
            );
        }
    }
}
