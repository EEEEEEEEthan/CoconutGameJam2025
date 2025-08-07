using System;
using System.Runtime.CompilerServices;
using Game.Utilities.Pools;
namespace Game.Utilities
{
	public sealed class ThreadConverter
	{
		public readonly struct Awaiter : INotifyCompletion
		{
			readonly ThreadConverter converter;
			public bool IsCompleted => false;
			internal Awaiter(ThreadConverter converter) => this.converter = converter;
			public void OnCompleted(Action continuation) => converter.Invoke(continuation);
			public void GetResult() { }
		}
		public readonly struct Awaitable
		{
			readonly ThreadConverter converter;
			internal Awaitable(ThreadConverter converter) => this.converter = converter;
			public Awaiter GetAwaiter() => new(converter);
		}
		readonly object locker = new();
		Action[] actions;
		Pooled disposable;
		int count;
		public ThreadConverter() => disposable = ArrayPoolThreaded<Action>.RentWithoutDefaultValue(1, out actions);
		public void Invoke(Action action)
		{
			lock (locker)
			{
				if (count >= actions.Length)
				{
					var newDisposable = ArrayPoolThreaded<Action>.RentWithoutDefaultValue(actions.Length << 1, out var newArray);
					Array.Copy(actions, newArray, actions.Length);
					disposable.Dispose();
					disposable = newDisposable;
					actions = newArray;
				}
				actions[count++] = action;
			}
		}
		public Awaitable Await() => new(this);
		public void Update()
		{
			int count;
			Action[] copy;
			Pooled disposable;
			lock (locker)
			{
				disposable = ArrayPoolThreaded<Action>.RentWithoutDefaultValue(actions.Length, out copy);
				Array.Copy(actions, copy, count = this.count);
				this.count = 0;
			}
			for (var i = 0; i < count; ++i) copy[i].TryInvoke();
			disposable.Dispose();
		}
	}
}
