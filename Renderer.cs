using OpenTK.Mathematics;
using PAPrefabToolkit.Data;
using System;
using System.Collections.Generic;

namespace Project3D
{
    public class Renderer
    {
        private Node _rootNode;
        private PrefabObject _transformObj;

        private Vector3[] _theme;

        private Matrix4d _view = Matrix4d.Identity;
        private Matrix4d _projection = Matrix4d.CreateOrthographic(16f / 9f * 5.0f, 5.0f, -1.0f, 1.0f);

        private Vector3d _lightDir = Vector3d.Normalize(new Vector3(-1f, -0.5f, -0.5f));

        public Renderer(Node rootNode, Vector3[] theme, PrefabObject transformObj)
        {
            _rootNode = rootNode;
            _theme = theme;
            _transformObj = transformObj;
        }

        public void Render(Prefab prefab, List<PrefabObject> objects, float time)
        {
            int index = 0;
            RecursivelyRenderScene(prefab, _rootNode, Matrix4d.Identity, objects, time, objects.Count == 0, ref index);
        }

        private void RecursivelyRenderScene(Prefab prefab, Node node, Matrix4d parentTransform, List<PrefabObject> objects, float time, bool createNew, ref int index)
        {
            Matrix4d local =
                Matrix4d.Scale(node.Scale) *
                Matrix4d.CreateFromQuaternion(node.Rotation) *
                Matrix4d.CreateTranslation(node.Position);

            Matrix4d model = local * parentTransform;

            if (node.Vertices != null && node.Indices != null)
            {
                VertexInData vsIn = new VertexInData
                {
                    Model = model,
                    ModelViewProjection = model * _view * _projection,
                    LightDirection = _lightDir,
                    Color = node.Color
                };
            
                ProcessedTriangle[] triangles = RenderGeometry(node.Vertices, node.Indices, vsIn);

                for (int i = 0; i < triangles.Length; i++)
                {
                    ProcessedTriangle triangle = triangles[i];

                    PrefabObject prefabObject;
                    if (createNew)
                    {
                        prefabObject = new PrefabObject(prefab, "Tri-" + i, parent: _transformObj)
                        {
                            AutoKillType = PrefabObjectAutoKillType.Fixed,
                            AutoKillOffset = 10.0f,
                            ParentType = (true, true, true),
                            Origin = (PrefabObjectOriginX.Right, PrefabObjectOriginY.Top),
                            Shape = PrefabObjectShape.Triangle,
                            ShapeOption = 2,
                            Depth = triangle.Depth
                        };
                        objects.Add(prefabObject);
                    }
                    else
                    {
                        prefabObject = objects[index];
                    }
                    index++;

                    prefabObject.Events.PositionKeyframes.Add(new PrefabObject.ObjectEvents.PositionKeyframe()
                    {
                        Time = time,
                        Value = new System.Numerics.Vector2((float)triangle.Position.X, (float)triangle.Position.Y),
                        Easing = PrefabObjectEasing.Instant
                    });

                    prefabObject.Events.ScaleKeyframes.Add(new PrefabObject.ObjectEvents.ScaleKeyframe()
                    {
                        Time = time,
                        Value = new System.Numerics.Vector2((float)triangle.Scale.X, (float)triangle.Scale.Y),
                        Easing = PrefabObjectEasing.Instant
                    });

                    var rotKfs = prefabObject.Events.RotationKeyframes;
                    float lastRot = 0f;
                    for (int j = 0; j < rotKfs.Count; j++)
                    {
                        lastRot += rotKfs[j].Value;
                    }
                    rotKfs.Add(new PrefabObject.ObjectEvents.RotationKeyframe()
                    {
                        Time = time,
                        Value = (float)(MathHelper.RadiansToDegrees(triangle.Rotation) - lastRot),
                        Easing = PrefabObjectEasing.Instant
                    });

                    prefabObject.Events.ColorKeyframes.Add(new PrefabObject.ObjectEvents.ColorKeyframe()
                    {
                        Time = time,
                        Value = triangle.Color,
                        Easing = PrefabObjectEasing.Instant
                    });
                }
            }

            foreach (Node child in node.Children)
                RecursivelyRenderScene(prefab, child, model, objects, time, createNew, ref index);
        }

        private ProcessedTriangle[] RenderGeometry(Vertex[] vertices, int[] indices, VertexInData shaderData)
        {
            int trianglesCount = indices.Length / 3;

            ProcessedTriangle[] triangles = new ProcessedTriangle[trianglesCount * 2];
            for (int i = 0; i < trianglesCount; i++)
            {
                int indexOffset = i * 3;

                //initialize vertex shaders
                VertexShader vs1 = new DefaultVertexShader(vertices[indices[indexOffset + 0]], shaderData);
                VertexShader vs2 = new DefaultVertexShader(vertices[indices[indexOffset + 1]], shaderData);
                VertexShader vs3 = new DefaultVertexShader(vertices[indices[indexOffset + 2]], shaderData);

                //run vertex shaders
                VertexOutData vsOut1 = vs1.ProcessVertex();
                VertexOutData vsOut2 = vs2.ProcessVertex();
                VertexOutData vsOut3 = vs3.ProcessVertex();

                //get output positions from vertex shaders
                Vector4d pos1 = vsOut1.Position;
                Vector4d pos2 = vsOut2.Position;
                Vector4d pos3 = vsOut3.Position;

                //perspective divide
                Vector3d ssPos1 = pos1.Xyz / pos1.W;
                Vector3d ssPos2 = pos2.Xyz / pos2.W;
                Vector3d ssPos3 = pos3.Xyz / pos3.W;

                //calculate averages
                double avgDepth = (ssPos1.Z + ssPos2.Z + ssPos3.Z) / 3f;
                Vector3 avgColor = (vsOut1.Color + vsOut2.Color + vsOut3.Color) / 3f;

                Triangle triangle = new Triangle(
                    new Vector2d(ssPos1.X, ssPos1.Y),
                    new Vector2d(ssPos2.X, ssPos2.Y),
                    new Vector2d(ssPos3.X, ssPos3.Y));

                //convert to right triangles
                triangle.ToRightAngledTriangles(out Triangle rTri1, out Triangle rTri2);

                //convert to pa transform
                rTri1.GetPositionScaleRotation(out var rPos1, out var sca1, out var rot1);
                rTri2.GetPositionScaleRotation(out var rPos2, out var sca2, out var rot2);

                int paCol = GetThemeColorIndex(avgColor);
                int paDepth = GetIntDepth(avgDepth);

                int triIndexOffset = i * 2;
                triangles[triIndexOffset + 0] = new ProcessedTriangle
                {
                    Position = rPos1,
                    Scale = sca1,
                    Rotation = rot1,
                    Color = paCol,
                    Depth = paDepth
                };

                triangles[triIndexOffset + 1] = new ProcessedTriangle
                {
                    Position = rPos2,
                    Scale = sca2,
                    Rotation = rot2,
                    Color = paCol,
                    Depth = paDepth
                };
            }

            return triangles;
        }

        private int GetIntDepth(double depthFloat)
        {
            return (int)(depthFloat * 64f);
        }

        private int GetThemeColorIndex(Vector3 color)
        {
            int index = 0;
            float minDelta = 4f;

            for (int i = 0; i < _theme.Length; i++)
            {
                float delta = MathF.Abs(_theme[i].X + _theme[i].Y + _theme[i].Z - color.X - color.Y - color.Z);

                if (delta < minDelta)
                {
                    minDelta = delta;
                    index = i;
                }
            }

            return index;
        }
    }
}
