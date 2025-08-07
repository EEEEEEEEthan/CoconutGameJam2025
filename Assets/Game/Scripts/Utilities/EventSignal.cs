using System;
using System.Runtime.CompilerServices;
namespace Game.Utilities
{
	/// <summary>
	///     一次性发布的事件。隐式转换为bool，表示是否已经发布。
	/// </summary>
	public class EventSignal
	{
		public readonly struct Awaiter : INotifyCompletion
		{
			readonly EventSignal awaitable;
			public bool IsCompleted => awaitable.completed;
			internal Awaiter(EventSignal awaitable) => this.awaitable = awaitable;
			public void OnCompleted(Action continuation)
			{
				if (awaitable.completed)
					continuation.TryInvoke();
				else
					awaitable.OnContinuation += continuation;
			}
			public void GetResult() { }
		}
		public sealed class Handle : EventSignal
		{
			public void Publish()
			{
				if (completed) throw new InvalidOperationException("Already completed");
				completed = true;
				var continuation = OnContinuation;
				OnContinuation = null;
				continuation?.TryInvoke();
			}
		}
		public static implicit operator bool(EventSignal signal) => signal.completed;
		bool completed;
		public event Action OnCompleted
		{
			add
			{
				if (completed)
					value.TryInvoke();
				else
					OnContinuation += value;
			}
			remove => OnContinuation -= value;
		}
		event Action OnContinuation;
		public Awaiter GetAwaiter() => new(this);
	}
}
