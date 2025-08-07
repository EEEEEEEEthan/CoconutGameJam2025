#define USE_POOL
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Game.Utilities.Pools;
using UnityEngine;
using UnityEngine.Assertions;
namespace Game.Utilities.Collections
{
	public interface IReadOnlyQuadTree<T>
	{
		QuadTree<T>.CenterQueryEnumerator Query(Vector2 center);
	}
	/// <inheritdoc />
	public sealed class QuadTree<T> : IReadOnlyQuadTree<T>
	{
		public struct CenterQueryEnumerator : IEnumerator<T>
		{
			readonly Node root;
			Node.QueryEnumerator enumerator;
			(Pooled disposable, List<T> value) pooled;
			public bool Disposed { get; private set; }
			public T Current { get; private set; }
			readonly object IEnumerator.Current => Current;
			internal CenterQueryEnumerator(QuadTree<T> tree, float x, float y)
			{
				root = tree.root;
				if (root != null)
				{
					var disposable = ListPoolThreaded<T>.Rent(out var list);
					pooled = (disposable, list);
					enumerator = root.Query(x, y);
				}
				else
				{
					pooled = default;
					enumerator = default;
				}
				Disposed = false;
				Current = default;
			}
			public void Reset()
			{
				if (root != null)
				{
					var list = pooled.value;
					list.Clear();
					enumerator.Reset();
				}
			}
			public void Dispose()
			{
				if (Disposed) throw new InvalidOperationException("Already disposed");
				Disposed = true;
				if (root is null) return;
				enumerator.Dispose();
				pooled.disposable.Dispose();
				pooled = default;
			}
			public readonly CenterQueryEnumerator GetEnumerator() => this;
			public bool MoveNext()
			{
				if (root is null) return false;
				var list = pooled.value;
				while (list.Count <= 0 && enumerator.MoveNext())
					// ReSharper disable once PossibleNullReferenceException
					list.AddRange(enumerator.Current.values);
				if (list.TryPopLast(out var last))
				{
					Current = last;
					return true;
				}
				return false;
			}
		}
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "MemberCanBeProtected.Local")]
		abstract class Node
		{
			public float XMin
#if UNITY_EDITOR
			{
				get;
				private set;
			}
#else
				;
#endif
			public float XMax
#if UNITY_EDITOR
			{
				get;
				private set;
			}
#else
				;
#endif
			public float YMin
#if UNITY_EDITOR
			{
				get;
				private set;
			}
#else
				;
#endif
			public float YMax
#if UNITY_EDITOR
			{
				get;
				private set;
			}
#else
				;
#endif
			public float XMid
#if UNITY_EDITOR
			{
				get;
				private set;
			}
#else
				;
#endif
			public float YMid
#if UNITY_EDITOR
			{
				get;
				private set;
			}
#else
				;
#endif
			protected int changeFlag;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public abstract Node Add(float x, float y, T value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public abstract Node Remove(float x, float y, T value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public abstract Node Update(float oldX, float oldY, float newX, float newY, T value);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public abstract void Query(float xMin, float xMax, float yMin, float yMax, List<Leaf> result);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected abstract float GetSqrDistance(float x, float y);
			public void ResetRect(float xMin, float xMax, float yMin, float yMax)
			{
				XMin = xMin;
				XMax = xMax;
				YMin = yMin;
				YMax = yMax;
				XMid = (xMin + xMax) * 0.5f;
				YMid = (yMin + yMax) * 0.5f;
			}
			public bool RectContains(float x, float y) => x >= XMin && x < XMax && y >= YMin && y < YMax;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void GetRect(int index, out float xMin, out float xMax, out float yMin, out float yMax)
			{
				if ((index & 1) == 0)
				{
					xMin = XMin;
					xMax = XMid;
				}
				else
				{
					xMin = XMid;
					xMax = XMax;
				}
				if ((index & 2) == 0)
				{
					yMin = YMin;
					yMax = YMid;
				}
				else
				{
					yMin = YMid;
					yMax = YMax;
				}
			}
			/// <summary>
			///     异步不安全
			/// </summary>
			internal struct QueryEnumerator : IEnumerator<Leaf>
			{
				readonly float x;
				readonly float y;
				readonly HeapSingle<Node> heap;
				readonly Node node;
				readonly int changeFlag;
				Pooled disposable;
				bool disposed;
				public Leaf Current { get; private set; }
				readonly object IEnumerator.Current => Current;
				internal QueryEnumerator(Node node, float x, float y)
				{
					this.node = node;
					changeFlag = node.changeFlag;
					this.x = x;
					this.y = y;
					Current = null;
					disposable = HeapSinglePoolThreaded<Node>.Rent(out heap);
					heap.Add(node, node.GetSqrDistance(x, y));
					disposed = false;
				}
				public void Reset()
				{
					Current = null;
					heap.Clear();
					heap.Add(node, node.GetSqrDistance(x, y));
				}
				public void Dispose()
				{
					if (disposed)
					{
						Debug.LogError("already disposed");
						return;
					}
					disposed = true;
					disposable.Dispose();
				}
				public bool MoveNext()
				{
					if (changeFlag != node.changeFlag)
					{
						Debug.LogError("QuadTree changed while enumerating");
						return false;
					}
					var get = false;
					while (!get && heap.TryPop(out var node, out _))
					{
						if (node is Leaf leaf)
						{
							Current = leaf;
							get = true;
							continue;
						}
						var branch = (Branch)node;
						for (var i = 0; i < 4; ++i)
						{
							var child = branch.children[i];
							if (child is { }) heap.Add(child, child.GetSqrDistance(x, y));
						}
					}
					return get;
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public QueryEnumerator Query(float x, float y) => new(this, x, y);
			public void DrawGizmos()
			{
				var a = new Vector2(XMin, YMin);
				var b = new Vector2(XMax, YMin);
				var c = new Vector2(XMax, YMax);
				var d = new Vector2(XMin, YMax);
				Gizmos.DrawLine(a, b);
				Gizmos.DrawLine(b, c);
				Gizmos.DrawLine(c, d);
				Gizmos.DrawLine(d, a);
				switch (this)
				{
					case Branch branch:
					{
						for (var i = 0; i < 4; ++i) branch.children[i]?.DrawGizmos();
						break;
					}
					case Leaf leaf:
						Gizmos.DrawCube(new(leaf.X, leaf.Y), Vector3.one);
						break;
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected int GetIndex(float x, float y)
			{
				var index = 0;
				if (x >= XMid) index |= 1;
				if (y >= YMid) index |= 2;
				return index;
			}
		}
		sealed class Leaf : Node
		{
			static readonly ObjectPool<Leaf> threadedPool = new(
				create: () => new(),
				onReturn: leaf =>
				{
					leaf.values.Clear();
					return true;
				}
			);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Leaf New()
			{
#if USE_POOL
				threadedPool.Rent(out var leaf);
#else
				var leaf = new Leaf();
#endif
				return leaf;
			}
			public readonly List<T> values = new();
			public float X
#if UNITY_EDITOR
			{
				get;
				private set;
			}
#else
				;
#endif
			public float Y
#if UNITY_EDITOR
			{
				get;
				private set;
			}
#else
				;
#endif
			public override string ToString() => $"Leaf(x={X},y={Y},xMin={XMin},xMax={XMax},yMin={YMin},yMax={YMax})";
			public override Node Add(float x, float y, T value)
			{
				++changeFlag;
				if (X == x && Y == y)
				{
					values.Add(value);
					return this;
				}
				var branch = Branch.New();
				branch.ResetRect(XMin, XMax, YMin, YMax);
				var index = GetIndex(X, Y);
				branch.children[index] = this;
				branch.GetRect(index, out var xMin, out var xMax, out var yMin, out var yMax);
				ResetRect(xMin, xMax, yMin, yMax);
				return branch.Add(x, y, value);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override Node Remove(float x, float y, T value)
			{
				++changeFlag;
				values.Remove(value);
				if (values.Count == 0)
				{
					threadedPool.Return(this);
					return null;
				}
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override Node Update(float oldX, float oldY, float newX, float newY, T value)
			{
				++changeFlag;
				if (values.Count == 1)
				{
					X = newX;
					Y = newY;
					return this;
				}
				values.Remove(value);
				return Add(newX, newY, value);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override void Query(float xMin, float xMax, float yMin, float yMax, List<Leaf> result)
			{
				if (X >= xMin && X <= xMax && Y >= yMin && Y <= yMax) result.Add(this);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override float GetSqrDistance(float x, float y)
			{
				var dx = X - x;
				var dy = Y - y;
				return dx * dx + dy * dy;
			}
			public void ResetRect(float x, float y, float xMin, float xMax, float yMin, float yMax)
			{
				X = x;
				Y = y;
				base.ResetRect(xMin, xMax, yMin, yMax);
			}
		}
		sealed class Branch : Node
		{
			static readonly ObjectPool<Branch> threadedPool = new(
				create: () => new(),
				onReturn: branch =>
				{
					Array.Clear(branch.children, 0, 4);
					return true;
				}
			);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Branch New()
			{
#if USE_POOL
				threadedPool.Rent(out var branch);
#else
				var branch = new Branch();
#endif
				return branch;
			}
			public readonly Node[] children = new Node[4];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override Node Update(float oldX, float oldY, float newX, float newY, T value)
			{
				++changeFlag;
				var oldIndex = GetIndex(oldX, oldY);
				var newIndex = GetIndex(newX, newY);
				var oldChild = children[oldIndex];
				if (oldIndex == newIndex)
				{
					children[oldIndex] = oldChild.Update(oldX, oldY, newX, newY, value);
					return this;
				}
				children[oldIndex] = oldChild.Remove(oldX, oldY, value);
				var newChild = children[newIndex];
				if (newChild is null)
				{
					var leaf = Leaf.New();
					GetRect(newIndex, out var xMin, out var xMax, out var yMin, out var yMax);
					leaf.ResetRect(newX, newY, xMin, xMax, yMin, yMax);
					leaf.values.Add(value);
					children[newIndex] = leaf;
					return this;
				}
				children[newIndex] = newChild.Add(newX, newY, value);
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override Node Add(float x, float y, T value)
			{
				++changeFlag;
				Assert.IsTrue(RectContains(x, y));
				var index = GetIndex(x, y);
				var child = children[index];
				if (child is null)
				{
					var leaf = Leaf.New();
					GetRect(index, out var xMin, out var xMax, out var yMin, out var yMax);
					leaf.ResetRect(x, y, xMin, xMax, yMin, yMax);
					leaf.values.Add(value);
					children[index] = leaf;
					return this;
				}
				children[index] = child.Add(x, y, value);
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override void Query(float xMin, float xMax, float yMin, float yMax, List<Leaf> result)
			{
				if (xMax <= XMin || xMin >= XMax || yMax <= YMin || yMin >= YMax) return;
				for (var i = 0; i < 4; ++i) children[i]?.Query(xMin, xMax, yMin, yMax, result);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public override Node Remove(float x, float y, T value)
			{
				++changeFlag;
				Assert.IsTrue(RectContains(x, y));
				var index = GetIndex(x, y);
				var child = children[index];
				child = children[index] = child.Remove(x, y, value);
				if (child is null or Leaf)
				{
					var count = 0;
					var onlyChildIndex = 0;
					for (var i = 0; i < 4; ++i)
						if (children[i] is { })
						{
							onlyChildIndex = i;
							++count;
						}
					if (count == 1)
					{
						var onlyChild = children[onlyChildIndex];
						if (onlyChild is Leaf leaf)
						{
							leaf.ResetRect(leaf.X, leaf.Y, XMin, XMax, YMin, YMax);
							Assert.IsTrue(onlyChild != this);
							threadedPool.Return(this);
							return onlyChild;
						}
					}
				}
				return this;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected override float GetSqrDistance(float x, float y)
			{
				var dx = x < XMin ? XMin - x : x > XMax ? x - XMax : 0;
				var dy = y < YMin ? YMin - y : y > YMax ? y - YMax : 0;
				return dx * dx + dy * dy;
			}
		}
		readonly Dictionary<T, Vector2> item2Position = new();
		Node root;
		Rect rect;
		public QuadTree(Rect rect) => this.rect = rect;
		public void AddOrUpdate(T value, Vector2 position)
		{
			if (!rect.Contains(position)) throw new ArgumentOutOfRangeException($"position: {position} is not in the rect: {rect}");
			if (item2Position.TryGetValue(value, out var oldPosition))
			{
				if (oldPosition == position) return;
				item2Position[value] = position;
				root = root.Update(oldPosition.x, oldPosition.y, position.x, position.y, value);
			}
			else
			{
				item2Position.Add(value, position);
				if (root is null)
				{
					var leaf = Leaf.New();
					leaf.ResetRect(position.x, position.y, rect.xMin, rect.xMax, rect.yMin, rect.yMax);
					root = leaf;
				}
				root = root.Add(position.x, position.y, value);
			}
		}
		public void DrawGizmos()
		{
			Assert.IsTrue(Application.isEditor);
			root?.DrawGizmos();
		}
		public CenterQueryEnumerator Query(Vector2 center) => new(this, center.x, center.y);
		public IEnumerable<T> Query(Rect rect)
		{
			if (root is null) yield break;
			var result = new List<Leaf>();
			root.Query(rect.xMin, rect.xMax, rect.yMin, rect.yMax, result);
			foreach (var leaf in result)
				foreach (var value in leaf.values)
					yield return value;
		}
		public bool Remove(T value)
		{
			if (item2Position.Remove(value, out var position))
			{
				root = root.Remove(position.x, position.y, value);
				return true;
			}
			return false;
		}
	}
}
