using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Tortuga.Graphics;

namespace Tortuga.Utils
{
    public class OBJLoader
    {
        public struct Index
        {
            public int Vertex;
            public int Texture;
            public int Normal;
        };

        public List<Vector3> Vertices;
        public List<Vector2> Textures;
        public List<Vector3> Normals;
        public List<Index> Indices;

        public OBJLoader(string file)
        {
            Vertices = new List<Vector3>();
            Textures = new List<Vector2>();
            Normals = new List<Vector3>();
            Indices = new List<Index>();

            var rawOBJ = File.ReadAllText(file).Split('\n');
            foreach (var line in rawOBJ)
            {
                if (line.StartsWith("v "))
                {
                    var match = Regex.Match(line, @"v ([0-9\.\-]+) ([0-9\.\-]+) ([0-9\.\-]+)");
                    if (match.Success)
                    {
                        Vertices.Add(new Vector3(
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
                        Textures.Add(new Vector2(
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
                        Normals.Add(new Vector3(
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
                        Indices.Add(new Index
                        {
                            Vertex = Convert.ToInt32(match.Groups[1].Value) - 1,
                            Texture = Convert.ToInt32(match.Groups[2].Value) - 1,
                            Normal = Convert.ToInt32(match.Groups[3].Value) - 1
                        });
                        Indices.Add(new Index
                        {
                            Vertex = Convert.ToInt32(match.Groups[4].Value) - 1,
                            Texture = Convert.ToInt32(match.Groups[5].Value) - 1,
                            Normal = Convert.ToInt32(match.Groups[6].Value) - 1
                        });
                        Indices.Add(new Index
                        {
                            Vertex = Convert.ToInt32(match.Groups[7].Value) - 1,
                            Texture = Convert.ToInt32(match.Groups[8].Value) - 1,
                            Normal = Convert.ToInt32(match.Groups[9].Value) - 1
                        });
                    }
                }
            }
        }

        public Vertex[] ToGraphicsVertices
        {
            get
            {
                var output = new List<Vertex>();
                foreach (var index in Indices)
                {
                    output.Add(new Vertex
                    {
                        Position = Vertices[index.Vertex],
                        TextureCoordinates = Textures[index.Texture],
                        Normal = Normals[index.Normal]
                    });
                }
                return output.ToArray();
            }
        }
        public uint[] ToGraphicsIndex
        {
            get
            {
                var output = new uint[Indices.Count];
                for (uint i = 0; i < Indices.Count; i++)
                    output[i] = i;
                return output;
            }
        }
    }
}