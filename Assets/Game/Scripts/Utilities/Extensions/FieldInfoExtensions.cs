using System.Reflection;
using JetBrains.Annotations;
namespace Game.Utilities
{
	public static partial class Extensions
	{
		[NotNull] public static T GetValue<T>(this FieldInfo @this, object obj) => (T)@this.GetValue(obj);
	}
}
