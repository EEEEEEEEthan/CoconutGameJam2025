using System;
using Game.Utilities.UnityTools;
namespace Game.Utilities
{
	public class TimedCache<T>
	{
		readonly Action onExpire;
		readonly Func<T> create;
		readonly Action<T> onDispose;
		readonly TimeSpan expireTime;
		DateTime lastAccessTime;
		bool cached;
		T cachedValue;
		public T Value
		{
			get
			{
				if (cached) return cachedValue;
				lastAccessTime = DateTime.Now;
				MainThreadTimerManager.InvokeAfter(expireTime.Seconds, onExpire);
				return cachedValue = create();
			}
		}
		bool Expired => DateTime.Now - lastAccessTime > expireTime;
		public TimedCache(Func<T> create, Action<T> onDispose, TimeSpan expireTime)
		{
			lastAccessTime = DateTime.Now;
			this.create = create;
			this.onDispose = onDispose;
			this.expireTime = expireTime;
			onExpire = OnExpire;
		}
		void OnExpire()
		{
			if (Expired)
			{
				if (cached)
				{
					var value = cachedValue;
					cached = false;
					cachedValue = default;
					onDispose?.TryInvoke(value);
				}
			}
			else
			{
				MainThreadTimerManager.InvokeAfter(expireTime.TotalSeconds, onExpire);
			}
		}
	}
}
