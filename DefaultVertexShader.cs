using OpenTK.Mathematics;

namespace Project3D
{
    public class DefaultVertexShader : VertexShader
    {
        public DefaultVertexShader(Vertex vertex, VertexInData vertexIn) : base(vertex, vertexIn)
        {
        }

        public override VertexOutData ProcessVertex()
        {
            return new VertexOutData
            {
                Position = new Vector4d(Vertex.Position, 1f) * VertexIn.ModelViewProjection,
                Color = VertexIn.Color
            };
        }
    }
}
