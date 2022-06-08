using OpenTK.Mathematics;
using PAPrefabToolkit;

namespace Project3D.Engine.Data;

/// <summary>
/// A triangle class that tries to imitate an array (because I refuse to heap allocate)
/// </summary>
public struct Triangle
{
    private Vector3d point0;
    private Vector3d point1;
    private Vector3d point2;

    public Triangle(ReadOnlySpan<Vector3d> points)
    {
        point0 = points[0];
        point1 = points[1];
        point2 = points[2];
    }
    
    public Triangle(Vector3d point0, Vector3d point1, Vector3d point2)
    {
        this.point0 = point0;
        this.point1 = point1;
        this.point2 = point2;
    }

    public Vector3d this[int i]
    {
        get => i switch
        {
            0 => point0,
            1 => point1,
            2 => point2,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (i)
            {
                case 0:
                    point0 = value;
                    break;
                case 1:
                    point1 = value;
                    break;
                case 2:
                    point2 = value;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}