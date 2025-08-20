// Shader Graph Baker <https://u3d.as/2VQd>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.ShaderGraph;


namespace AmazingAssets.ShaderGraphBaker.Editor
{
    static internal class TextureBaker
    {
        static Mesh quadMesh = Resources.GetBuiltinResource(typeof(Mesh), "Quad.fbx") as Mesh;
        static int minTextureSize = 16;
        static string shaderTimeParameter = "_TimeParameters";

        static internal void BakeTexture(Node node)
        {
            string saveDirectory;
            string saveFileName;
            string saveExtension;
            bool savePathIsProjectRelative;
            if (GetTextureSavePathOptions(node, out saveDirectory, out saveFileName, out saveExtension, out savePathIsProjectRelative) == false)
                return;


            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            Material material = GetPreviewMaterial(node, materialPropertyBlock, node.Mode);
            if (material == null)
                return;


            int framesCount;
            float increment;
            float initialValue;
            GetSequenceBakeOption(node, out framesCount, out initialValue, out increment);


            int textureSize;
            GetTextureSize(node, out textureSize);


            string lastSavedFilePath = string.Empty;


            switch (node.Mode)
            {
                case Enum.Mode.Single:
                case Enum.Mode.Sequence:
                    {
                        for (int i = 0; i < framesCount; i++)
                        {
                            UnityEditor.EditorUtility.DisplayProgressBar("Hold On", string.Empty, (float)i / framesCount);

                            //Update 'Time' node
                            UpdateTimeNode(materialPropertyBlock, initialValue + increment * i);


                            Texture2D texture = GetPreviewTexture(material, materialPropertyBlock, textureSize, textureSize, node.SuperSize, false, node.Format, node.TextureType);
                            if (texture != null)
                            {
                                string savePath;
                                if (framesCount == 1)
                                    savePath = Path.Combine(saveDirectory, $"{saveFileName}.{saveExtension}");
                                else
                                    savePath = Path.Combine(saveDirectory, $"{saveFileName} [{i}].{saveExtension}");


                                SaveTexture(texture, node.Format, savePath);
                                GameObject.DestroyImmediate(texture);

                                AssetDatabase.Refresh();

                                //Update import settings for 'Normal' textures
                                if (savePathIsProjectRelative)
                                    UpdateImportSettings(savePath, node.TextureType);


                                lastSavedFilePath = savePath;
                            }
                        }
                    } 
                    break;

                case Enum.Mode.Atlas:
                    {
                        int rows, columns;
                        GetRowsAndColumns(node, out rows, out columns);

                        if (columns * textureSize > SystemInfo.maxTextureSize)
                        {
                            Debug.LogError($"Generated texture resolution [{columns * textureSize}] exceeds system limit [{SystemInfo.maxTextureSize}].\n");
                            break;
                        }

                        if (rows * textureSize > SystemInfo.maxTextureSize)
                        {
                            Debug.LogError($"Generated texture resolution [{rows * textureSize}] exceeds system limit [{SystemInfo.maxTextureSize}].\n");
                            break;
                        }


                        //Texture formats
                        TextureFormat textureFormat;
                        RenderTextureFormat renderTextureFormat;
                        GetTextureFormats(node.Format, out textureFormat, out renderTextureFormat);


                        //Main grid texture
                        Texture2D gridTexture = new Texture2D(columns * textureSize, rows * textureSize, textureFormat, false);

                        int x = 0;
                        int y = gridTexture.height - textureSize;

                        for (int i = 0; i < framesCount; i++)
                        {
                            UnityEditor.EditorUtility.DisplayProgressBar("Hold On", string.Empty, (float)i / framesCount);

                            //Update 'Time' node
                            UpdateTimeNode(materialPropertyBlock, initialValue + increment * i);


                            Texture2D texture = GetPreviewTexture(material, materialPropertyBlock, textureSize, textureSize, node.SuperSize, false, node.Format, node.TextureType);
                            if (texture != null)
                            {
                                gridTexture.SetPixels(x, y, textureSize, textureSize, texture.GetPixels());

                                //Update x & y
                                x += textureSize;
                                if (x >= gridTexture.width)
                                {
                                    x = 0;
                                    y -= textureSize;
                                }

                                GameObject.DestroyImmediate(texture);
                            }
                        }


                        string savePath = Path.Combine(saveDirectory, $"{saveFileName}.{saveExtension}");

                        SaveTexture(gridTexture, node.Format, savePath);

                        AssetDatabase.Refresh();

                        //Update import settings for 'Normal' textures
                        if (savePathIsProjectRelative)
                            UpdateImportSettings(savePath, node.TextureType);


                        lastSavedFilePath = savePath;
                    }
                    break;

                case Enum.Mode._2DArray:
                    {
                        //Format
                        TextureFormat textureFormat = TextureFormat.BC7;

                        Texture2DArray texture2DArray = new Texture2DArray(textureSize, textureSize, framesCount, textureFormat, true);
                        texture2DArray.filterMode = FilterMode.Bilinear;
                        texture2DArray.wrapMode = TextureWrapMode.Clamp;


                        for (int slice = 0; slice < framesCount; slice++)
                        {
                            //UnityEditor.EditorUtility.DisplayProgressBar("Hold On", string.Empty, (float)i / framesCount);


                            //Update 'Time' node
                            UpdateTimeNode(materialPropertyBlock, initialValue + increment * slice);


                            Texture2D texture = GetPreviewTexture(material, materialPropertyBlock, textureSize, textureSize, node.SuperSize, false, Enum.Format.PNG, node.TextureType);

                            //Compress
                            UnityEditor.EditorUtility.CompressTexture(texture, textureFormat, UnityEditor.TextureCompressionQuality.Best);

                            //Copy
                            for (int m = 0; m < texture.mipmapCount; m++)
                                Graphics.CopyTexture(texture, 0, m, texture2DArray, slice, m);


                            GameObject.DestroyImmediate(texture);
                        }


                        string savePath = Path.Combine(saveDirectory, $"{saveFileName}.asset");

                        if (savePathIsProjectRelative)
                            AssetDatabase.CreateAsset(texture2DArray, savePath);
                        else
                            Utilities.SaveAssetUsingTempFolder(texture2DArray, savePath);


                        lastSavedFilePath = savePath;
                    }
                    break;

                case Enum.Mode.Texture3D:
                    {
                        //Format
                        TextureFormat textureFormat = TextureFormat.BC7;

                        Texture3D texture3D = new Texture3D(textureSize, textureSize, framesCount, textureFormat, false);
                        texture3D.wrapMode = TextureWrapMode.Clamp;
                        for (int slice = 0; slice < framesCount; slice++)
                        {
                            //UnityEditor.EditorUtility.DisplayProgressBar("Hold On", string.Empty, (float)i / framesCount);


                            //Update 'Time' node
                            UpdateTimeNode(materialPropertyBlock, initialValue + increment * slice);


                            Texture2D texture = GetPreviewTexture(material, materialPropertyBlock, textureSize, textureSize, node.SuperSize, false, Enum.Format.PNG, node.TextureType);

                            //Compress
                            UnityEditor.EditorUtility.CompressTexture(texture, textureFormat, UnityEditor.TextureCompressionQuality.Best);

                            //Copy
                            Graphics.CopyTexture(texture, 0, texture3D, slice);


                            GameObject.DestroyImmediate(texture);
                        }


                        string savePath = Path.Combine(saveDirectory, $"{saveFileName}.asset");

                        if (savePathIsProjectRelative)
                            AssetDatabase.CreateAsset(texture3D, savePath);
                        else
                            Utilities.SaveAssetUsingTempFolder(texture3D, savePath);


                        lastSavedFilePath = savePath;
                    }
                    break;


                default:
                    break;
            }


            UnityEditor.EditorUtility.ClearProgressBar();


            //Highlight last saved file inside Project window
            if (savePathIsProjectRelative)
            {
                UnityEngine.Object lastSavedFile = AssetDatabase.LoadAssetAtPath(lastSavedFilePath, typeof(Texture));

                if (node.OutputTexture == null)
                    UnityEditor.EditorGUIUtility.PingObject(lastSavedFile);
                else
                    node.OutputTexture = (Texture)lastSavedFile;
            }


            //Cleanup           
            GameObject.DestroyImmediate(material.shader);
            GameObject.DestroyImmediate(material);

            Resources.UnloadUnusedAssets();
        }
        static internal void BakeShader(Node node)
        {
            string saveDirectory;
            string saveFileName;
            bool savePathIsProjectRelative;
            if (GetShaderSavePathOptions(node, out saveDirectory, out saveFileName, out savePathIsProjectRelative) == false)
                return;

            //Generate shader
            string shaderString = GetPreviewShader(node, false);

            //Update shader name
            int startIndex = shaderString.IndexOf('"');
            int endIndex = shaderString.IndexOf('"', shaderString.IndexOf('"') + 1);
            shaderString = shaderString.Remove(startIndex, endIndex - startIndex + 1);
            shaderString = shaderString.Insert(startIndex, $"\"Shader Graph Baker/{saveFileName}\"");


            string savePath = Path.Combine(saveDirectory, $"{saveFileName}.shader");

            File.WriteAllText(savePath, shaderString);

            if (savePathIsProjectRelative)
            {
                AssetDatabase.Refresh();

                UnityEditor.EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(savePath, typeof(Shader)));
            }
        }
        static void GetSequenceBakeOption(Node node, out int framesCount, out float initialValue, out float increment)
        {
            switch (node.Mode)
            {
                case Enum.Mode.Sequence:
                    {
                        framesCount = Utilities.TryParseInt(node.GetSlotValue(Node.ImageCountSlotID, GenerationMode.ForReals), 1);
                            
                        initialValue = Utilities.TryParseFloat(node.GetSlotValue(Node.TimeInitialValueSlotID, GenerationMode.ForReals), 0);
                        increment = Utilities.TryParseFloat(node.GetSlotValue(Node.TimeIncrementSlotID, GenerationMode.ForReals), 1);
                    }
                    break;

                case Enum.Mode._2DArray:
                case Enum.Mode.Texture3D:
                    {
                        framesCount = Utilities.TryParseInt(node.GetSlotValue(Node.SlicesSlotID, GenerationMode.ForReals), 1);                             

                        initialValue = Utilities.TryParseFloat(node.GetSlotValue(Node.TimeInitialValueSlotID, GenerationMode.ForReals), 0);
                        increment = Utilities.TryParseFloat(node.GetSlotValue(Node.TimeIncrementSlotID, GenerationMode.ForReals), 1);                            
                    }
                    break;

                case Enum.Mode.Atlas:
                    {
                        int rows, collumns;
                        GetRowsAndColumns(node, out rows, out collumns);

                        framesCount = rows * collumns;

                        initialValue = Utilities.TryParseFloat(node.GetSlotValue(Node.TimeInitialValueSlotID, GenerationMode.ForReals), 0);
                        increment = Utilities.TryParseFloat(node.GetSlotValue(Node.TimeIncrementSlotID, GenerationMode.ForReals), 1);
                    }
                    break; 

                case Enum.Mode.Single:
                default:
                    {
                        framesCount = 1;
                        increment = 0;
                        initialValue = 0;
                    }
                    break;
            }


            //No sequence without 'timeStep'
            if (increment == 0)
                framesCount = 1;

            //Frames count cannot be negative
            if (framesCount < 1)
                framesCount = 1;
        }
        static bool GetTextureSavePathOptions(Node node, out string saveDirectory, out string saveFileName, out string saveExtension, out bool savePathIsProjectRelative)
        {
            saveDirectory = string.Empty;
            saveFileName = string.Empty;
            saveExtension = string.Empty;
            savePathIsProjectRelative = false;


            if (node.OutputTexture != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(node.OutputTexture);

                if (string.IsNullOrWhiteSpace(assetPath) == false)
                {
                    if (Path.GetExtension(assetPath).ToLowerInvariant() == ("." + GetTextureSaveExtension(node.Mode, node.Format)))
                    {
                        saveDirectory = Path.GetDirectoryName(assetPath);
                        saveFileName = Path.GetFileNameWithoutExtension(assetPath);
                        saveExtension = Path.GetExtension(assetPath).ToLowerInvariant().Replace(".", string.Empty);

                        return true;
                    }
                    else
                    {
                        string newPath = Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath)) + "." + GetTextureSaveExtension(node.Mode, node.Format);

                        //Change file extension
                        try
                        {
                            File.Move(assetPath, newPath);
                        }
                        catch (System.Exception)
                        {
                            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(newPath, typeof(Texture));
                            Debug.LogError($"Cannot overwrite selected texture. File with the same name and extension already exists:\n'{newPath}'.\n", asset);

                            return false;
                        }


                        //Update meta
                        if (File.Exists(assetPath + ".meta"))
                            File.Move(assetPath + ".meta", newPath + ".meta");

                        AssetDatabase.Refresh();


                        saveDirectory = Path.GetDirectoryName(newPath);
                        saveFileName = Path.GetFileNameWithoutExtension(newPath);
                        saveExtension = Path.GetExtension(newPath).ToLowerInvariant().Replace(".", string.Empty);

                        return true;
                    }
                }
            }


            string savePanelPath;
            //if (string.IsNullOrWhiteSpace(node.lastSavedTextureFilePath) == false && (File.Exists(node.lastSavedTextureFilePath) || Directory.Exists(Path.GetDirectoryName(node.lastSavedTextureFilePath))))
            //{
            //    savePanelPath = UnityEditor.EditorUtility.SaveFilePanel("Save texture", Path.GetDirectoryName(node.lastSavedTextureFilePath), Path.GetFileNameWithoutExtension(node.lastSavedTextureFilePath), GetTextureSaveExtension(node.Mode, node.Format));
            //}
            //else
            {
                //Get current ShaderGraph file path
                string graphPath = string.Empty;
                GUID guid;
                if (GUID.TryParse(node.owner.owner.graph.assetGuid, out guid))
                    graphPath = AssetDatabase.GUIDToAssetPath(guid);


                if (File.Exists(graphPath))
                    savePanelPath = UnityEditor.EditorUtility.SaveFilePanel("Save Texture", Path.GetDirectoryName(graphPath), Path.GetFileNameWithoutExtension(graphPath), GetTextureSaveExtension(node.Mode, node.Format));
                else
                    savePanelPath = UnityEditor.EditorUtility.SaveFilePanel("Save Texture", "Assets", "New Shader Graph Texture", GetTextureSaveExtension(node.Mode, node.Format));
            }

            if (string.IsNullOrWhiteSpace(savePanelPath) == false)
            {
                //If extension was changed inside the SavePanel to something unsupported
                string savePanelExtension = Path.GetExtension(savePanelPath).ToLowerInvariant();
                if ((savePanelExtension == ".jpg" || savePanelExtension == ".png" || savePanelExtension == ".tga" || savePanelExtension == ".exr" || savePanelExtension == ".asset") == false)
                    savePanelPath = Path.ChangeExtension(savePanelPath, "." + GetTextureSaveExtension(node.Mode, node.Format));


                //Check if path is project relative
                savePathIsProjectRelative = Utilities.IsPathProjectRelative(savePanelPath);


                //Adjust path
                if (savePathIsProjectRelative)
                    savePanelPath = Utilities.ConvertPathToProjectRelative(savePanelPath);


                saveDirectory = Path.GetDirectoryName(savePanelPath);
                saveFileName = Path.GetFileNameWithoutExtension(savePanelPath);
                saveExtension = Path.GetExtension(savePanelPath).ToLowerInvariant().Replace(".", string.Empty);

                node.lastSavedTextureFilePath = savePanelPath;


                return true;
            }


            return false;
        }
        static bool GetShaderSavePathOptions(Node node, out string saveDirectory, out string saveFileName, out bool savePathIsProjectRelative)
        {
            saveDirectory = string.Empty;
            saveFileName = string.Empty;
            savePathIsProjectRelative = false;



            string savePanelPath;
            if (string.IsNullOrWhiteSpace(node.lastSavedShaderFilePath) == false && (File.Exists(node.lastSavedShaderFilePath) || Directory.Exists(Path.GetDirectoryName(node.lastSavedShaderFilePath))))
            {
                savePanelPath = UnityEditor.EditorUtility.SaveFilePanel("Save Shader", Path.GetDirectoryName(node.lastSavedShaderFilePath), Path.GetFileNameWithoutExtension(node.lastSavedShaderFilePath), "shader");
            }
            else
            {
                //Get current ShaderGraph file path
                string graphPath = string.Empty;
                GUID guid;
                if (GUID.TryParse(node.owner.owner.graph.assetGuid, out guid))
                    graphPath = AssetDatabase.GUIDToAssetPath(guid);


                if (File.Exists(graphPath))
                    savePanelPath = UnityEditor.EditorUtility.SaveFilePanel("Save Shader", Path.GetDirectoryName(graphPath), Path.GetFileNameWithoutExtension(graphPath), "shader");
                else
                    savePanelPath = UnityEditor.EditorUtility.SaveFilePanel("Save Shader", "Assets", "New Shader", "shader");
            }

            if (string.IsNullOrWhiteSpace(savePanelPath) == false)
            {
                //Check if path is project relative
                savePathIsProjectRelative = Utilities.IsPathProjectRelative(savePanelPath);


                //Adjust path
                if (savePathIsProjectRelative)
                    savePanelPath = Utilities.ConvertPathToProjectRelative(savePanelPath);


                saveDirectory = Path.GetDirectoryName(savePanelPath);
                saveFileName = Path.GetFileNameWithoutExtension(savePanelPath);

                node.lastSavedShaderFilePath = savePanelPath;


                return true;
            }


            return false;
        }

        static void SaveTexture(Texture2D texture, Enum.Format format, string savePath)
        {
            byte[] bytes;
            switch (format)
            {
                case Enum.Format.JPG: bytes = texture.EncodeToJPG(100); break;
                case Enum.Format.TGA: bytes = texture.EncodeToTGA(); break;
                case Enum.Format.EXR: bytes = texture.EncodeToEXR(Texture2D.EXRFlags.None); break;
                case Enum.Format.EXRZip: bytes = texture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP); break;

                case Enum.Format.PNG:
                default: bytes = texture.EncodeToPNG(); break;
            }

            File.WriteAllBytes(savePath, bytes);
        }
        static Texture2D GetPreviewTexture(Material material, MaterialPropertyBlock materialPropertyBlock, int width, int height, Enum.SuperSize superSize, bool mipChain, Enum.Format format, Enum.Type textureType)
        {
            //Texture resolution
            int superWidth, superHeight;
            GetSuperSizeWidthAndHeight(width, height, superSize, out superWidth, out superHeight);



            //Texture formats
            TextureFormat textureFormat;
            RenderTextureFormat renderTextureFormat;
            GetTextureFormats(format, out textureFormat, out renderTextureFormat);

            //Color space
            RenderTextureReadWrite rtColorSpace = (textureType == Enum.Type.Normal || textureType == Enum.Type.LinearData) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;

                     
            //Create textures
            RenderTexture renderTexture = RenderTexture.GetTemporary(superWidth, superHeight, 16, renderTextureFormat, rtColorSpace);
            RenderTexture renderTextureSuperSize = superSize == Enum.SuperSize.None ? null : RenderTexture.GetTemporary(width, height, 16, renderTextureFormat, rtColorSpace);
            Texture2D texture = new Texture2D(width, height, textureFormat, mipChain);


            RenderTexture.active = null;

            //Setup render camera
            GameObject cameraGO = new GameObject();
            cameraGO.transform.position = Vector3.forward * -1;
            cameraGO.transform.rotation = Quaternion.identity;

            Camera camera = cameraGO.AddComponent<Camera>();
            camera.enabled = false;
            camera.cameraType = CameraType.Preview;
            camera.orthographic = true;
            camera.orthographicSize = 0.5f;
            camera.farClipPlane = 10.0f;
            camera.nearClipPlane = 1.0f;
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.clear;
            camera.renderingPath = RenderingPath.Forward;
            camera.useOcclusionCulling = false;
            camera.allowMSAA = false;
            camera.allowHDR = true;


            camera.targetTexture = renderTexture;
            Graphics.DrawMesh(quadMesh, Matrix4x4.identity, material, 1, camera, 0, materialPropertyBlock, ShadowCastingMode.Off, false, null, false);
            camera.Render();


            if (superSize == Enum.SuperSize.None)
            {
                RenderTexture.active = renderTexture;
            }
            else
            {
                Graphics.Blit(renderTexture, renderTextureSuperSize);

                RenderTexture.active = renderTextureSuperSize;
            }

            
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, mipChain);
            texture.Apply(mipChain);



            //Cleanup
            RenderTexture.active = null;

            GameObject.DestroyImmediate(cameraGO);
            RenderTexture.ReleaseTemporary(renderTexture);
            if (renderTextureSuperSize != null)
                RenderTexture.ReleaseTemporary(renderTextureSuperSize);



            return texture;
        }
        static internal string GetPreviewShader(Node node, bool correctAlpha)
        {
            var wasAsyncAllowed = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = true;


            Generator generator = new Generator(node.owner, node, GenerationMode.ForReals, $"hidden/preview/{node.GetVariableNameForNode()}", null);
            string generatedShader = generator.generatedShader;


            //Correct alpha 
            if (node.FindInputSlot<MaterialSlot>(Node.InputSlotId).concreteValueType == ConcreteSlotValueType.Vector4)
            {
                ShaderStringBuilder shaderStringBuilder = new ShaderStringBuilder();
                node.GenerateNodeCode(shaderStringBuilder, GenerationMode.Preview);
                string sData = shaderStringBuilder.ToString();
                sData = sData.Substring(0, sData.IndexOf(';'));
                if (sData.Contains("$precision4 "))
                {
                    string varName = sData.Substring(sData.IndexOf(" ") + 1);

                    //Linear > RGB
                    //https://docs.unity3d.com/Packages/com.unity.shadergraph@10.10/manual/Colorspace-Conversion-Node.html
                    if (QualitySettings.activeColorSpace == ColorSpace.Linear && correctAlpha)
                        generatedShader = generatedShader.Replace($"{varName}.z, 1.0", $"{varName}.z, ({varName}.w <= 0.0031308 ? ({varName}.w * 12.92) : ((pow(max(abs({varName}.w), 1.192092896e-07), 1.0 / 2.4) * 1.055) - 0.055))");
                    else
                        generatedShader = generatedShader.Replace($"{varName}.z, 1.0", $"{varName}.z, {varName}.w");
                }
            }

            //Restore
            ShaderUtil.allowAsyncCompilation = wasAsyncAllowed;


            return generatedShader;
        }
        static Material GetPreviewMaterial(Node node, MaterialPropertyBlock materialPropertyBlock, Enum.Mode mode)
        {
            HashSet<AbstractMaterialNode> sources = new HashSet<AbstractMaterialNode>() { node };
            HashSet<AbstractMaterialNode> nodesToDraw = new HashSet<AbstractMaterialNode>();
            PreviewManager.PropagateNodes(sources, PreviewManager.PropagationDirection.Upstream, nodesToDraw);

            PooledList<PreviewProperty> perMaterialPreviewProperties = PooledList<PreviewProperty>.Get();
            PreviewManager.CollectPreviewProperties(node.owner, nodesToDraw, perMaterialPreviewProperties, materialPropertyBlock);


            string shaderString = GetPreviewShader(node, true);


            //Check if 'TimeParameter' is used
            bool containsTimeParameter = shaderString.Contains(shaderTimeParameter);

            if ((mode == Enum.Mode.Sequence || mode == Enum.Mode.Atlas || mode == Enum.Mode._2DArray || mode == Enum.Mode.Texture3D) && containsTimeParameter == false)
            {
                Debug.LogError("Cannot create image sequence. ShaderGraph doesn't use 'Time' node.\n");

                return null;
            }

            if (mode == Enum.Mode.Single && containsTimeParameter)
            {
                Debug.LogWarning("ShaderGraph uses 'Time' node. Baked texture may be incorrect.\n");
            }



            Material material = null;
            Shader shader = ShaderUtil.CreateShaderAsset(shaderString);
            if (shader != null && ShaderUtil.ShaderHasError(shader) == false)
            {
                material = new Material(shader);
                PreviewManager.AssignPerMaterialPreviewProperties(material, perMaterialPreviewProperties);
            }
            else
            {
                Debug.LogError("Cannot create shader.\n");
            }

            

            return material;
        }

        static string GetTextureSaveExtension(Enum.Mode mode, Enum.Format format)
        {
            if (mode == Enum.Mode._2DArray || mode == Enum.Mode.Texture3D)
                return "asset";

            if (format == Enum.Format.EXRZip)
                return "exr";
            else
                return format.ToString().ToLowerInvariant();
        }
        static void GetTextureSize(Node node, out int textureSize)
        {
            if (node.Resolution == Enum.Resolution.Custom)
            {
                textureSize = Utilities.TryParseInt(node.GetSlotValue(Node.CustomResolutionSlotID, GenerationMode.ForReals), minTextureSize);
            }
            else
            {
                textureSize = Utilities.TryParseInt(node.Resolution.ToString(), minTextureSize);
            }


            int maxTextureSize = node.Mode == Enum.Mode.Texture3D ? 2048 : SystemInfo.maxTextureSize;

            textureSize = (int)Mathf.Clamp(textureSize, minTextureSize, maxTextureSize);
        }
        static void GetSuperSizeWidthAndHeight(int width, int height, Enum.SuperSize superSize, out int superWidth, out int superHeight)
        {
            if (superSize != Enum.SuperSize.None)
            {
                int scale = superSize == Enum.SuperSize._4 ? 4 : 2;


                if (width == height)
                {
                    if (width * scale < SystemInfo.maxTextureSize)
                        width *= scale;

                    height = width;
                }
                else if (width > height)
                {
                    if (width * scale < SystemInfo.maxTextureSize)
                    {
                        width *= scale;
                        height *= scale;
                    }
                }
                else
                {
                    if (height * scale < SystemInfo.maxTextureSize)
                    {
                        width *= scale;
                        height *= scale;
                    }
                }
            }

            superWidth = width;
            superHeight = height;
        }
        static void GetRowsAndColumns(Node node, out int rows, out int columns)
        {
            Vector2 slot = Utilities.StringToVector2(node.GetSlotValue(Node.RowsAndColumnsID, GenerationMode.ForReals));

            rows = (int)slot.x;
            rows = rows < 1 ? 1 : rows;

            columns = (int)slot.y;
            columns = columns < 1 ? 1 : columns;
        }
        static void GetTextureFormats(Enum.Format format, out TextureFormat textureFormat, out RenderTextureFormat renderTextureFormat)
        {
            if (format == Enum.Format.EXR || format == Enum.Format.EXRZip)
            {
                renderTextureFormat = RenderTextureFormat.ARGBFloat;

                textureFormat = TextureFormat.RGBAFloat;
            }
            else
            {
                renderTextureFormat = RenderTextureFormat.ARGB32;

                textureFormat = TextureFormat.ARGB32;
            }
        }
        static void UpdateImportSettings(string assetPath, Enum.Type textureType)
        {
            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (textureImporter != null)
            {
                if (textureType == Enum.Type.Normal)
                {
                    textureImporter.textureType = TextureImporterType.NormalMap;
                    textureImporter.convertToNormalmap = false;
                }
                else if(textureType == Enum.Type.LinearData)
                {
                    textureImporter.textureType = TextureImporterType.Default;
                    textureImporter.sRGBTexture = false;
                }
                else
                {
                    textureImporter.textureType = TextureImporterType.Default;
                    textureImporter.sRGBTexture = true;
                }

                textureImporter.textureShape = TextureImporterShape.Texture2D;                

                textureImporter.SaveAndReimport();
            }
        }
        static void UpdateTimeNode(MaterialPropertyBlock materialPropertyBlock, float value)
        {
            materialPropertyBlock.SetVector(shaderTimeParameter, new Vector4(value, Mathf.Sin(value), Mathf.Cos(value), value));
        }
    }
}
