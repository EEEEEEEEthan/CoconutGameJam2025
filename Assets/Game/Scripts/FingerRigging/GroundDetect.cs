using System;
using System.Collections.Generic;
using Game.Utilities;
using Game.Utilities.Pools;
using UnityEngine;
namespace Game.FingerRigging
{
	[RequireComponent(typeof(SphereCollider))]
	public class GroundDetect : MonoBehaviour
	{
		readonly HashSet<Collider> colliders = new();
		new Collider collider;
		public Collider Collider => collider ??= GetComponent<Collider>();
		public IReadOnlyCollection<Collider> Colliders => colliders;
		public bool Any => colliders.Count > 0;
		public event Action<Collider> OnTriggerEntered;
		public event Action<Collider> OnTriggerExited;
		void Update()
		{
			// in case some colliders were destroyed or disabled
			using var _ = ListPoolThreaded<Collider>.Rent(out var expired);
			foreach (var collider in colliders)
				if (!collider || !collider.enabled || !collider.gameObject.activeInHierarchy)
					expired.Add(collider);
			foreach (var collider in expired)
				if (colliders.Remove(collider))
					OnTriggerExited?.TryInvoke(collider);
		}
		void OnTriggerEnter(Collider other)
		{
			if (ShouldIgnoreCollider(other)) return;
			if (colliders.Add(other)) OnTriggerEntered?.TryInvoke(other);
		}
		void OnTriggerExit(Collider other)
		{
			if (ShouldIgnoreCollider(other)) return;
			if (colliders.Remove(other)) OnTriggerExited?.TryInvoke(other);
		}
		bool ShouldIgnoreCollider(Collider other) => other.gameObject.layer == gameObject.layer;
	}
}
