using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public readonly struct ReaderBlock : IDisposable
		{
			readonly BinaryReader reader;
			readonly long endPosition;
			public ReaderBlock(BinaryReader reader)
			{
				this.reader = reader;
				int length;
				try
				{
					length = reader.ReadInt32();
				}
				catch (EndOfStreamException)
				{
					Debug.LogError("检查之前的读逻辑有没有问题");
					throw;
				}
				endPosition = reader.BaseStream.Position + length;
			}
			public void Dispose() => reader.BaseStream.Position = endPosition;
		}
		public readonly struct WriterBlock : IDisposable
		{
			readonly BinaryWriter writer;
			readonly long beginPosition;
			public WriterBlock(BinaryWriter writer)
			{
				this.writer = writer;
				beginPosition = writer.BaseStream.Position;
				writer.Write(0);
			}
			public void Dispose()
			{
				var currentPosition = writer.BaseStream.Position;
				writer.BaseStream.Position = beginPosition;
				var length = (int)(currentPosition - beginPosition - sizeof(int));
				writer.Write(length);
				writer.BaseStream.Position = currentPosition;
			}
		}
		public static ReaderBlock ReadScope(this BinaryReader @this) => new(@this);
		public static ReaderBlock ReadScope(this BinaryReader @this, string key)
		{
			var disposable = new ReaderBlock(@this);
			var readKey = @this.ReadInt32();
			if (key.GetHashCode() != readKey) throw new($"Key mismatch: expected:{key}, got:{readKey}");
			return disposable;
		}
		public static ReaderBlock ReadScope(this BinaryReader @this, int key)
		{
			var disposable = new ReaderBlock(@this);
			var k = @this.ReadInt32();
			if (key != k) throw new($"Key mismatch: expected:{key}, got:{k}");
			return disposable;
		}
		public static WriterBlock WriteScope(this BinaryWriter @this) => new(@this);
		public static WriterBlock WriteScope(this BinaryWriter @this, string key)
		{
			var disposable = new WriterBlock(@this);
			@this.Write(key.GetHashCode());
			return disposable;
		}
		public static WriterBlock WriteScope(this BinaryWriter @this, int key)
		{
			var disposable = new WriterBlock(@this);
			@this.Write(key);
			return disposable;
		}
		public static void Write(this BinaryWriter @this, Vector2 value)
		{
			@this.Write(value.x);
			@this.Write(value.y);
		}
		public static Vector2 ReadVector2(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle());
		public static void Write(this BinaryWriter @this, Vector2Int value)
		{
			@this.Write(value.x);
			@this.Write(value.y);
		}
		public static Vector2Int ReadVector2Int(this BinaryReader @this) => Vector2IntExtensions.Create(@this.ReadInt32(), @this.ReadInt32());
		public static void Write(this BinaryWriter @this, Color32 value)
		{
			@this.Write(value.r);
			@this.Write(value.g);
			@this.Write(value.b);
			@this.Write(value.a);
		}
		public static Color32 ReadColor32(this BinaryReader @this) => new(@this.ReadByte(), @this.ReadByte(), @this.ReadByte(), @this.ReadByte());
		public static void Write(this BinaryWriter @this, DateTime value)
		{
			@this.Write((byte)value.Kind);
			@this.Write(value.Ticks);
		}
		public static DateTime ReadDateTime(this BinaryReader @this)
		{
			var kind = (DateTimeKind)@this.ReadByte();
			switch (kind)
			{
				case DateTimeKind.Unspecified:
				case DateTimeKind.Utc:
				case DateTimeKind.Local:
					break;
				default:
					throw new InvalidDataException($"Invalid DateTimeKind: {kind}");
			}
			var ticks = @this.ReadInt64();
			return new(ticks, kind);
		}
		public static void Write(this BinaryWriter @this, Dictionary<int, int> dictionary)
		{
			@this.Write(dictionary.Count);
			foreach (var (key, value) in dictionary)
			{
				@this.Write(key);
				@this.Write(value);
			}
		}
		public static Dictionary<int, int> ReadDictionaryIntInt(this BinaryReader @this)
		{
			var dictionary = new Dictionary<int, int>();
			@this.ReadDictionaryIntInt(dictionary);
			return dictionary;
		}
		public static void ReadDictionaryIntInt(this BinaryReader @this, Dictionary<int, int> dictionary)
		{
			var count = @this.ReadInt32();
			for (var i = 0; i < count; i++) dictionary.Add(@this.ReadInt32(), @this.ReadInt32());
		}
	}
}
