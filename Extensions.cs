using OpenTK.Mathematics;
using System;
using System.Linq;

namespace Project3D
{
    public static class Extensions
    {
        private struct AnglePointPair
        {
            public Vector2d Position;
            public double Angle;

            public AnglePointPair(Vector2d position, double angle)
            {
                Position = position;
                Angle = angle;
            }
        }

        public static void ToRightAngledTriangles(this Triangle source, out Triangle tri0, out Triangle tri1)
        {
            AnglePointPair[] pairs =
            {
                new AnglePointPair(source[0], AngleBetween(source[1] - source[0], source[2] - source[0])), //bac
                new AnglePointPair(source[1], AngleBetween(source[2] - source[1], source[0] - source[1])), //cba
                new AnglePointPair(source[2], AngleBetween(source[1] - source[2], source[0] - source[2]))  //bca
            };

            pairs = pairs.OrderByDescending(x => x.Angle).ToArray();

            double tanB = Math.Tan(pairs[1].Angle);

            Vector2d vecBC = pairs[2].Position - pairs[1].Position;
            double BC = vecBC.Length;
            double AH = 2f * source.Area / BC;

            double BH = AH / tanB;
            Vector2d H = Vector2d.Lerp(pairs[1].Position, pairs[2].Position, BH / BC);

            tri0 = new Triangle(H, pairs[0].Position, pairs[1].Position);
            tri1 = new Triangle(H, pairs[0].Position, pairs[2].Position);
        }

        public static void GetPositionScaleRotation(this Triangle triangle, out Vector2d position, out Vector2d scale, out double rotation)
        {
            Vector2d AB = triangle[1] - triangle[0];
            Vector2d AC = triangle[2] - triangle[0];

            Vector2d ABdir = Vector2d.Normalize(AB);
            Vector2d ACdir = Vector2d.Normalize(AC);

            position = triangle[0];
            rotation = Math.Atan2(ABdir.Y, ABdir.X); //rotate the triangle according to AB

            scale = new Vector2(0f, 0f);

            Vector2d rotatedACdir = RotateVector2(ACdir, -rotation); //rotate AC
            if (rotatedACdir.Y < 0f)
            {
                scale.Y = -AC.Length;
            }
            else
            {
                scale.Y = AC.Length;
            }

            scale.X = AB.Length;
        }

        private static Vector2d RotateVector2(Vector2d vec, double radians)
        {
            return new Vector2d(
                vec.X * Math.Cos(radians) - vec.Y * Math.Sin(radians),
                vec.X * Math.Sin(radians) + vec.Y * Math.Cos(radians)
            );
        }

        private static double AngleBetween(Vector2d vec0, Vector2d vec1)
        {
            return Math.Acos(Vector2d.Dot(vec0, vec1) / (vec0.Length * vec1.Length));
        }
    }
}
