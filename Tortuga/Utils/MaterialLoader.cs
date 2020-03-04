using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;
using Tortuga.Graphics;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace Tortuga.Utils
{
    public static class MaterialLoader
    {
        private static Dictionary<string, object>[] GetObjectArray(object array)
        {
            var rtn = new List<Dictionary<string, object>>();
            foreach (var item in array as ICollection<object>)
                rtn.Add(item as Dictionary<string, object>);
            return rtn.ToArray();
        }
        private static string[] GetStringArray(object array)
        {
            var rtn = new List<string>();
            foreach (var item in array as ICollection<object>)
                rtn.Add(item as string);
            return rtn.ToArray();
        }
        private static Color GetColor(object data)
        {
            try
            {
                var regex = new Regex(@"[a-zA-Z]{4}[\ ]*\([\ ]*([0-9]+)[\ ]*,[\ ]*([0-9]+)[\ ]*,[\ ]*([0-9]+)[\ ]*,[\ ]*([0-9]+)[\ ]*\)");
                var match = regex.Match(data as string);
                var r = int.Parse(match.Groups[1].Value);
                var g = int.Parse(match.Groups[2].Value);
                var b = int.Parse(match.Groups[3].Value);
                var a = int.Parse(match.Groups[4].Value);
                return Color.FromArgb(a, r, g, b);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return Color.White;
        }
        private static Vector2 GetVector2(object data)
        {
            var reg = new Regex(@"[a-zA-Z0-9]{4}[\ ]*\([\ ]*([0-9\.\-]+)[\ ]*,[\ ]*([0-9\.\-]+)[\ ]*\)");
            var match = reg.Match(data as string);
            var rtn = new Vector2();
            rtn.X = float.Parse(match.Groups[1].ToString());
            rtn.Y = float.Parse(match.Groups[2].ToString());
            return rtn;
        }
        private static Vector4 GetVector4(object data)
        {
            var reg = new Regex(@"[a-zA-Z0-9]{4}[\ ]*\([\ ]*([0-9\.\-]+)[\ ]*,[\ ]*([0-9\.\-]+)[\ ]*,[\ ]*([0-9\.\-]+)[\ ]*,[\ ]*([0-9\.\-]+)[\ ]*\)");
            var match = reg.Match(data as string);
            var rtn = new Vector4();
            rtn.X = float.Parse(match.Groups[1].ToString());
            rtn.Y = float.Parse(match.Groups[2].ToString());
            rtn.Z = float.Parse(match.Groups[3].ToString());
            rtn.W = float.Parse(match.Groups[4].ToString());
            return rtn;
        }
        private static int GetInt(object data)
        {
            return System.Convert.ToInt32(System.Math.Round((double)data));
        }
        private static uint GetUInt(object data)
        {
            return System.Convert.ToUInt32(System.Math.Round((double)data));
        }
        private static float GetFloat(object data)
        {
            return System.Convert.ToSingle((double)data);
        }
        private static float[] VectorToArray(Vector2 data)
        {
            return new float[]{
                data.X, data.Y
            };
        }
        private static float[] VectorToArray(Vector4 data)
        {
            return new float[]{
                data.X, data.Y, data.Z, data.W
            };
        }

        public static async Task<Material> Load(string path)
        {
            if (File.Exists(path) == false)
                throw new FileNotFoundException("could not find materail file");

            var jsonContent = File.ReadAllText(path);
            var obj = Json.JsonParser.FromJson(jsonContent);
            try
            {
                var lighting = (bool)obj["Light"];
                //setup shader
                var shadersJSON = obj["Shaders"] as Dictionary<string, object>;
                var vertexShader = shadersJSON["Vertex"] as string;
                var fragmentShader = shadersJSON["Fragment"] as string;
                var shader = Shader.Load(vertexShader, fragmentShader);

                //create new material
                var material = new Material(shader, lighting);
                //setup material descriptor sets
                var setsJSON = obj["DescriptorSets"] as ICollection<object>;
                foreach (var setRawJSON in setsJSON)
                {
                    var setJSON = setRawJSON as Dictionary<string, object>;
                    var type = setJSON["Type"] as string;
                    var name = setJSON["Name"] as string;
                    var bindings = GetObjectArray(setJSON["Bindings"] as ICollection<object>);

                    if (type == "UniformData")
                    {
                        var totalBytes = new List<byte[]>();
                        var byteSizes = new List<uint>();
                        foreach (var binding in bindings)
                        {
                            var dataValues = GetObjectArray(binding["Values"] as ICollection<object>);
                            var byteData = new List<byte>();
                            foreach (var data in dataValues)
                            {
                                var dataType = data["Type"] as string;
                                if (dataType == "Int")
                                {
                                    var bytes = System.BitConverter.GetBytes(GetInt(data["Value"]));
                                    foreach (var b in bytes)
                                        byteData.Add(b);
                                }
                                else if (dataType == "Float")
                                {
                                    var bytes = System.BitConverter.GetBytes(GetFloat(data["Value"]));
                                    foreach (var b in bytes)
                                        byteData.Add(b);
                                }
                                else if (dataType == "Vec2")
                                {
                                    var dataArr = VectorToArray(GetVector2(data["Value"]));
                                    foreach (var num in dataArr)
                                    {
                                        var bytes = System.BitConverter.GetBytes(num);
                                        foreach (var b in bytes)
                                            byteData.Add(b);
                                    }
                                }
                                else if (dataType == "Vec4")
                                {
                                    var dataArr = VectorToArray(GetVector4(data["Value"]));
                                    foreach (var num in dataArr)
                                    {
                                        var bytes = System.BitConverter.GetBytes(num);
                                        foreach (var b in bytes)
                                            byteData.Add(b);
                                    }
                                }
                            }
                            totalBytes.Add(byteData.ToArray());
                            byteSizes.Add(Convert.ToUInt32(byteData.Count * sizeof(byte)));
                        }

                        material.CreateUniformData(name, byteSizes.ToArray());
                        var tasks = new Task[bindings.Length];
                        for (int i = 0; i < bindings.Length; i++)
                            tasks[i] = material.UpdateUniformDataArray<byte>(name, i, totalBytes[i]);
                        Task.WaitAll(tasks);
                    }
                    else if (type == "SampledImage2D")
                    {
                        var mipLevels = new uint[bindings.Length];
                        for (int i = 0; i < bindings.Length; i++)
                            mipLevels[i] = Convert.ToUInt32(Math.Round((double)bindings[i]["MipLevel"]));

                        material.CreateSampledImage(name, mipLevels);

                        for (int i = 0; i < bindings.Length; i++)
                        {
                            var binding = bindings[i];
                            var stringValue = binding["Value"] as string;
                            if (stringValue != null)
                            {
                                if (File.Exists(stringValue))
                                {
                                    await material.UpdateSampledImage(
                                        name,
                                        i,
                                        await ImageLoader.Load(stringValue)
                                    );
                                }
                                else
                                {
                                    var color = GetColor(stringValue);
                                    await material.UpdateSampledImage(
                                        name,
                                        i,
                                        Graphics.Image.SingleColor(color)
                                    );
                                }
                            }
                            else
                            {
                                var multiImage = GetStringArray(binding["Value"] as ICollection<object>);
                                if (multiImage.Length <= 4 && multiImage.Length > 0)
                                {
                                    var R = await ImageLoader.Load(multiImage[0]);
                                    if (multiImage.Length > 1)
                                    {
                                        var G = await ImageLoader.Load(multiImage[1]);
                                        R.CopyChannel(G, Graphics.Image.Channel.G);
                                    }
                                    if (multiImage.Length > 2)
                                    {
                                        var B = await ImageLoader.Load(multiImage[2]);
                                        R.CopyChannel(B, Graphics.Image.Channel.B);
                                    }
                                    if (multiImage.Length > 3)
                                    {
                                        var A = await ImageLoader.Load(multiImage[3]);
                                        R.CopyChannel(A, Graphics.Image.Channel.A);
                                    }
                                    await material.UpdateSampledImage(
                                        name,
                                        i,
                                        R
                                    );
                                }
                                else
                                {
                                    await material.UpdateSampledImage(
                                        name,
                                        i,
                                        Graphics.Image.SingleColor(Color.Black)
                                    );
                                }
                            }
                        }
                    }
                }
                return material;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
            return Material.ErrorMaterial;
        }
    }
}