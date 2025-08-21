using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
namespace SketchPostProcess
{
	[Serializable, VolumeComponentMenu("Post-processing/Custom/SketchVolume"),]
	public sealed class SketchVolume : CustomPostProcessVolumeComponent, IPostProcessComponent
	{
		const string shaderName = "Shader Graphs/SketchShader";
		static readonly int MainTex = Shader.PropertyToID("_MainTex");
		static readonly int Color0 = Shader.PropertyToID("_Color0");
		static readonly int Color1 = Shader.PropertyToID("_Color1");
		static readonly int Color2 = Shader.PropertyToID("_Color2");
		static readonly int Color3 = Shader.PropertyToID("_Color3");
		static readonly int MinMaxScaleDensity0 = Shader.PropertyToID("_MinMaxScaleDensity0");
		static readonly int MinMaxScaleDensity1 = Shader.PropertyToID("_MinMaxScaleDensity1");
		static readonly int MinMaxScaleDensity2 = Shader.PropertyToID("_MinMaxScaleDensity2");
		static readonly int MinMaxScaleDensity3 = Shader.PropertyToID("_MinMaxScaleDensity3");
		static readonly int NoiseStrengthAndScale = Shader.PropertyToID("_NoiseStrengthAndScale");
		static readonly int Seeds = Shader.PropertyToID("_Seeds");
		static readonly int Weight = Shader.PropertyToID("_Weight");
		static readonly int TimeScale = Shader.PropertyToID("_TimeScale");
		[SerializeField] BoolParameter enabled = new(true, true);
		[SerializeField] ClampedFloatParameter weight = new(0.8f, 0, 1);
		[SerializeField] FloatRangeParameter range0 = new(new(0.15f, 0.2f), 0, 1);
		[SerializeField] FloatRangeParameter range1 = new(new(0.075f, 0.15f), 0, 1);
		[SerializeField] FloatRangeParameter range2 = new(new(0.0375f, 0.75f), 0, 1);
		[SerializeField] FloatRangeParameter range3 = new(new(0f, 0.0375f), 0, 1);
		[SerializeField] ClampedFloatParameter scale0 = new(4, 1, 24);
		[SerializeField] ClampedFloatParameter scale1 = new(8, 1, 24);
		[SerializeField] ClampedFloatParameter scale2 = new(12, 1, 24);
		[SerializeField] ClampedFloatParameter scale3 = new(16, 1, 24);
		[SerializeField] ClampedFloatParameter density0 = new(0, 0, 64);
		[SerializeField] ClampedFloatParameter density1 = new(2, 0, 64);
		[SerializeField] ClampedFloatParameter density2 = new(4, 0, 64);
		[SerializeField] ClampedFloatParameter density3 = new(8, 0, 64);
		[SerializeField] ColorParameter color0 = new(Color.black);
		[SerializeField] ColorParameter color1 = new(Color.black);
		[SerializeField] ColorParameter color2 = new(Color.black);
		[SerializeField] ColorParameter color3 = new(Color.black);
		[SerializeField] Vector4Parameter seeds = new(new(1, 2, 3, 4));
		[SerializeField] ClampedFloatParameter noiseScale = new(1, 0, 5);
		[SerializeField] ClampedFloatParameter noiseStrength = new(0.002f, 0, 0.01f);
		[SerializeField] FloatParameter timeScale = new(5f);
		[SerializeField, HideInInspector,] Material material;
		public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
		public float Weigth
		{
			get => weight.value;
			set => weight.value = value;
		}
		void Reset() => enabled.Override(true);
		public override void Setup()
		{
			base.Setup();
			if (Shader.Find(shaderName) != null)
				material = new(Shader.Find(shaderName));
			else
				Debug.LogError(
					$"Unable to find shader '{shaderName}'. Post Process Volume SketchVolume is unable to load.");
		}
		public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
		{
			if (material == null) return;
			material.SetTexture(MainTex, source);
			var range0 = this.range0.overrideState ? this.range0.value : new(0.15f, 0.2f);
			var range1 = this.range1.overrideState ? this.range1.value : new(0.075f, 0.15f);
			var range2 = this.range2.overrideState ? this.range2.value : new(0.0375f, 0.075f);
			var range3 = this.range3.overrideState ? this.range3.value : new(0, 0.0375f);
			var scale0 = this.scale0.overrideState ? this.scale0.value : 4f;
			var scale1 = this.scale1.overrideState ? this.scale1.value : 8f;
			var scale2 = this.scale2.overrideState ? this.scale2.value : 12f;
			var scale3 = this.scale3.overrideState ? this.scale3.value : 16f;
			var density0 = this.density0.overrideState ? this.density0.value : 0f;
			var density1 = this.density1.overrideState ? this.density1.value : 2f;
			var density2 = this.density2.overrideState ? this.density2.value : 4f;
			var density3 = this.density3.overrideState ? this.density3.value : 8f;
			var color0 = this.color0.overrideState ? this.color0.value : Color.black;
			var color1 = this.color1.overrideState ? this.color1.value : Color.black;
			var color2 = this.color2.overrideState ? this.color2.value : Color.black;
			var color3 = this.color3.overrideState ? this.color3.value : Color.black;
			var seeds = this.seeds.overrideState ? this.seeds.value : new(1, 2, 3, 4);
			var noiseScale = this.noiseScale.overrideState ? this.noiseScale.value : 1f;
			var noiseStrength = this.noiseStrength.overrideState ? this.noiseStrength.value : 0.002f;
			material.SetColor(Color0, color0);
			material.SetColor(Color1, color1);
			material.SetColor(Color2, color2);
			material.SetColor(Color3, color3);
			material.SetVector(MinMaxScaleDensity0, new(range0.x, range0.y, scale0, density0));
			material.SetVector(MinMaxScaleDensity1, new(range1.x, range1.y, scale1, density1));
			material.SetVector(MinMaxScaleDensity2, new(range2.x, range2.y, scale2, density2));
			material.SetVector(MinMaxScaleDensity3, new(range3.x, range3.y, scale3, density3));
			material.SetVector(NoiseStrengthAndScale, new Vector2(noiseStrength, noiseScale));
			material.SetVector(Seeds, seeds);
			material.SetFloat(Weight, weight.overrideState ? weight.value : 0.8f);
			material.SetFloat(TimeScale, timeScale.overrideState ? timeScale.value : 5f);
			HDUtils.DrawFullScreen(cmd, material, destination, shaderPassId: 0);
		}
		public override void Cleanup()
		{
			CoreUtils.Destroy(material);
			material = null;
			base.Cleanup();
		}
		bool IPostProcessComponent.IsActive()
		{
			if (!enabled.overrideState) return false;
			if (!enabled.value) return false;
			if (weight.overrideState && weight.value <= 0) return false;
			return true;
		}
	}
}
