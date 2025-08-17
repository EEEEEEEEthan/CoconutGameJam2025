using System;
using System.Linq;
using Game.ResourceManagement;
using UnityEngine;
namespace Game.Gameplay
{
	[ExecuteAlways]
	public class FurSurface : MonoBehaviour
	{
		public enum UVType
		{
			Uv,
			ObjectXy,
			ObjectXz,
			WorldXy,
			WorldXz,
		}
		[SerializeField] UVType uvtype;
		[SerializeField, Range(0, 128),] int layerCount = 10;
		[SerializeField] Gradient gradient = new()
		{
			alphaKeys = new GradientAlphaKey[]
			{
				new(1, 1),
				new(0, 1),
			},
			colorKeys = new GradientColorKey[]
			{
				new(Color.red, 0),
				new(Color.green, 1),
			},
		};
		[SerializeField, Min(0),] float fullLength = 0.1f;
		[SerializeField] Vector4 noiseScales = new(100, 100, 100, 100);
		[SerializeField] Vector4 noiseWeights = new(1, 1, 1, 1);
		[SerializeField] AnimationCurve layerToLength = new()
		{
			keys = new Keyframe[]
			{
				new(0, 0, 0, 999, 0, 0.001f),
				new(1, 1, 0, 0, 0.1f, 0),
			},
		};
		[SerializeField] AnimationCurve layerToAlphaClip = new()
		{
			keys = new Keyframe[]
			{
				new(0, 0),
				new(1, 1),
			},
		};
		[SerializeField, Range(0, 1),] float smoothness;
		[SerializeField, Range(0, 1),] float ambientOcclusion;
		[SerializeField, Range(0, 1),] float metallic;
		[SerializeField] float gravityStrength;
		[SerializeField] float gravityPower = 2;
		MeshRenderer meshRenderer;
		MeshRenderer MeshRenderer => meshRenderer ??= GetComponent<MeshRenderer>();
		void OnEnable() => Refresh();
		void OnDisable() => MeshRenderer.sharedMaterials = Array.Empty<Material>();
		void OnBecameVisible()
		{
			var bounds = MeshRenderer.bounds;
			bounds = new(bounds.center, bounds.size + Vector3.one * fullLength * 2);
			MeshRenderer.bounds = bounds;
			var localBounds = MeshRenderer.localBounds;
			localBounds = new(localBounds.center, localBounds.size + Vector3.one * fullLength);
			MeshRenderer.localBounds = localBounds;
		}
#if UNITY_EDITOR
		void OnValidate()
		{
			if (Application.isPlaying) return;
			if (!UnityEditor.Selection.gameObjects.Contains(gameObject)) return;
			Refresh();
		}
#endif
		void Refresh()
		{
			if (!enabled) return;
			var sharedMaterials = new Material[layerCount];
			var baseMaterial = ResourceTable.furMaterialMat.Main;
			Array.Copy(MeshRenderer.sharedMaterials, sharedMaterials, Mathf.Min(MeshRenderer.sharedMaterials.Length, layerCount));
			for (var i = 0; i < layerCount; i++)
			{
				Material material;
				if (sharedMaterials[i] && sharedMaterials[i].shader == baseMaterial.shader)
					material = sharedMaterials[i];
				else
					material = sharedMaterials[i] = new(baseMaterial)
					{
						hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector,
					};
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
				material.SetInt("_UVTYPE", (int)uvtype);
			}
			MeshRenderer.sharedMaterials = sharedMaterials;
		}
	}
}
