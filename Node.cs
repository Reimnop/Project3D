using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Project3D
{
    public class Node
    {
        public Vector3d Position = Vector3.Zero;
        public Vector3d Scale = Vector3.One;
        public Quaterniond Rotation = Quaterniond.Identity;

        public Vector3Sequence PositionSequence;
        public Vector3Sequence ScaleSequence;
        public QuaternionSequence RotationSequence;

        public Vector3 Color = Vector3.One;

        public Vertex[] Vertices;
        public int[] Indices;

        public List<Node> Children = new List<Node>();
    }
}
