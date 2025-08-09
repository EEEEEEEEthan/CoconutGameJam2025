#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using Game.Utilities;
using Game.Utilities.Pools;
using UnityEditor;
namespace Game.ResourceManagement
{
	static class ResourceTableGenerator
	{
		static readonly Regex regex = new(@"[^0-9a-zA-Z_]+", RegexOptions.Compiled);
		static string GetFieldName(string fileNameWithExtension)
		{
			var extension = Path.GetExtension(fileNameWithExtension);
			var fieldName = fileNameWithExtension[..^extension.Length] + extension[1..].ToUpperCamelCase();
			fieldName = regex.Replace(fieldName, "");
			while (fieldName.Length > 0 && char.IsDigit(fieldName[0])) fieldName = fieldName[1..];
			if (fieldName.Length <= 0) fieldName = "unnamedResource";
			if (char.IsUpper(fieldName[0])) fieldName = char.ToLowerInvariant(fieldName[0]) + fieldName[1..];
			return fieldName;
		}
		[MenuItem("*Game*/ResGenerate Resource Table", priority = 100)]
		static void Generate()
		{
			var fields = typeof(ResourceTable).GetFields((System.Reflection.BindingFlags)0xffff);
			using var _ = DictionaryPoolThreaded<string, Loader>.Rent(out var name2Loader);
			using var __ = HashSetPoolThreaded<string>.Rent(out var fieldNames);
			using var ___ = HashSetPoolThreaded<string>.Rent(out var containedGuids);
			foreach (var field in fields)
				if (typeof(Loader).IsAssignableFrom(field.FieldType))
				{
					var fieldValue = (Loader)field.GetValue(null);
					if (fieldValue.Resources.Count <= 0) continue;
					fieldNames.Add(field.Name);
					containedGuids.Add(fieldValue.guid);
					name2Loader[field.Name] = fieldValue;
				}
			const string folder = "Assets/Game/Resources/";
			var guids = AssetDatabase.FindAssets("", new[] { folder, });
			foreach (var guid in guids)
			{
				if (!containedGuids.Add(guid)) continue;
				var path = AssetDatabase.GUIDToAssetPath(guid);
				if (Directory.Exists(path)) continue;
				var relativePathWithExtension = path[folder.Length..];
				var extension = Path.GetExtension(relativePathWithExtension);
				var fieNameWithExtension = Path.GetFileName(path);
				var fieldName = GetFieldName(fieNameWithExtension);
				var copiedFieldName = fieldName;
				var i = 0;
				var resourceLoader = new Loader(relativePathWithExtension[..^extension.Length], guid);
				while (!fieldNames.Add(fieldName)) fieldName = $"{copiedFieldName}{++i}";
				name2Loader[fieldName] = resourceLoader;
			}
			using var ____ = ListPoolThreaded<(string path, string code)>.Rent(out var resources);
			foreach (var (name, loader) in name2Loader)
			{
				var path = loader.pathWithoutExtension;
				var type = loader.Resources[0].GetType();
				var code = $"public static readonly {nameof(Loader)}<{type.Name}> {name} = new(\"{path}\", \"{loader.guid}\");";
				resources.Add((path, code));
			}
			resources.Sort();
			using var _____ = StringBuilderPoolThreaded.Rent(out var builder);
			foreach (var (_, code) in resources) builder.AppendLine("\t\t" + code);
			var text = File.ReadAllText("Assets/Game/Scripts/ResourceManagement/ResourceTable.cs");
			const string startMark = "#region autogen";
			const string endMark = "#endregion autogen";
			var prefix = text[..(text.IndexOf(startMark, StringComparison.Ordinal) + startMark.Length)];
			var suffix = text[text.IndexOf(endMark, StringComparison.Ordinal)..];
			var newText = $"{prefix}\n{builder}\t\t{suffix}";
			File.WriteAllText("Assets/Game/Scripts/ResourceManagement/ResourceTable.cs", newText);
			AssetDatabase.Refresh();
		}
	}
}
#endif
