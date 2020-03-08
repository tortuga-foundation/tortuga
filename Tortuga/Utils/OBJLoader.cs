using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Tortuga.Components;
using System.Threading.Tasks;

namespace Tortuga.Utils
{
    public class OBJLoader
    {
        public Vertex[] ToGraphicsVertices => _graphicsVertices;
        public uint[] ToGraphicsIndex => _graphicsIndices;

        private struct Index
        {
            public int Vertex;
            public int Texture;
            public int Normal;
        };

        private List<Vector3> _vertices;
        private List<Vector2> _textures;
        private List<Vector3> _normals;
        private List<Index> _indices;
        private uint[] _graphicsIndices;
        private Vertex[] _graphicsVertices;

        private OBJLoader() { }

        public static async Task<OBJLoader> Load(string file)
        {
            if (File.Exists(file) == false)
                throw new FileNotFoundException();

            var loader = new OBJLoader();
            await Task.Run(() =>
            {
                loader._vertices = new List<Vector3>();
                loader._textures = new List<Vector2>();
                loader._normals = new List<Vector3>();
                loader._indices = new List<Index>();

                var rawOBJ = File.ReadAllText(file).Split('\n');
                foreach (var line in rawOBJ)
                {
                    if (line.StartsWith("v "))
                    {
                        var match = Regex.Match(line, @"v ([0-9\.\-]+) ([0-9\.\-]+) ([0-9\.\-]+)");
                        if (match.Success)
                        {
                            loader._vertices.Add(new Vector3(
                                Convert.ToSingle(match.Groups[1].Value),
                                Convert.ToSingle(match.Groups[2].Value),
                                Convert.ToSingle(match.Groups[3].Value)
                            ));
                        }
                    }
                    else if (line.StartsWith("vt "))
                    {
                        var match = Regex.Match(line, @"vt ([0-9\.\-]+) ([0-9\.\-]+)");
                        if (match.Success)
                        {
                            loader._textures.Add(new Vector2(
                                Convert.ToSingle(match.Groups[1].Value),
                                Convert.ToSingle(match.Groups[2].Value)
                            ));
                        }
                    }
                    else if (line.StartsWith("vn "))
                    {
                        var match = Regex.Match(line, @"vn ([0-9\.\-]+) ([0-9\.\-]+) ([0-9\.\-]+)");
                        if (match.Success)
                        {
                            loader._normals.Add(new Vector3(
                                Convert.ToSingle(match.Groups[1].Value),
                                Convert.ToSingle(match.Groups[2].Value),
                                Convert.ToSingle(match.Groups[3].Value)
                            ));
                        }
                    }
                    else if (line.StartsWith("f "))
                    {
                        var match = Regex.Match(line, @"f ([0-9]+)/([0-9]+)/([0-9]+) ([0-9]+)/([0-9]+)/([0-9]+) ([0-9]+)/([0-9]+)/([0-9]+)");
                        if (match.Success)
                        {
                            loader._indices.Add(new Index
                            {
                                Vertex = Convert.ToInt32(match.Groups[1].Value) - 1,
                                Texture = Convert.ToInt32(match.Groups[2].Value) - 1,
                                Normal = Convert.ToInt32(match.Groups[3].Value) - 1
                            });
                            loader._indices.Add(new Index
                            {
                                Vertex = Convert.ToInt32(match.Groups[4].Value) - 1,
                                Texture = Convert.ToInt32(match.Groups[5].Value) - 1,
                                Normal = Convert.ToInt32(match.Groups[6].Value) - 1
                            });
                            loader._indices.Add(new Index
                            {
                                Vertex = Convert.ToInt32(match.Groups[7].Value) - 1,
                                Texture = Convert.ToInt32(match.Groups[8].Value) - 1,
                                Normal = Convert.ToInt32(match.Groups[9].Value) - 1
                            });
                        }
                    }
                }

                loader._graphicsIndices = new uint[loader._indices.Count];
                for (uint i = 0; i < loader._indices.Count; i++)
                    loader._graphicsIndices[i] = i;

                var output = new List<Vertex>();
                foreach (var index in loader._indices)
                {
                    output.Add(new Vertex
                    {
                        Position = loader._vertices[index.Vertex],
                        TextureCoordinates = loader._textures[index.Texture],
                        Normal = loader._normals[index.Normal]
                    });
                }
                loader._graphicsVertices = output.ToArray();
            });
            return loader;
        }
    }
}