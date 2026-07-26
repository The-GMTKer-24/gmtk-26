using System;
using UnityEngine;

namespace Attacks
{
    public class DirectionController
    {
        public float direction;
        private float speed;
        public String print;

        public DirectionController(float direction, float speed)
        {
            this.direction = direction;
            this.speed = speed;
        }

        public void Update(Vector2 delta, float deltaTime)
        {
            float desiredDirection = (Unity.Mathematics.math.atan2(delta.y, delta.x) / Unity.Mathematics.math.PI2 * 360 + 360 - 90f) % 360;
            float lowerDesiredDirection = desiredDirection - 360;
            float currentDirection = direction;
            if (direction > desiredDirection)
            {
                currentDirection = direction - 360;
            }
            float movement = speed * deltaTime;
            float distance = desiredDirection - currentDirection;
            if (desiredDirection - currentDirection > currentDirection - lowerDesiredDirection)
            {
                movement = -movement;
                distance = lowerDesiredDirection - currentDirection;
            }

            if (Mathf.Abs(movement) >= Mathf.Abs(distance))
            {
                movement = distance;
            }

            print = ("Update: " + desiredDirection + ", " + movement + ", " + distance + ", " + currentDirection);
            direction = (direction + movement + 360 * 10) % 360;
        }
    }
}