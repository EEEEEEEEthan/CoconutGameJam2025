// Shader Graph Baker <https://u3d.as/2VQd>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;


namespace AmazingAssets.ShaderGraphBaker.Editor
{
    static internal class Utilities
    {
        static internal void SaveAssetUsingTempFolder(Texture texture, string originalPath)
        {
            //Create asset in the TEMP folder and then copy it to the required directory
            string tempFolderFolder = "Assets/TEMP";
            bool folderExists = AssetDatabase.IsValidFolder(tempFolderFolder);
            if (folderExists == false)
                AssetDatabase.CreateFolder("Assets", "TEMP");

            string tempSavePath = Path.Combine(tempFolderFolder, Path.GetFileName(originalPath));
            AssetDatabase.CreateAsset(texture, tempSavePath);

            //Copy file
            File.Copy(tempSavePath, originalPath, true);

            //Delete temp file
            File.Delete(tempSavePath);

            //Remove meta
            if (File.Exists(tempSavePath + ".meta"))
                File.Delete(tempSavePath + ".meta");

            //Delete temp folder
            if (folderExists == false)
            {
                Directory.Delete(tempFolderFolder);

                //Remove meta
                if (File.Exists(tempFolderFolder + ".meta"))
                    File.Delete(tempFolderFolder + ".meta");
            }

            AssetDatabase.Refresh();
        }
        static internal string ConvertPathToProjectRelative(string path)
        {
            //Before using this method, make sure path 'is' project relative

            return NormalizePath("Assets" + path.Substring(Application.dataPath.Length));
        }
        static internal bool IsPathProjectRelative(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return NormalizePath(path).Contains(NormalizePath(Application.dataPath));
        }
        static internal string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;
            else
                return path.Replace("//", "/").Replace("\\\\", "/").Replace("\\", "/");
        }

        static internal Vector2 StringToVector2(string inputSlot)
        {
            Vector2 result = Vector2.zero;

            if (string.IsNullOrWhiteSpace(inputSlot) == false)
            {
                //inputSlot should be something like this: $precision2 (10, 1)
                inputSlot = inputSlot.Substring(inputSlot.IndexOf('('));

                //remove '(' & ')'
                inputSlot = inputSlot.Replace("(", string.Empty).Replace(")", string.Empty).Trim();

                //Split
                string[] sArray = inputSlot.Split(',');

                //Try read values
                float fValue;
                if (float.TryParse(sArray[0], out fValue))
                    result.x = fValue;

                if (float.TryParse(sArray[1], out fValue))
                    result.y = fValue;
            }


            return result;
        }

        static internal float TryParseFloat(string value, float defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;


            value = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

            float retValue = 0;
            if (float.TryParse(value, out retValue) == false)
                retValue = defaultValue;

            return retValue;
        }
        static internal int TryParseInt(string value, int defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;


            value = new string(value.Where(c => char.IsDigit(c)).ToArray());

            int retValue = 0;
            if (int.TryParse(value, out retValue) == false)
                retValue = defaultValue;

            return retValue;
        }
    }
}
