using System;
using System.Collections.Generic;
namespace Game.Utilities
{
	public interface ITrackableValue<out T>
	{
		public T Value { get; }
		public event Action OnValueChanged;
	}
	public class TrackableValue<T> : ITrackableValue<T>
	{
		readonly EqualityComparer<T> comparer;
		T value;
		public T Value
		{
			get => value;
			set
			{
				if (comparer.Equals(this.value, value)) return;
				this.value = value;
				OnValueChanged?.Invoke();
			}
		}
		public event Action OnValueChanged;
		public TrackableValue(T value, EqualityComparer<T> comparer = null)
		{
			this.value = value;
			this.comparer = comparer ?? EqualityComparer<T>.Default;
		}
	}
}
