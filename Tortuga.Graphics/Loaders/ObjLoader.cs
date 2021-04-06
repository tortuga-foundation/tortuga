using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Responsible for mesh object io
    /// </summary>
    public static partial class AssetLoader
    {
        /// <summary>
        /// Used to load obj files
        /// This is how indexes are represented in obj
        /// </summary>
        public class ObjIndex
        {
            /// <summary>
            /// Vertex index
            /// </summary>
            public int Vertex;
            /// <summary>
            /// Texture index
            /// </summary>
            public int Texture;
            /// <summary>
            /// Normal index
            /// </summary>
            public int Normal;
        }

        /// <summary>
        /// load a obj file into a mesh type object
        /// </summary>
        /// <param name="file">path to the obj</param>
        /// <returns>Task</returns>
        public static async Task<Mesh> LoadObj(string file)
        {
            if (file.ToLower().EndsWith(".obj") == false)
                throw new InvalidOperationException("file name does not end with '.obj'");

            var vertices = new List<Vector3>();
            var textures = new List<Vector2>();
            var normals = new List<Vector3>();
            var indices = new List<ObjIndex>();

            var rawObj = File.ReadAllText(file).Split('\n');
            foreach (var line in rawObj)
            {
                if (line.StartsWith("v "))
                {
                    var match = Regex.Match(line, @"v ([0-9\.\-]+) ([0-9\.\-]+) ([0-9\.\-]+)");
                    if (match.Success)
                    {
                        vertices.Add(new System.Numerics.Vector3(
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
                        textures.Add(new System.Numerics.Vector2(
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
                        normals.Add(new System.Numerics.Vector3(
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
                        indices.Add(new ObjIndex
                        {
                            Vertex = Convert.ToInt32(match.Groups[1].Value) - 1,
                            Texture = Convert.ToInt32(match.Groups[2].Value) - 1,
                            Normal = Convert.ToInt32(match.Groups[3].Value) - 1
                        });
                        indices.Add(new ObjIndex
                        {
                            Vertex = Convert.ToInt32(match.Groups[4].Value) - 1,
                            Texture = Convert.ToInt32(match.Groups[5].Value) - 1,
                            Normal = Convert.ToInt32(match.Groups[6].Value) - 1
                        });
                        indices.Add(new ObjIndex
                        {
                            Vertex = Convert.ToInt32(match.Groups[7].Value) - 1,
                            Texture = Convert.ToInt32(match.Groups[8].Value) - 1,
                            Normal = Convert.ToInt32(match.Groups[9].Value) - 1
                        });
                    }
                }
            }

            //set indices and vertices to obj indices and vertices
            var mesh = new Mesh();
            mesh.Indices = indices.Select((i, index) => (ushort)index).ToArray();
            mesh.Vertices = indices.Select(i => new Vertex
            {
                Position = vertices[i.Vertex],
                TextureCoordinates = textures[i.Texture],
                Normal = normals[i.Normal]
            }).ToArray();

            //calculate tangent and bi-tangent
            mesh.ReCalculateNormals();
            //update mesh buffers
            await mesh.UpdateBuffers();
            return mesh;
        }
    }
}