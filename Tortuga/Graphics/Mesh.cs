using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Vertex object that is used for rendering
    /// </summary>
    [System.Serializable]
    public struct Vertex
    {
        /// <summary>
        /// Position of the vertex
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Texture coordinate of the vertex
        /// </summary>
        public Vector2 TextureCoordinates;
        /// <summary>
        /// Normal position of the vertex
        /// </summary>
        public Vector3 Normal;
    }


    /// <summary>
    /// Contains the mesh data
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// Indices for this mesh
        /// </summary>
        public ushort[] Indices;

        /// <summary>
        /// Vertices for this mesh that will be used in rendering
        /// </summary>
        public Vertex[] Vertices;

        private struct OBJIndex
        {
            public int Vertex;
            public int Texture;
            public int Normal;
        };

        private static Mesh LoadOBJ(string file)
        {
            var vertices = new List<Vector3>();
            var textures = new List<Vector2>();
            var normals = new List<Vector3>();
            var indices = new List<OBJIndex>();

            var rawOBJ = File.ReadAllText(file).Split('\n');
            foreach (var line in rawOBJ)
            {
                if (line.StartsWith("v "))
                {
                    var match = Regex.Match(line, @"v ([0-9\.\-]+) ([0-9\.\-]+) ([0-9\.\-]+)");
                    if (match.Success)
                    {
                        vertices.Add(new Vector3(
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
                        textures.Add(new Vector2(
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
                        normals.Add(new Vector3(
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
                        indices.Add(new OBJIndex
                        {
                            Vertex = Convert.ToInt32(match.Groups[1].Value) - 1,
                            Texture = Convert.ToInt32(match.Groups[2].Value) - 1,
                            Normal = Convert.ToInt32(match.Groups[3].Value) - 1
                        });
                        indices.Add(new OBJIndex
                        {
                            Vertex = Convert.ToInt32(match.Groups[4].Value) - 1,
                            Texture = Convert.ToInt32(match.Groups[5].Value) - 1,
                            Normal = Convert.ToInt32(match.Groups[6].Value) - 1
                        });
                        indices.Add(new OBJIndex
                        {
                            Vertex = Convert.ToInt32(match.Groups[7].Value) - 1,
                            Texture = Convert.ToInt32(match.Groups[8].Value) - 1,
                            Normal = Convert.ToInt32(match.Groups[9].Value) - 1
                        });
                    }
                }
            }
            var mesh = new Mesh();

            mesh.Indices = new ushort[indices.Count];
            for (ushort i = 0; i < indices.Count; i++)
                mesh.Indices[i] = i;

            var output = new List<Graphics.Vertex>();
            foreach (var index in indices)
            {
                output.Add(new Graphics.Vertex
                {
                    Position = vertices[index.Vertex],
                    TextureCoordinates = textures[index.Texture],
                    Normal = normals[index.Normal]
                });
            }
            mesh.Vertices = output.ToArray();
            return mesh;
        }

        /// <summary>
        /// This function can be used to load an obj file as a mesh
        /// </summary>
        /// <param name="file">Path to the obj file</param>
        /// <returns>Mesh object that can be used for rendering</returns>
        public static Task<Mesh> Load(string file)
        {
            if (File.Exists(file) == false)
                throw new FileNotFoundException();

            if (file.ToLower().EndsWith(".obj"))
                return Task.FromResult(LoadOBJ(file));
            else
                throw new NotSupportedException("this type of file is not currently supported");
        }
    }
}