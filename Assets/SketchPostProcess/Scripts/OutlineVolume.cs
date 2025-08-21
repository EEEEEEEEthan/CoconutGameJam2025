using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace SketchPostProcess
{
    [Serializable, VolumeComponentMenu("Post-processing/Custom/OutlineVolume")]
    public sealed class OutlineVolume : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        const string shaderName = "Shader Graphs/OutlineShader";
        [SerializeField] BoolParameter enabled = new(true, true);
        [SerializeField] ClampedFloatParameter threshold = new(0.8f, 0, Mathf.Sqrt(3));
        [SerializeField] ClampedFloatParameter strength = new(1, 0, 2);
        [SerializeField] MinFloatParameter power = new(1, 0);
        [SerializeField] ClampedFloatParameter weight = new(0.8f, 0, 1);
        [SerializeField] ClampedFloatParameter sourceColor = new(0, 0, 1);
        Material material;

        public override void Setup()
        {
            base.Setup();
            if (Shader.Find(shaderName) != null)
                material = new Material(Shader.Find(shaderName));
            else
                Debug.LogError(
                    $"Unable to find shader '{shaderName}'. Post Process Volume SketchVolume is unable to load.");
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(material);
            base.Cleanup();
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (material == null) return;
            material.SetTexture("_MainTex", source);
            material.SetFloat("_Threshold", threshold.value);
            material.SetFloat("_Strength", strength.value);
            material.SetFloat("_Power", power.value);
            material.SetFloat("_Weight", weight.value);
            material.SetFloat("_SourceColor", sourceColor.value);
            HDUtils.DrawFullScreen(cmd, material, destination);
        }

        bool IPostProcessComponent.IsActive()
        {
            if (!enabled.overrideState) return false;
            if (!enabled.value) return false;
            return true;
        }
    }
}