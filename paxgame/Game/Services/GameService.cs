
using System.Numerics;
using Game.Models;

namespace Game.Services
{
    public static class GameService
    {
        public static readonly List<int> directions = new List<int>() { 45, 90, 135, 180, 225, 270, 315 };
        public static Random random = new Random();

        public static int GetRandomDirection()
        {
            int index = random.Next(0, directions.Count);
            return directions[index];
        }

        public static Vector2 mirrorImage(float a, float b, float c, Vector2 pos)
        {
            float temp = -2 * (a * pos.X + b * pos.Y + c) /
                            (a * a + b * b);
            float x = temp * a + pos.X;
            float y = temp * b + pos.Y;
            return new Vector2(x, y);
        }

        public static float Distance(Vector2 v1, Vector2 v2)
        {
            float dx = v1.X - v2.X;
            float dy = v1.Y - v2.Y;
            return dx * dx + dy * dy;
        }

        public static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (float)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (float)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Math/Vector2.cs
        // Moves a point /current/ towards /target/.
        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
        {
            // avoid vector ops because current scripting backends are terrible at inlining
            float toVector_x = target.X - current.X;
            float toVector_y = target.Y - current.Y;

            float sqDist = toVector_x * toVector_x + toVector_y * toVector_y;

            if (sqDist == 0 || (maxDistanceDelta >= 0 && sqDist <= maxDistanceDelta * maxDistanceDelta))
                return target;

            float dist = (float)Math.Sqrt(sqDist);

            return new Vector2(current.X + toVector_x / dist * maxDistanceDelta,
                current.Y + toVector_y / dist * maxDistanceDelta);
        }

        public static BattleUnit? Hit(Hit hit, BattleUnit unit)
        {
            double mitigation = hit.Attacs * unit.BattleDefence.Armor;
            unit.BattleDefence.BattleHealthpoints -= hit.Damage - mitigation;

            if (unit.BattleDefence.BattleHealthpoints <= 0)
            {
                unit.BattleDefence.BattleHealthpoints = 0;
                return hit.Unit;
            } else
            {
                return null;
            }
        }
    }
}