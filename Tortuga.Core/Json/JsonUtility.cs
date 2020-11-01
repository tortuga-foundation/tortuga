using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace Tortuga.Core.Json
{
    /// <summary>
    /// Used to import custom type of json
    /// </summary>
    public readonly struct Custom { }

    /// <summary>
    /// Responsible for converting json object to common data types
    /// </summary>
    public static class JsonUtility
    {
        /// <summary>
        /// Data converters for different types of objects
        /// </summary>
        public static Dictionary<string, KeyValuePair<Type, Func<JsonElement, object>>> DataConverter
        = new Dictionary<string, KeyValuePair<Type, Func<JsonElement, object>>>();

        /// <summary>
        /// Convert a json element to common data type
        /// </summary>
        /// <param name="element">json element</param>
        /// <param name="dataType">returns what type of object this is</param>
        /// <param name="data">returns the converted object</param>
        /// <returns>true if manage to convert json to object</returns>
        public static bool ToObject(
            this JsonElement element,
            out Type dataType,
            out object data
        )
        {
            dataType = null;
            data = null;
            try
            {

                //try to get json data type
                if (element.TryGetProperty(
                    "Type",
                    out JsonElement typeJsonElement
                ) == false)
                    return false;

                //check if data type is supported
                var typeInString = typeJsonElement.GetString();
                if (DataConverter.ContainsKey(typeInString) == false)
                    throw new Exception("unknown data type was passed from json");

                //get data type as 'Type' object
                dataType = DataConverter[typeInString].Key;

                //try to get content of the data
                if (element.TryGetProperty(
                    "Data",
                    out JsonElement jsonData
                ) == false)
                    return false;

                //use mapper function to map the data
                data = DataConverter[typeInString].Value.Invoke(jsonData);
                //return true
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void InitDataTypes()
        {
            //int
            Core.Json.JsonUtility.DataConverter.Add(
                "Int32",
                new KeyValuePair<System.Type, System.Func<JsonElement, object>>(
                    typeof(int),
                    (JsonElement element) => element.GetInt32()
                )
            );

            //float
            Core.Json.JsonUtility.DataConverter.Add(
                "Float",
                new KeyValuePair<System.Type, System.Func<JsonElement, object>>(
                    typeof(float),
                    (JsonElement element) => element.GetSingle()
                )
            );

            //int array
            Core.Json.JsonUtility.DataConverter.Add(
                "Int32Array",
                new KeyValuePair<System.Type, System.Func<JsonElement, object>>(
                    typeof(int[]),
                    (JsonElement element) => (
                        element.EnumerateArray()
                        .Select(e => e.GetInt32())
                        .ToArray()
                    )
                )
            );

            //float array
            Core.Json.JsonUtility.DataConverter.Add(
                "FloatArray",
                new KeyValuePair<System.Type, System.Func<JsonElement, object>>(
                    typeof(float[]),
                    (JsonElement element) => (
                        element.EnumerateArray()
                        .Select(e => e.GetSingle())
                        .ToArray()
                    )
                )
            );

            //vector2
            Core.Json.JsonUtility.DataConverter.Add(
                "Vector2",
                new KeyValuePair<System.Type, System.Func<JsonElement, object>>(
                    typeof(Vector2),
                    (JsonElement element) => (
                        new Vector2(
                            element.GetProperty("X").GetSingle(),
                            element.GetProperty("Y").GetSingle()
                        )
                    )
                )
            );

            //vector3
            Core.Json.JsonUtility.DataConverter.Add(
                "Vector3",
                new KeyValuePair<System.Type, System.Func<JsonElement, object>>(
                    typeof(Vector3),
                    (JsonElement element) => (
                        new Vector3(
                            element.GetProperty("X").GetSingle(),
                            element.GetProperty("Y").GetSingle(),
                            element.GetProperty("Z").GetSingle()
                        )
                    )
                )
            );

            //vector4
            Core.Json.JsonUtility.DataConverter.Add(
                "Vector4",
                new KeyValuePair<System.Type, System.Func<JsonElement, object>>(
                    typeof(Vector4),
                    (JsonElement element) => (
                        new Vector4(
                            element.GetProperty("X").GetSingle(),
                            element.GetProperty("Y").GetSingle(),
                            element.GetProperty("Z").GetSingle(),
                            element.GetProperty("W").GetSingle()
                        )
                    )
                )
            );

            //matrix4x4
            Core.Json.JsonUtility.DataConverter.Add(
                "Matrix4x4",
                new KeyValuePair<System.Type, System.Func<JsonElement, object>>(
                    typeof(Matrix4x4),
                    (JsonElement element) => (
                        new Matrix4x4(
                            element.GetProperty("M11").GetSingle(),
                            element.GetProperty("M12").GetSingle(),
                            element.GetProperty("M13").GetSingle(),
                            element.GetProperty("M14").GetSingle(),
                            element.GetProperty("M21").GetSingle(),
                            element.GetProperty("M22").GetSingle(),
                            element.GetProperty("M23").GetSingle(),
                            element.GetProperty("M24").GetSingle(),
                            element.GetProperty("M31").GetSingle(),
                            element.GetProperty("M32").GetSingle(),
                            element.GetProperty("M33").GetSingle(),
                            element.GetProperty("M34").GetSingle(),
                            element.GetProperty("M41").GetSingle(),
                            element.GetProperty("M42").GetSingle(),
                            element.GetProperty("M43").GetSingle(),
                            element.GetProperty("M44").GetSingle()
                        )
                    )
                )
            );

            //custom
            Core.Json.JsonUtility.DataConverter.Add(
                "Custom",
                new KeyValuePair<System.Type, System.Func<JsonElement, object>>(
                    typeof(Core.Json.Custom),
                    (JsonElement element) => element
                )
            );
        }
    }
}