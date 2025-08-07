using System;
using System.Runtime.CompilerServices;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.Utilities
{
	public sealed class Awaitable
	{
		public readonly struct Awaiter : INotifyCompletion
		{
			readonly Awaitable awaitable;
			readonly uint lifeSpanId;

			// ReSharper disable once InconsistentlySynchronizedField
			public bool IsCompleted => awaitable.completed;
			internal Awaiter(Awaitable awaitable)
			{
				this.awaitable = awaitable;
				lifeSpanId = awaitable.lifeSpanId;
			}
			public void OnCompleted(Action continuation)
			{
				lock (awaitable)
				{
					if (lifeSpanId != awaitable.lifeSpanId)
					{
						Debug.LogError("flag changed!");
						return;
					}
					awaitable.Await(continuation);
				}
			}
			public void GetResult() { }
		}
		public readonly struct Handle
		{
			readonly Awaitable awaitable;
			readonly uint lifeSpanId;
			internal Handle(Awaitable awaitable)
			{
				this.awaitable = awaitable;
				lifeSpanId = awaitable.lifeSpanId;
			}
			public void Set()
			{
				lock (awaitable)
				{
					if (lifeSpanId != awaitable.lifeSpanId)
					{
						Debug.LogError("flag changed!");
						return;
					}
					awaitable.SetResult();
				}
			}
		}
		static readonly ObjectPool<Awaitable> threadedPool = new(create: () => new());
		public static Awaitable Create(out Handle handle)
		{
			threadedPool.Rent(out var awaitable);
			lock (awaitable)
			{
				++awaitable.lifeSpanId;
				awaitable.completed = false;
				handle = new(awaitable);
				return awaitable;
			}
		}
		bool completed;
		uint lifeSpanId;
		Action continuation;
		bool Awaiting => continuation != null;
		Awaitable() { }
		public Awaiter GetAwaiter()
		{
			lock (this)
			{
				return new(this);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Trigger()
		{
			var continuation = this.continuation;
			this.continuation = null;
			continuation?.TryInvoke();
			threadedPool.Return(this);
		}
		void Await(Action continuation)
		{
			this.continuation = continuation;
			if (completed && Awaiting) Trigger();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SetResult()
		{
			completed = true;
			if (completed && Awaiting) Trigger();
		}
	}
	public sealed class Awaitable<T>
	{
		public readonly struct Awaiter : INotifyCompletion
		{
			readonly Awaitable<T> awaitable;
			readonly uint lifeSpanId;
			public bool IsCompleted => awaitable.completed;
			internal Awaiter(Awaitable<T> awaitable)
			{
				this.awaitable = awaitable;
				lifeSpanId = awaitable.lifeSpanId;
			}
			public void OnCompleted(Action continuation)
			{
				lock (awaitable)
				{
					if (lifeSpanId != awaitable.lifeSpanId)
					{
						Debug.LogError("flag changed!");
						return;
					}
					awaitable.Await(continuation);
				}
			}
			public T GetResult() => awaitable.result;
		}
		public readonly struct Handle
		{
			readonly Awaitable<T> awaitable;
			readonly uint lifeSpanId;
			internal Handle(Awaitable<T> awaitable)
			{
				this.awaitable = awaitable;
				lifeSpanId = awaitable.lifeSpanId;
			}
			public void Set(T result)
			{
				lock (awaitable)
				{
					if (lifeSpanId != awaitable.lifeSpanId)
					{
						Debug.LogError("flag changed!");
						return;
					}
					awaitable.SetResult(result);
				}
			}
		}
		static readonly ObjectPool<Awaitable<T>> threadedPool = new(() => new());
		public static Awaitable<T> Create(out Handle handle)
		{
			threadedPool.Rent(out var awaitable);
			lock (awaitable)
			{
				++awaitable.lifeSpanId;
				awaitable.completed = false;
				handle = new(awaitable);
				return awaitable;
			}
		}
		bool completed;
		T result;
		uint lifeSpanId;
		Action continuation;
		bool Awaiting => continuation != null;
		Awaitable() { }
		public Awaiter GetAwaiter()
		{
			lock (this)
			{
				return new(this);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Trigger()
		{
			var continuation = this.continuation;
			this.continuation = null;
			continuation?.TryInvoke();
			threadedPool.Return(this);
		}
		void Await(Action continuation)
		{
			this.continuation = continuation;
			if (completed && Awaiting) Trigger();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SetResult(T result)
		{
			completed = true;
			this.result = result;
			if (completed && Awaiting) Trigger();
		}
	}
}
