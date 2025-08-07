using System;
using System.Collections;
using System.Collections.Generic;
namespace Game.Utilities.Collections
{
	public class QuickAccessIdManagement
	{
		public readonly struct Id : IEquatable<Id>
		{
			public static implicit operator ulong(Id id) => id.id;
			public static implicit operator Id(ulong id) => new(id);
			readonly ulong id;
			internal int Index => (int)(id & 0xFFFFFFFFul);
			internal uint Number => (uint)(id >> 32);
			internal Id(uint id, int index) => this.id = ((ulong)id << 32) | (uint)index;
			Id(ulong id) => this.id = id;
			public override int GetHashCode() => (int)Number;
			public override string ToString() => Number.ToString();
			public bool Equals(Id other) => id == other.id;
		}
		public interface IItem
		{
			Id Id { get; internal set; }
		}
	}
	public class QuickAccessIdManagement<T> : QuickAccessIdManagement where T : QuickAccessIdManagement.IItem
	{
		T[] items;
		BitArray activeItems;
		uint currentNumber;
		int currentIndex;
		Id NextFreeId
		{
			get
			{
				var length = items.Length;
				for (var i = currentIndex; i < length; ++i)
					if (!activeItems.Get(i))
						return new(++currentNumber, currentIndex = i);
				for (var i = 0; i < currentIndex; ++i)
					if (!activeItems.Get(i))
						return new(++currentNumber, currentIndex = i);
				throw new InvalidOperationException("No free id available.");
			}
		}
		public T this[Id id]
		{
			get
			{
				if (!activeItems.Get(id.Index)) throw new KeyNotFoundException($"Item with id number {id.Number} not found.");
				var item = items[id.Index];
				if (item.Id.Number != id.Number) throw new KeyNotFoundException($"Item with id number {id.Number} not found.");
				return item;
			}
		}
		public void Add(IItem item)
		{
			var id = NextFreeId;
			var index = id.Index;
			if (activeItems.Get(index)) throw new InvalidOperationException("Id already in use.");
			activeItems.Set(index, true);
			items[index] = (T)item;
		}
		public bool Remove(IItem item)
		{
			var id = item.Id;
			var index = id.Index;
			if (!activeItems.Get(index)) return false;
			if (items[index].Id != id) return false;
			activeItems.Set(index, false);
			items[index] = default;
			return true;
		}
		public bool TryGet(Id id, out T item)
		{
			if (!activeItems.Get(id.Index))
			{
				item = default;
				return false;
			}
			item = items[id.Index];
			if (item.Id.Number != id.Number)
			{
				item = default;
				return false;
			}
			return true;
		}
		public T GetValueOrDefault(Id id, T defaultValue = default)
		{
			if (!activeItems.Get(id.Index)) return defaultValue;
			var item = items[id.Index];
			return item.Id.Number != id.Number ? defaultValue : item;
		}
	}
}
