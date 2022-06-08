using PAPrefabToolkit;
using PAPrefabToolkit.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace Project3D
{
    internal class Program
    {
        private static List<PrefabObject> prefabObjects = new List<PrefabObject>();

        private static void Main(string[] args)
        {
            Node node;

            using (AssetImporter importer = new AssetImporter("hk.dae"))
            {
                node = importer.LoadModel();
            }
            
            Prefab prefab = new Prefab("3d-testing", PrefabType.Misc_1);

            PrefabObject transform = new PrefabObject(prefab, "Transform")
            {
                ObjectType = PrefabObjectType.Empty
            };

            transform.Events.ScaleKeyframes.Add(new PrefabObject.ObjectEvents.ScaleKeyframe
            {
                Value = new Vector2(71.1111111111f, 40f) / 1.5f * new Vector2(1.0f, -1.0f)
            });

            Vector3[] theme =
            {
                Vector3.One
            };

            Renderer renderer = new Renderer(node, theme, transform);

            for (int i = 0; i < 1; i++)
            {
                float time = i / 24f;
                
                // RecursivelyUpdateAnimation(time, node);
                renderer.Render(prefab, prefabObjects, time);

                Console.WriteLine("rendering frame " + i);
            }

            File.WriteAllText("3d-testing.lsp", PrefabBuilder.BuildPrefab(prefab));
        }

        private static void RecursivelyUpdateAnimation(float time, Node node)
        {
            if (node.PositionSequence != null)
            {
                node.Position = node.PositionSequence.GetValue(time);
            }

            if (node.ScaleSequence != null)
            {
                node.Scale = node.ScaleSequence.GetValue(time);
            }

            if (node.RotationSequence != null)
            {
                node.Rotation = node.RotationSequence.GetValue(time);
            }

            foreach (Node child in node.Children)
                RecursivelyUpdateAnimation(time, child);
        }
    }
}
