using System;
using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class FurSurface : MonoBehaviour
	{
		[SerializeField] Material material;
		[SerializeField, Range(0, 128),] int layerCount = 10;
		[SerializeField] Gradient gradient;
		[SerializeField, Min(0),] float fullLength;
		[SerializeField] Vector4 noiseScales = new(100, 100, 100, 100);
		[SerializeField] Vector4 noiseWeights = new(1, 1, 1, 1);
		[SerializeField] AnimationCurve layerToLength;
		[SerializeField] AnimationCurve layerToAlphaClip;
		[SerializeField] float smoothness;
		[SerializeField] float ambientOcclusion;
		[SerializeField] float metallic;
		[SerializeField] float gravityStrength;
		[SerializeField] float gravityPower;
		MeshRenderer meshRenderer;
		MeshRenderer MeshRenderer => meshRenderer ??= GetComponent<MeshRenderer>();
		void OnEnable() => Refresh();
		void OnDisable() => MeshRenderer.sharedMaterials = new Material[0];
		void OnBecameVisible()
		{
			var bounds = MeshRenderer.bounds;
			bounds = new(bounds.center, bounds.size + Vector3.one * fullLength * 2);
			MeshRenderer.bounds = bounds;
			var localBounds = MeshRenderer.localBounds;
			localBounds = new(localBounds.center, localBounds.size + Vector3.one * fullLength);
			MeshRenderer.localBounds = localBounds;
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
			Array.Copy(MeshRenderer.sharedMaterials, sharedMaterials, Mathf.Min(MeshRenderer.sharedMaterials.Length, layerCount));
			for (var i = 0; i < layerCount; i++)
			{
				Material material;
				if (sharedMaterials[i] && sharedMaterials[i].shader == this.material.shader)
					material = sharedMaterials[i];
				else
					material = sharedMaterials[i] = new(this.material);
				var progress = (i + 1f) / layerCount;
				material.SetFloat("_Length", layerToLength.Evaluate(progress) * fullLength);
				material.SetVector("_NoiseScales", noiseScales);
				material.SetVector("_NoiseWeights", noiseWeights);
				material.color = gradient.Evaluate(progress);
				material.SetFloat("_AlphaClipThreshold", layerToAlphaClip.Evaluate(progress));
				material.SetFloat("_Smoothness", smoothness);
				material.SetFloat("_AmbientOcclusion", ambientOcclusion);
				material.SetFloat("_Metallic", metallic);
				material.SetFloat("_GravityStrength", gravityStrength);
				material.SetFloat("_GravityPower", gravityPower);
			}
			MeshRenderer.sharedMaterials = sharedMaterials;
		}
	}
}
