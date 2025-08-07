using System;
using System.Runtime.CompilerServices;
namespace Game.Utilities
{
	readonly struct CallbackItem : ICallbackItem
	{
		readonly Action action;
		public CallbackItem(Action action) => this.action = action;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invoke() => action();
	}
	readonly struct CallbackItem<T> : ICallbackItem<T>
	{
		readonly Action<T> action;
		public CallbackItem(Action<T> action) => this.action = action;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invoke(T arg) => action(arg);
	}
}
