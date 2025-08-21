using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace SketchPostProcess
{
    [Serializable, VolumeComponentMenu("Post-processing/Custom/BlurVolume")]
    public sealed class BlurVolume : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        const string shaderName = "Shader Graphs/BlurShader";
        [SerializeField] BoolParameter enabled = new(true, true);
        [SerializeField] ClampedFloatParameter strength = new(0.8f, 0, 10);
        [SerializeField] ClampedFloatParameter angle = new(0, 0, 360);
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
            material.SetFloat("_Strength", strength.value);
            material.SetFloat("_Angle", angle.value * Mathf.Deg2Rad);
            HDUtils.DrawFullScreen(cmd, material, destination);
        }

        bool IPostProcessComponent.IsActive()
        {
            if (!enabled.overrideState) return false;
            if (!enabled.value) return false;
            if (strength.value <= 0) return false;
            return true;
        }
    }
}