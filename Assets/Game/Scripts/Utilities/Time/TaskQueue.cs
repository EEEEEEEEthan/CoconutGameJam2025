using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Utilities.Pools;
using JetBrains.Annotations;
using UnityEngine;
namespace Game.Utilities.Time
{
	public sealed class TaskQueue
	{
		public static bool enabled = true;
		readonly Queue<Action> queue = new();
		float threshold = 0.1f;
		TimeSpan frameTime = TimeSpan.FromMilliseconds(16.666666666666666666666666666667);
		DateTime frameBegin;
		public float Threshold
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => threshold;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				if (value is <= 0 or >= 1) throw new ArgumentOutOfRangeException();
				threshold = value;
			}
		}
		public TimeSpan FrameTime
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => frameTime;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				if (value.Ticks < 0) throw new ArgumentOutOfRangeException();
				frameTime = value;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update()
		{
			if (enabled)
				UpdateWhileEnabled();
			else
				UpdateWhileDisabled();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue([NotNull] Action action)
		{
			lock (queue)
			{
				queue.Enqueue(action);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void UpdateWhileEnabled()
		{
			frameBegin = DateTime.Now;
			var endTime = frameBegin + frameTime;
			// ReSharper disable once InconsistentlySynchronizedField
			while (queue.Count > 0)
			{
				// ReSharper disable once InconsistentlySynchronizedField
				var min = (queue.Count * threshold).CeilToInt();
				for (var i = 0; i < min; ++i)
				{
					Action action;
					lock (queue)
					{
						if (!queue.TryDequeue(out action)) return;
					}
					try
					{
						action.TryInvoke();
					}
					catch (Exception e)
					{
						Debug.LogException(e);
					}
				}
				var now = DateTime.Now;
				if (now < frameBegin || now > endTime) return;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void UpdateWhileDisabled()
		{
			Pooled disposable;
			Action[] actions;
			lock (queue)
			{
				disposable = ArrayPoolThreaded<Action>.RentWithoutDefaultValue(queue.Count, out actions);
				queue.CopyTo(actions, 0);
				queue.Clear();
			}
			using (disposable)
			{
				foreach (var action in actions) action?.TryInvoke();
			}
		}
	}
}
