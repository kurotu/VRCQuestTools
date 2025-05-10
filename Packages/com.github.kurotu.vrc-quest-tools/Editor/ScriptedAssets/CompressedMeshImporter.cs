using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace KRT.VRCQuestTools.ScriptedAssets
{
    [ScriptedImporter(1, "mesh.vqt")]
    internal class CompressedMeshImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            using (var fs = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read))
            using (var gzip = new GZipStream(fs, CompressionMode.Decompress))
            using (var sr = new StreamReader(gzip, Encoding.UTF8))
            {
                var json = sr.ReadToEnd();
                //var serializable = JsonConvert.DeserializeObject<MeshSerializer.SerializableMesh>(json, new FloatConverter());
                var serializable = JsonUtility.FromJson<MeshSerializer.SerializableMesh>(json);
                var mesh = MeshSerializer.GetMesh(serializable);
                ctx.AddObjectToAsset("main", mesh);
                ctx.SetMainObject(mesh);
            }
        }

        public static Mesh SaveAsAsset(Mesh mesh, string path)
        {
            if (!path.EndsWith(".mesh.vqt"))
            {
                throw new ArgumentException("File name must end with \".mesh.vqt\"");
            }

            var serializable = MeshSerializer.GetSerializableMesh(mesh);
            //var json = JsonConvert.SerializeObject(serializable, new FloatConverter(),new Vector2Converter(), new Vector3Converter(), new Vector4Converter(), new QuaternionConverter());
            var json = JsonUtility.ToJson(serializable);

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var gzip = new GZipStream(fs, CompressionMode.Compress))
            using (var sw = new StreamWriter(gzip, Encoding.UTF8))
            {
                sw.Write(json);
            }

            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Mesh>(path);
        }

        private class FloatConverter : JsonConverter<float>
        {
            public override void WriteJson(JsonWriter writer, float value, JsonSerializer serializer)
            {
                JToken token = JToken.FromObject(value);
                if (token.Type == JTokenType.Float)
                {
                    writer.WriteValue(value.ToString("G9"));
                    return;
                }
                writer.WriteValue(value);
            }

            public override float ReadJson(JsonReader reader, Type objectType, float existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return float.Parse((string)reader.Value);
            }
        }

        private class Vector2Converter : JsonConverter<Vector2>
        {
            public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
            {
                writer.WriteStartArray();
                writer.WriteValue(value[0]);
                writer.WriteValue(value[1]);
                writer.WriteEndArray();
            }
            public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                JObject obj = JObject.Load(reader);
                return new Vector2(
                    obj[0].ToObject<float>(),
                    obj[1].ToObject<float>()
                );
            }
        }

        private class Vector3Converter : JsonConverter<Vector3>
        {
            public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
            {
                writer.WriteStartArray();
                writer.WriteValue(value[0]);
                writer.WriteValue(value[1]);
                writer.WriteValue(value[2]);
                writer.WriteEndArray();
            }
            public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                JObject obj = JObject.Load(reader);
                return new Vector3(
                    obj[0].ToObject<float>(),
                    obj[1].ToObject<float>(),
                    obj[2].ToObject<float>()
                );
            }
        }

        private class Vector4Converter : JsonConverter<Vector4>
        {
            public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
            {
                writer.WriteStartArray();
                writer.WriteValue(value[0]);
                writer.WriteValue(value[1]);
                writer.WriteValue(value[2]);
                writer.WriteValue(value[3]);
                writer.WriteEndArray();
            }
            public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                JObject obj = JObject.Load(reader);
                return new Vector4(
                    obj[0].ToObject<float>(),
                    obj[1].ToObject<float>(),
                    obj[2].ToObject<float>(),
                    obj[3].ToObject<float>()
                );
            }
        }

        private class QuaternionConverter : JsonConverter<Quaternion>
        {
            public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
            {
                writer.WriteStartArray();
                writer.WriteValue(value[0]);
                writer.WriteValue(value[1]);
                writer.WriteValue(value[2]);
                writer.WriteValue(value[3]);
                writer.WriteEndArray();
            }
            public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                JObject obj = JObject.Load(reader);
                return new Quaternion(
                    obj[0].ToObject<float>(),
                    obj[1].ToObject<float>(),
                    obj[2].ToObject<float>(),
                    obj[3].ToObject<float>()
                );
            }
        }
    }
}
