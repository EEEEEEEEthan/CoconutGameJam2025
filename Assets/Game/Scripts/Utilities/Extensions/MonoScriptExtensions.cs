#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static void Replace(this MonoScript @this, string beginMark, string endMark, string replacement)
		{
			var path = AssetDatabase.GetAssetPath(@this);
			var content = File.ReadAllText(path);
			var beginIndex = content.IndexOf(beginMark, StringComparison.Ordinal);
			if (beginIndex < 0)
			{
				Debug.LogError($"Begin mark {beginMark} not found in {path}");
				return;
			}
			var lastLine = content.LastIndexOf('\n', beginIndex);
			var indent = content[lastLine..beginIndex].Count(static c => c == '\t');
			Replace(@this, beginMark, endMark, replacement, indent);
		}
		public static void Replace(
			this MonoScript @this,
			string beginMark,
			string endMark,
			string replacement,
			int indent)
		{
			var path = AssetDatabase.GetAssetPath(@this);
			var content = File.ReadAllText(path);
			var beginIndex = content.IndexOf(beginMark, StringComparison.Ordinal);
			var endIndex = content.IndexOf(endMark, StringComparison.Ordinal);
			if (beginIndex < 0)
			{
				Debug.LogError($"Begin mark {beginMark} not found in {path}");
				return;
			}
			if (endIndex < 0)
			{
				Debug.LogError($"End mark {endMark} not found in {path}");
				return;
			}
			var builder = new StringBuilder();
			builder.AppendLine(content[..(beginIndex + beginMark.Length)]);
			var indentText = new string('\t', indent);
			foreach (var line in replacement.Split('\n')) builder.AppendLine(indentText + line.TrimEnd());
			builder.Append(indentText);
			builder.Append(content[endIndex..]);
			File.WriteAllText(path, builder.ToString());
			AssetDatabase.ImportAsset(path);
		}
		public static void InsertLine(this MonoScript @this, string mark, string insert)
		{
			var path = AssetDatabase.GetAssetPath(@this);
			var text = @this.text;
			var match = Regex.Match(text, @"\n(\s+)" + $"({mark})");
			if (match.Success)
			{
				var indent = match.Groups[1].ToString();
				text = text.Insert(match.Groups[2].Index, insert + "\r\n" + indent);
				File.WriteAllText(path, text);
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.LogError($"Mark {mark} not found in {path}");
			}
		}
	}
}
#endif
