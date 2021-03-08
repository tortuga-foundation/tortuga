using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Tortuga.Core
{
    public static class JsonUtility
    {
        /// <summary>
        /// Takes a type object and json element. Then converts the json element to that type object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public delegate object DefaultPropertyParser(Type type, JsonElement element);
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public static Dictionary<string, DefaultPropertyParser> DefaultProperties = new Dictionary<string, DefaultPropertyParser>
        {
            {
                typeof(Int32).Name,
                (Type t, JsonElement el) =>
                {
                    var data = new List<Int32>();
                    foreach (var d in el.EnumerateArray())
                        data.Add(d.GetInt32());
                    return data;
                }
            },
            {
                typeof(string).Name,
                (Type t, JsonElement el) =>
                {
                    return el.GetString();
                }
            },
            {
                typeof(float).Name,
                (Type t, JsonElement el) =>
                {
                    var data = new List<float>();
                    foreach(var d in el.EnumerateArray())
                        data.Add(d.GetSingle());
                    return data;
                }
            },
            {
                typeof(object).Name,
                (Type t, JsonElement el) =>
                {
                    var typeString = el.GetProperty("Type").GetString();
                    var valueObject = el.GetProperty("Value");
                    return DefaultProperties[typeString].Invoke(t, valueObject);
                }
            }
        };

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T JsonToDataType<T>(string rawJson) where T : class
        {
            var element = JsonSerializer.Deserialize<JsonElement>(rawJson);
            return JsonToDataType<T>(element);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T JsonToDataType<T>(JsonElement element) where T : class
        {
            var type = typeof(T);
            return JsonToDataType(element, type) as T;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object JsonToDataType(JsonElement element, Type type)
        {
            //error handling
            if (element.TryGetProperty("Type", out JsonElement jsonType) == false)
                throw new JsonException("json does not contain type property");
            if (element.TryGetProperty("Value", out JsonElement jsonValue) == false)
                throw new JsonException("json does not contain value property");
            if (type.Name != jsonType.GetString())
                throw new InvalidOperationException("json type does not match the type provided");

            var data = Activator.CreateInstance(type);
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (jsonValue.TryGetProperty(property.Name, out JsonElement el) == false)
                    continue;

                var propertyType = property.PropertyType;
                if (DefaultProperties.ContainsKey(propertyType.Name))
                {
                    property.SetValue(
                        data,
                        DefaultProperties[propertyType.Name].Invoke(propertyType, el)
                    );
                }
                else if (propertyType.IsGenericType)
                {
                    var arrayObject = (System.Collections.IList)Activator.CreateInstance(propertyType);
                    var arrayElType = propertyType.GenericTypeArguments.First();
                    foreach (var arrayEl in el.EnumerateArray())
                        arrayObject.Add(JsonToDataType(arrayEl, arrayElType));
                    property.SetValue(data, arrayObject);
                }
                else
                {
                    property.SetValue(
                        data,
                        JsonToDataType(el, property.GetType())
                    );
                }
            }
            return data;
        }
    }
}