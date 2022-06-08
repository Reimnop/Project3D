using OpenTK.Mathematics;
using Project3D.Engine.Animation;

namespace Project3D.Engine.Data;

public class AnimatedObject
{
    public Triangle[] Triangles { get; }
    public Sequence<Matrix4> Animation { get; }

    public AnimatedObject(Triangle[] triangles, Sequence<Matrix4> animation)
    {
        Triangles = triangles;
        Animation = animation;
    }
}