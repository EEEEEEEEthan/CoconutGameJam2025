// Shader Graph Baker <https://u3d.as/2VQd>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System;
using UnityEngine;

namespace AmazingAssets.ShaderGraphBaker.Editor
{
    public class Manual : ScriptableObject
    {
        public enum URLType { OpenPage, MailTo }


        public Texture2D icon;
        public string title;
        public Section[] sections;
        public bool loadedLayout;

        [Serializable]
        public class Section
        {
            public string heading, text, linkText, url;
            public URLType urlType;
        }
    }
}
 
