using Assimp;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Project3D
{
    public class AssetImporter : IDisposable
    {
        public struct AnimChannelPair
        {
            public Animation Animation;
            public NodeAnimationChannel Channel;
        }

        private string _path;

        private Dictionary<string, AnimChannelPair> _animLookup = new Dictionary<string, AnimChannelPair>();

        private AssimpContext _context;
        private Scene _scene;

        public AssetImporter(string path)
        {
            _path = path;

            _context = new AssimpContext();
            _scene = _context.ImportFile(_path, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals);
        }

        public Node LoadModel()
        {
            //load animations to lookup table
            foreach (Animation anim in _scene.Animations)
            {
                if (anim.HasNodeAnimations)
                {
                    foreach (NodeAnimationChannel channel in anim.NodeAnimationChannels) 
                    {
                        _animLookup.TryAdd(channel.NodeName, new AnimChannelPair
                        {
                            Animation = anim,
                            Channel = channel
                        });
                    }
                }
            }

            return ProcessNodeRecursively(_scene.RootNode);
        }

        private Node ProcessNodeRecursively(Assimp.Node node)
        {
            node.Transform.Decompose(out var scale, out var rotation, out var translation);

            Node pNode = new Node();
            pNode.Position = new Vector3d(translation.X, translation.Y, translation.Z);
            pNode.Scale = new Vector3d(scale.X, scale.Y, scale.Z);
            pNode.Rotation = new Quaterniond(rotation.X, rotation.Y, rotation.Z, rotation.W);

            //convert assimp animations
            if (_animLookup.TryGetValue(node.Name, out AnimChannelPair animChannelPair))
            {
                Animation animation = animChannelPair.Animation;
                NodeAnimationChannel animationChannel = animChannelPair.Channel;

                if (animationChannel.HasPositionKeys)
                {
                    Keyframe<Vector3d>[] keyframes = new Keyframe<Vector3d>[animationChannel.PositionKeyCount];

                    List<VectorKey> posKeys = animationChannel.PositionKeys;

                    for (int i = 0; i < keyframes.Length; i++)
                    {
                        keyframes[i] = new Keyframe<Vector3d>()
                        {
                            Time = (float)posKeys[i].Time / (float)animation.TicksPerSecond,
                            Value = new Vector3(posKeys[i].Value.X, posKeys[i].Value.Y, posKeys[i].Value.Z)
                        };
                    }

                    pNode.PositionSequence = new Vector3Sequence(keyframes);
                }

                if (animationChannel.HasScalingKeys)
                {
                    Keyframe<Vector3d>[] keyframes = new Keyframe<Vector3d>[animationChannel.ScalingKeyCount];

                    List<VectorKey> scaKeys = animationChannel.ScalingKeys;

                    for (int i = 0; i < keyframes.Length; i++)
                    {
                        keyframes[i] = new Keyframe<Vector3d>()
                        {
                            Time = (float)scaKeys[i].Time / (float)animation.TicksPerSecond,
                            Value = new Vector3d(scaKeys[i].Value.X, scaKeys[i].Value.Y, scaKeys[i].Value.Z)
                        };
                    }

                    pNode.ScaleSequence = new Vector3Sequence(keyframes);
                }

                if (animationChannel.HasRotationKeys)
                {
                    Keyframe<Quaterniond>[] keyframes = new Keyframe<Quaterniond>[animationChannel.RotationKeyCount];

                    List<QuaternionKey> rotKeys = animationChannel.RotationKeys;

                    for (int i = 0; i < keyframes.Length; i++)
                    {
                        keyframes[i] = new Keyframe<Quaterniond>()
                        {
                            Time = (float)rotKeys[i].Time / (float)animation.TicksPerSecond,
                            Value = new Quaterniond(rotKeys[i].Value.X, rotKeys[i].Value.Y, rotKeys[i].Value.Z, rotKeys[i].Value.W)
                        };
                    }

                    pNode.RotationSequence = new QuaternionSequence(keyframes);
                }
            }

            if (node.HasMeshes)
            {
                foreach (int meshIndex in node.MeshIndices)
                {
                    //multiple meshes per node
                    Node mNode = new Node();
                    Mesh mesh = _scene.Meshes[meshIndex];

                    Vertex[] vertices = new Vertex[mesh.VertexCount];
                    int[] indices = mesh.GetIndices();

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3D vertex = mesh.Vertices[i];
                        Vector3D normal = mesh.Normals[i];

                        vertices[i] = new Vertex
                        {
                            Position = new Vector3(vertex.X, vertex.Y, vertex.Z),
                            Normal = new Vector3(normal.X, normal.Y, normal.Z)
                        };
                    }

                    mNode.Vertices = vertices;
                    mNode.Indices = indices;

                    Material material = _scene.Materials[mesh.MaterialIndex];

                    mNode.Color = new Vector3(material.ColorDiffuse.R, material.ColorDiffuse.G, material.ColorDiffuse.B);

                    //add node mesh as child
                    pNode.Children.Add(mNode);
                }
            }

            foreach (Assimp.Node child in node.Children)
                pNode.Children.Add(ProcessNodeRecursively(child));

            return pNode;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
