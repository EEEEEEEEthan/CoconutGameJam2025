// Shader Graph Baker <https://u3d.as/2VQd>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using UnityEngine;


namespace AmazingAssets.ShaderGraphBaker
{
    public static class MaterialExtensions
    {
        public static Texture2D BakeTexture2D(this Material material, int resolution, bool hasMipmap, bool linear)
        {
            //Make sure texture size is correct
            resolution = Mathf.Clamp(resolution, 4, SystemInfo.maxTextureSize);

            //Adjust linear
            linear = (QualitySettings.activeColorSpace == ColorSpace.Linear && linear) ? true : false;

            //Create RT
            RenderTextureReadWrite renderTextureReadWrite = linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
            RenderTexture renderTexture = RenderTexture.GetTemporary(resolution, resolution, 16, RenderTextureFormat.Default, renderTextureReadWrite);

            //Render material to RT
            material.UpdateRenderTexture(ref renderTexture);


            //Bake RT to Texture2D
            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = renderTexture;

            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, hasMipmap, linear);
            texture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0, hasMipmap);
            texture.Apply(hasMipmap);

            //Cleanup
            RenderTexture.ReleaseTemporary(renderTexture);
            RenderTexture.active = previousRT;

            return texture;
        }

        public static void UpdateRenderTexture(this Material material, ref RenderTexture renderTexture)
        {
            if(renderTexture == null)
            {
                Debug.LogWarning("Cannot update RenderTexture, it is null.\n");

                return;
            }

            Graphics.Blit(Texture2D.whiteTexture, renderTexture, material);
        }
    }
}
