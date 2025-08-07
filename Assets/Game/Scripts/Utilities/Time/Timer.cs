using System;
using System.Collections.Generic;
using Game.Utilities.Collections;
namespace Game.Utilities.Time
{
	public readonly struct TimerId
	{
		internal readonly uint id;
		public bool Valid => id != 0;
		internal TimerId(uint id) => this.id = id;
	}
	public sealed class Timer
	{
		struct TimerInfo
		{
			public readonly double time;
			public readonly Action callbackItem;
			public TimerInfo(double time, Action callbackItem)
			{
				this.time = time;
				this.callbackItem = callbackItem;
			}
		}
		readonly Dictionary<uint, TimerInfo> timers = new();
		readonly HeapDouble<uint> timerHeap = new();
		readonly List<uint> pending = new();
		readonly object syncLock = new();
		double currentTime;
		uint currentId;
		public void Update(double currentTime)
		{
			this.currentTime = currentTime;
			lock (syncLock)
			{
				foreach (var id in pending)
					if (timers.TryGetValue(id, out var info))
						timerHeap.Add(id, info.time);
				pending.Clear();
			}
			while (TryPop(currentTime, out var timerInfo)) timerInfo.callbackItem?.TryInvoke();
		}
		public TimerId InvokeAfter(double seconds, Action callback)
		{
			uint id;
			var timerInfo = new TimerInfo(currentTime + seconds, callback);
			lock (syncLock)
			{
				pending.Add(id = ++currentId);
				timers.Add(id, timerInfo);
			}
			return new(id);
		}
		public void InvokeAfter(ref TimerId id, double seconds, Action callback)
		{
			CancelInvoke(id);
			id = InvokeAfter(seconds, callback);
		}
		public bool CancelInvoke(TimerId id)
		{
			var i = id.id;
			if (i == 0 || i > currentId) return false;
			lock (syncLock)
			{
				if (timers.Remove(i, out var timerInfo))
				{
					if (!pending.Remove(i)) timerHeap.Remove(i, timerInfo.time);
					return true;
				}
			}
			return false;
		}
		public bool CancelInvoke(ref TimerId id)
		{
			var removed = CancelInvoke(id);
			id = default;
			return removed;
		}
		bool TryPop(double currentTime, out TimerInfo timerInfo)
		{
			lock (syncLock)
			{
				if (timerHeap.TryPeek(out var id, out var time))
					if (time <= currentTime)
					{
						timerHeap.Pop();
						return timers.Remove(id, out timerInfo);
					}
			}
			timerInfo = default;
			return false;
		}
	}
}
