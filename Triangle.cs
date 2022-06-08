using OpenTK.Mathematics;
using System;

namespace Project3D
{
    public class Triangle
    {
        private Vector2d[] _points;

        public Vector2d this[int i]
        {
            get { return _points[i]; }
            set { _points[i] = value; }
        }

        public double Area
        {
            get
            {
                double a = (_points[0] - _points[1]).Length;
                double b = (_points[1] - _points[2]).Length;
                double c = (_points[2] - _points[0]).Length;

                double p = (a + b + c) / 2f;

                return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
            }
        }

        public Triangle()
        {
            _points = new Vector2d[3];
        }

        public Triangle(Vector2d pointA, Vector2d pointB, Vector2d pointC)
        {
            _points = new Vector2d[3];
            _points[0] = pointA;
            _points[1] = pointB;
            _points[2] = pointC;
        }

        public override string ToString()
        {
            return $"{{ {_points[0]}; {_points[1]}; {_points[2]} }}";
        }
    }
}
