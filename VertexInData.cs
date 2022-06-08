using OpenTK.Mathematics;

namespace Project3D
{
    public struct VertexInData
    {
        public Matrix4d Model;
        public Matrix4d ModelViewProjection;
        public Vector3d LightDirection;
        public Vector3 Color;
    }
}
