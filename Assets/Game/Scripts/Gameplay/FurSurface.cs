using System;
using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class FurSurface : MonoBehaviour
	{
		[SerializeField] MeshRenderer meshRenderer;
		[SerializeField] Material material;
		[SerializeField, Range(0, 128),] int layerCount = 10;
		[SerializeField] Gradient gradient;
		[SerializeField, Min(0),] float fullLength;
		[SerializeField] float noiseScale = 1f;
		[SerializeField] AnimationCurve layerToLength;
		[SerializeField] AnimationCurve layerToAlphaClip;
		[SerializeField] float smoothness;
		[SerializeField] float ambientOcclusion;
		[SerializeField] float metallic;
		[SerializeField] float gravityStrength;
		[SerializeField] float gravityPower;
		void OnEnable() => Refresh();
		void OnDisable() => meshRenderer.sharedMaterials = new Material[0];
		void OnBecameVisible()
		{
			var bounds = meshRenderer.bounds;
			bounds = new(bounds.center, bounds.size + Vector3.one * fullLength * 2);
			meshRenderer.bounds = bounds;
			var localBounds = meshRenderer.localBounds;
			localBounds = new(localBounds.center, localBounds.size + Vector3.one * fullLength);
			meshRenderer.localBounds = localBounds;
		}
		void OnValidate()
		{
			if (Application.isPlaying) return;
			Refresh();
		}
		void Refresh()
		{
			if (!enabled) return;
			var sharedMaterials = new Material[layerCount];
			Array.Copy(meshRenderer.sharedMaterials, sharedMaterials, Mathf.Min(meshRenderer.sharedMaterials.Length, layerCount));
			for (var i = 0; i < layerCount; i++)
			{
				Material material;
				if (sharedMaterials[i] && sharedMaterials[i].shader == this.material.shader)
					material = sharedMaterials[i];
				else
					material = sharedMaterials[i] = new(this.material);
				var progress = (i + 1f) / layerCount;
				material.SetFloat("_Length", layerToLength.Evaluate(progress) * fullLength);
				material.SetFloat("_NoiseScale", noiseScale);
				material.color = gradient.Evaluate(progress);
				material.SetFloat("_AlphaClipThreshold", layerToAlphaClip.Evaluate(progress));
				material.SetFloat("_Smoothness", smoothness);
				material.SetFloat("_AmbientOcclusion", ambientOcclusion);
				material.SetFloat("_Metallic", metallic);
				material.SetFloat("_GravityStrength", gravityStrength);
				material.SetFloat("_GravityPower", gravityPower);
			}
			meshRenderer.sharedMaterials = sharedMaterials;
		}
	}
}
