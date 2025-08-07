namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static string ToLowerCamelCase(this string value) => char.ToLower(value[0]) + value[1..];
		public static string ToUpperCamelCase(this string value) => char.ToUpper(value[0]) + value[1..];
		public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
	}
}
