using System.Text.RegularExpressions;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		public static bool TryMatch(this Regex patten, string input, out Match match)
		{
			match = patten.Match(input);
			return match.Success;
		}
	}
}
