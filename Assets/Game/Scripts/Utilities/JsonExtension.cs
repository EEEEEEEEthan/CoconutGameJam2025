using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace Game.Utilities
{
	sealed class Vector2IntConverter : JsonConverter<Vector2Int>
	{
		public static readonly Vector2IntConverter instance = new();
		Vector2IntConverter() { }
		public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(value.x);
			writer.WritePropertyName("y");
			writer.WriteValue(value.y);
			writer.WriteEndObject();
		}
		public override Vector2Int ReadJson(
			JsonReader reader,
			Type objectType,
			Vector2Int existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var obj = JObject.Load(reader);
			return Vector2IntExtensions.Create(obj["x"].Value<int>(), obj["y"].Value<int>());
		}
	}
	sealed class Vector2Converter : JsonConverter<Vector2>
	{
		public static readonly Vector2Converter instance = new();
		Vector2Converter() { }
		public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("x");
			writer.WriteValue(value.x);
			writer.WritePropertyName("y");
			writer.WriteValue(value.y);
			writer.WriteEndObject();
		}
		public override Vector2 ReadJson(
			JsonReader reader,
			Type objectType,
			Vector2 existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var obj = JObject.Load(reader);
			return new(obj["x"].Value<int>(), obj["y"].Value<int>());
		}
	}
}
