using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vulkan;

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
        /// <summary>
        /// Used for normal mapping
        /// </summary>
        public Vector3 Tangent;
        /// <summary>
        /// Used for normal mapping
        /// </summary>
        public Vector3 BiTangent;
    }


    /// <summary>
    /// Contains the mesh data
    /// </summary>
    public class Mesh
    {
        internal Graphics.API.Buffer VertexBuffer => _vertexBuffers;
        internal Graphics.API.Buffer IndexBuffer => _indexBuffers;
        private Graphics.API.Buffer _vertexBuffers;
        private Graphics.API.Buffer _indexBuffers;

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
        public static async Task<Mesh> Load(string file)
        {
            if (File.Exists(file) == false)
                throw new FileNotFoundException();

            if (file.ToLower().EndsWith(".obj"))
            {
                var mesh = LoadOBJ(file);
                mesh.RecalculateNormals();
                await mesh.UpdateBuffers();
                return mesh;
            }
            else
                throw new NotSupportedException("this type of file is not currently supported");
        }

        /// <summary>
        /// rebuilds the vertex and index buffers for gpu
        /// </summary>
        public async Task UpdateBuffers(bool force = false)
        {
            var expectedIndexSize = Convert.ToUInt32(sizeof(ushort) * this.Indices.Length);
            var expectedVertexSize = Convert.ToUInt32(Unsafe.SizeOf<Vertex>() * this.Vertices.Length);

            if (_indexBuffers == null || _indexBuffers.Size != expectedIndexSize || force)
            {
                _indexBuffers = Graphics.API.Buffer.CreateDevice(
                    API.Handler.MainDevice,
                    expectedIndexSize,
                    VkBufferUsageFlags.IndexBuffer
                );
            }
            var indexTask = _indexBuffers.SetDataWithStaging(this.Indices);
            if (_vertexBuffers == null || _vertexBuffers.Size != expectedVertexSize || force)
            {
                _vertexBuffers = Graphics.API.Buffer.CreateDevice(
                    API.Handler.MainDevice,
                    expectedVertexSize,
                    VkBufferUsageFlags.VertexBuffer
                );
            }
            var vertexTask = _vertexBuffers.SetDataWithStaging(this.Vertices);
            await indexTask;
            await vertexTask;
        }

        /// <summary>
        /// Destroys the mesh data from memory and mesh buffer information from gpu
        /// </summary>
        public void Dispose()
        {
            _indexBuffers = null;
            _vertexBuffers = null;
            this.Vertices = new Vertex[0];
            this.Indices = new ushort[0];
        }

        /// <summary>
        /// De-constructor for mesh
        /// </summary> 
        ~Mesh()
        {
            this.Dispose();
        }

        /// <summary>
        /// calculates tangents and bi-tangents for normal mapping
        /// </summary>
        public void RecalculateNormals()
        {
            for (int i = 0; i < this.Indices.Length; i += 3)
            {
                var v0 = this.Vertices[this.Indices[i + 0]];
                var v1 = this.Vertices[this.Indices[i + 1]];
                var v2 = this.Vertices[this.Indices[i + 2]];

                var deltaPos1 = v1.Position - v0.Position;
                var deltaPos2 = v2.Position - v0.Position;

                var deltaUV1 = v1.TextureCoordinates - v0.TextureCoordinates;
                var deltaUV2 = v2.TextureCoordinates - v0.TextureCoordinates;

                float r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
                var tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;
                var biTangent = (deltaPos2 * deltaUV1.X - deltaPos1 * deltaUV2.X) * r;

                this.Vertices[this.Indices[i + 0]].Tangent = tangent;
                this.Vertices[this.Indices[i + 1]].Tangent = tangent;
                this.Vertices[this.Indices[i + 2]].Tangent = tangent;

                this.Vertices[this.Indices[i + 0]].BiTangent = biTangent;
                this.Vertices[this.Indices[i + 1]].BiTangent = biTangent;
                this.Vertices[this.Indices[i + 2]].BiTangent = biTangent;
            }
        }
    }
}