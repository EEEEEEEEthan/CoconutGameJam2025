// Shader Graph Baker <https://u3d.as/2VQd>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
using System.Collections.Generic;

using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;
using UnityEditor.ShaderGraph.Drawing.Controls;


namespace AmazingAssets.ShaderGraphBaker.Editor
{
    internal class Enum
    {
        public enum Mode { Single, Sequence, Atlas, [InspectorName("2D Array")] _2DArray, [InspectorName("Texture 3D")] Texture3D }
        public enum Type {[InspectorName("Color Map")] ColorMap, Normal, [InspectorName("Linear Data")] LinearData }        
        public enum Format { JPG, PNG, TGA, EXR, EXRZip}
        public enum Resolution 
        {
            [InspectorName("16")] _16,
            [InspectorName("32")] _32,
            [InspectorName("64")] _64,
            [InspectorName("128")] _128,
            [InspectorName("256")] _256,
            [InspectorName("512")] _512,
            [InspectorName("1024")] _1024,
            [InspectorName("2048")] _2048,
            [InspectorName("4096")] _4096,
            [InspectorName("8192")] _8192,

            Custom 
        }
        public enum SuperSize 
        { 
            None,

            [InspectorName("2")] _2,
            [InspectorName("4")] _4 
        }
    }

    [Title("Amazing Assets", "Shader Graph Baker")]
    class Node : AbstractMaterialNode, IGeneratesBodyCode, IGeneratesFunction
    {
        public override string documentationURL => AssetInfo.assetManualLocation;

         
        public override bool hasPreview { get { return true; } }


        public const int InputSlotId = 0;
        public const int ImageCountSlotID = 1;
        public const int SlicesSlotID = 2;
        public const int RowsAndColumnsID = 3;
        public const int TimeInitialValueSlotID = 4;
        public const int TimeIncrementSlotID = 5;        
        public const int CustomResolutionSlotID = 6;
        const int OutputSlotId = 7;
        const string kInputSlotName = "Input";
        const string kImageCountSlotName = "Image Count";
        const string kSlicesSlotName = "Slices";
        const string kRowsAndColumnsSlotName = "Rows & Columns";
        const string kTimeInitialValueSlotName = "Time Initial Value";
        const string kTimeIncrementSlotName = "Time Increment";        
        const string kCustomResolutionSlotName = "Custom Resolution";
        const string kOutputSlotName = "Out";


        [SerializeField] float m_ImageCount = 10;
        [SerializeField] float m_Slices = 64;
        [SerializeField] float m_Rows = 10;
        [SerializeField] float m_Columns = 1;
        [SerializeField] float m_TimeInitialValue = 0;
        [SerializeField] float m_TimeIncrement = 1;        
        [SerializeField] float m_CustomResolution = 100;

        static bool isBaking = false;
        public string lastSavedTextureFilePath = string.Empty;
        public string lastSavedShaderFilePath = string.Empty;


        [SerializeField]
        private Enum.Mode m_Mode = Enum.Mode.Single;
        [EnumControl("Mode")]
        public Enum.Mode Mode
        {
            get { return m_Mode; }
            set 
            {
                if (m_Mode == value)
                    return;

                m_Mode = value;

                UpdateNodeAfterDeserialization();
                Dirty(ModificationScope.Node);
            }
        } 
            
        [SerializeField]
        private Enum.Type m_TextureType = Enum.Type.ColorMap;
        [EnumControl("Type")]
        public Enum.Type TextureType
        {
            get { return m_TextureType; }
            set
            {
                if (m_TextureType == value)
                    return;

                m_TextureType = value;

                UpdateTextureBakerButtonName();
                UpdateShaderBakerButtonName();

                Dirty(ModificationScope.Topological);
            }
        }
               
        [SerializeField]
        private Enum.Resolution m_Resolution = Enum.Resolution._1024;
        [EnumControl("Resolution")]
        public Enum.Resolution Resolution
        {
            get { return m_Resolution; }
            set
            {
                if (m_Resolution == value)
                    return;

                m_Resolution = value;

                UpdateNodeAfterDeserialization();
                Dirty(ModificationScope.Node);
            }
        }

        [SerializeField]
        private Enum.SuperSize m_SuperSize = Enum.SuperSize.None;
        [EnumControl("Super Size")]
        public Enum.SuperSize SuperSize
        {
            get { return m_SuperSize; }
            set
            {
                if (m_SuperSize == value)
                    return;

                m_SuperSize = value;

                Dirty(ModificationScope.Node);
            }
        }

        [SerializeField]
        private Enum.Format m_Format = Enum.Format.PNG;
        [EnumControl("Format")]
        public Enum.Format Format
        {
            get { return m_Format; }
            set
            {
                if (m_Format == value)
                    return;

                m_Format = value;

                m_OutputTexture.texture = null;

                UpdateTextureBakerButtonName();
                UpdateShaderBakerButtonName();

                Dirty(ModificationScope.Node);
            }
        }

        [ButtonControl("InitTextureBakerButton", "TextureBakerButtonCallback")]
        int textureBakerButtonControll { get; set; }
        public UnityEngine.UIElements.Button m_TextureBakerButton;
        
        [SerializeField]
        private SerializableTexture m_OutputTexture = new SerializableTexture();
        [TextureControl("")]
        public Texture OutputTexture
        {
            get
            { 
                return m_OutputTexture.texture;
            }

            set
            {
                m_OutputTexture.texture = value;

                UpdateTextureBakerButtonName();
                Dirty(ModificationScope.Node);
            }
        }

        [ButtonControl("InitShaderBakerButton", "ShaderBakerButtonCallback")]
        int shaderBakerButtonControll { get; set; }
        public UnityEngine.UIElements.Button m_ShaderBakerButton;



        public Node()
        {
            name = "Shader Graph Baker";

            m_PreviewMode = PreviewMode.Preview2D;

            UpdateNodeAfterDeserialization();
        }
        public override void ValidateNode()
        {
            base.ValidateNode();

            UpdateTextureBakerButtonName();
            UpdateShaderBakerButtonName();
        }
        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new DynamicVectorMaterialSlot(InputSlotId, kInputSlotName, kInputSlotName, SlotType.Input, Vector3.zero));
            AddSlot(new DynamicVectorMaterialSlot(OutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output, Vector3.zero, ShaderStageCapability.All, true));

            List<int> slotIDs = new List<int> { InputSlotId, OutputSlotId };


            RemoveSlotsNameNotMatching(slotIDs.ToArray(), true);

            switch (Mode)
            {
                case Enum.Mode.Sequence:
                    {
                        AddSlot(new Vector1MaterialSlot(ImageCountSlotID, kImageCountSlotName, kImageCountSlotName, SlotType.Input, m_ImageCount, ShaderStageCapability.All, string.Empty));
                        AddSlot(new Vector1MaterialSlot(TimeInitialValueSlotID, kTimeInitialValueSlotName, kTimeInitialValueSlotName, SlotType.Input, m_TimeInitialValue, ShaderStageCapability.All, string.Empty));
                        AddSlot(new Vector1MaterialSlot(TimeIncrementSlotID, kTimeIncrementSlotName, kTimeIncrementSlotName, SlotType.Input, m_TimeIncrement, ShaderStageCapability.All, string.Empty));

                        slotIDs.Add(ImageCountSlotID);
                        slotIDs.Add(TimeInitialValueSlotID);
                        slotIDs.Add(TimeIncrementSlotID);
                    }
                    break;

                case Enum.Mode._2DArray:
                case Enum.Mode.Texture3D:
                    {
                        AddSlot(new Vector1MaterialSlot(SlicesSlotID, kSlicesSlotName, kSlicesSlotName, SlotType.Input, m_Slices, ShaderStageCapability.All, string.Empty));
                        AddSlot(new Vector1MaterialSlot(TimeInitialValueSlotID, kTimeInitialValueSlotName, kTimeInitialValueSlotName, SlotType.Input, m_TimeInitialValue, ShaderStageCapability.All, string.Empty));
                        AddSlot(new Vector1MaterialSlot(TimeIncrementSlotID, kTimeIncrementSlotName, kTimeIncrementSlotName, SlotType.Input, m_TimeIncrement, ShaderStageCapability.All, string.Empty));                        

                        slotIDs.Add(SlicesSlotID);
                        slotIDs.Add(TimeInitialValueSlotID);
                        slotIDs.Add(TimeIncrementSlotID);                        
                    }
                    break;

                case Enum.Mode.Atlas:
                    {
                        AddSlot(new Vector2MaterialSlot(RowsAndColumnsID, kRowsAndColumnsSlotName, kRowsAndColumnsSlotName, SlotType.Input, new Vector2(m_Rows, m_Columns), ShaderStageCapability.All, "Rows", "Columns"));
                        AddSlot(new Vector1MaterialSlot(TimeInitialValueSlotID, kTimeInitialValueSlotName, kTimeInitialValueSlotName, SlotType.Input, m_TimeInitialValue, ShaderStageCapability.All, string.Empty));
                        AddSlot(new Vector1MaterialSlot(TimeIncrementSlotID, kTimeIncrementSlotName, kTimeIncrementSlotName, SlotType.Input, m_TimeIncrement, ShaderStageCapability.All, string.Empty));                        

                        slotIDs.Add(RowsAndColumnsID);
                        slotIDs.Add(TimeInitialValueSlotID);
                        slotIDs.Add(TimeIncrementSlotID);                        
                    }
                    break;

                default:
                    break;
            }

            if (Resolution == Enum.Resolution.Custom)
            {
                AddSlot(new Vector1MaterialSlot(CustomResolutionSlotID, kCustomResolutionSlotName, kCustomResolutionSlotName, SlotType.Input, m_CustomResolution, ShaderStageCapability.All, string.Empty));

                slotIDs.Add(CustomResolutionSlotID);
            }


            RemoveSlotsNameNotMatching(slotIDs.ToArray(), true);
        }
        string GetFunctionName()
        {
            return $"ShaderGraphBaker_{FindSlot<MaterialSlot>(InputSlotId).concreteValueType.ToShaderString()}";
        }
        public void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            var inputValue = GetSlotValue(InputSlotId, generationMode);
            var outputValue = GetVariableNameForSlot(OutputSlotId);
            sb.AppendLine("{0} {1};", FindOutputSlot<MaterialSlot>(OutputSlotId).concreteValueType.ToShaderString(), outputValue);

            sb.AppendLine("{0}({1}, {2});", GetFunctionName(), inputValue, outputValue);
        }
        public void GenerateNodeFunction(FunctionRegistry registry, GenerationMode generationMode)
        {
            registry.ProvideFunction(GetFunctionName(), s =>
            {
                s.AppendLine("void {0}({1} In, out {2} Out)",
                    GetFunctionName(),
                    FindInputSlot<MaterialSlot>(InputSlotId).concreteValueType.ToShaderString(),
                    FindOutputSlot<MaterialSlot>(OutputSlotId).concreteValueType.ToShaderString());
                using (s.BlockScope())
                {
                    s.AppendLine(GetNodeFunctionBody());
                }
            });
        }           
        public override void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
        {
            base.CollectPreviewMaterialProperties(properties);

             
            switch (Mode)
            {
                case Enum.Mode.Sequence:
                    {
                        m_ImageCount = Utilities.TryParseFloat(GetSlotValue(ImageCountSlotID, GenerationMode.ForReals), 1);
                        m_TimeInitialValue = Utilities.TryParseFloat(GetSlotValue(TimeInitialValueSlotID, GenerationMode.ForReals), 0);
                        m_TimeIncrement = Utilities.TryParseFloat(GetSlotValue(TimeIncrementSlotID, GenerationMode.ForReals), 1);
                    }
                    break;

                case Enum.Mode._2DArray:
                case Enum.Mode.Texture3D:
                    {
                        m_Slices = Utilities.TryParseFloat(GetSlotValue(SlicesSlotID, GenerationMode.ForReals), 1);
                        m_TimeInitialValue = Utilities.TryParseFloat(GetSlotValue(TimeInitialValueSlotID, GenerationMode.ForReals), 0);
                        m_TimeIncrement = Utilities.TryParseFloat(GetSlotValue(TimeIncrementSlotID, GenerationMode.ForReals), 1);                        
                    }
                    break; 

                case Enum.Mode.Atlas:
                    {
                        Vector2 rowsAndColumns = Utilities.StringToVector2(GetSlotValue(RowsAndColumnsID, GenerationMode.ForReals));
                        m_Rows = rowsAndColumns.x;
                        m_Columns = rowsAndColumns.y;

                        m_TimeInitialValue = Utilities.TryParseFloat(GetSlotValue(TimeInitialValueSlotID, GenerationMode.ForReals), 0);
                        m_TimeIncrement = Utilities.TryParseFloat(GetSlotValue(TimeIncrementSlotID, GenerationMode.ForReals), 1);                        
                    }
                    break;
                default:
                    break;
            }

            if (Resolution == Enum.Resolution.Custom)
            {
                m_CustomResolution = Utilities.TryParseFloat(GetSlotValue(CustomResolutionSlotID, GenerationMode.ForReals), 1024);
            }
        }
        string GetNodeFunctionBody()
        {
            if (TextureType == Enum.Type.Normal)
            {
                if (FindInputSlot<MaterialSlot>(InputSlotId).concreteValueType == ConcreteSlotValueType.Vector3)
                {
                    if (isBaking)
                    {
                        return "{Out = float3(In.r * 0.5 + 0.5, In.g * 0.5 + 0.5, 1);}";
                    }
                    else
                    {
                        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                        {
                            string shaderString = "{ float3 n = In.rgb * 0.5 + 0.5;" +
                                                  "  float3 linearRGBLo = n / 12.92;" +
                                                  "  float3 linearRGBHi = pow(max(abs((n + 0.055) / 1.055), 1.192092896e-07), float3(2.4, 2.4, 2.4)); " +
                                                  "  Out.rgb = float3(n <= 0.04045) ? linearRGBLo : linearRGBHi;}";

                            return shaderString;
                        }
                        else
                        {
                            return "{Out = float3(In.r * 0.5 + 0.5, In.g * 0.5 + 0.5, 1);}";
                        }
                    }
                }
                else 
                {
                    return "{Out = 0;}";
                }
            }
            else
            {
                return "{Out = In;}";
            }
        }
         
        public void InitTextureBakerButton(UnityEngine.UIElements.Button button)
        {
            m_TextureBakerButton = button;

            UpdateTextureBakerButtonName();
        }
        public void UpdateTextureBakerButtonName()
        {
            if (m_TextureBakerButton != null)
            {
                bool buttonEnabled = true;

                m_TextureBakerButton.text = m_OutputTexture.texture == null ? "Bake" : "Overwrite";

                if (TextureType == Enum.Type.ColorMap)
                {
                    m_TextureBakerButton.text += " (Color Map)";
                }
                else if(TextureType == Enum.Type.LinearData)
                {
                    m_TextureBakerButton.text += " (Linear Data)";
                }
                else //normal
                {
                    if (CanBakeNormal())
                    {
                        m_TextureBakerButton.text += " (Normal)";
                    }
                    else
                    {
                        buttonEnabled = false;
                        m_TextureBakerButton.text = $"Cannot Bake Normal From {FindInputSlot<MaterialSlot>(InputSlotId).concreteValueType.ToString().Replace("Vector", "In(") + ")"}";
                    }
                }


                m_TextureBakerButton.SetEnabled(buttonEnabled);
            }
        }
        public void TextureBakerButtonCallback()
        {
            if (TextureType == Enum.Type.Normal && CanBakeNormal() == false)
            {
                Debug.LogError($"Cannot Bake Normal From {FindInputSlot<MaterialSlot>(0).concreteValueType.ToString().Replace("Vector", "In(") + ")"}\n");
                return;
            }

            isBaking = true;
            {
                TextureBaker.BakeTexture(this);
            }
            isBaking = false;
        }

        public void InitShaderBakerButton(UnityEngine.UIElements.Button button)
        {
            m_ShaderBakerButton = button;

            UpdateShaderBakerButtonName();
        }
        public void UpdateShaderBakerButtonName()
        {
            if (m_ShaderBakerButton != null)
            {
                bool buttonEnabled = true;

                m_ShaderBakerButton.text = "Bake Shader";


                if (TextureType == Enum.Type.ColorMap || TextureType == Enum.Type.LinearData || CanBakeNormal())
                    buttonEnabled = true;
                else
                  
                    buttonEnabled = false;


                m_ShaderBakerButton.SetEnabled(buttonEnabled);
            }
        }
        public void ShaderBakerButtonCallback()
        {
            isBaking = true;
            {
                TextureBaker.BakeShader(this);
            }
            isBaking = false;
        }

        public bool CanBakeNormal()
        {
            if (TextureType != Enum.Type.Normal)
                return false;


            switch (FindInputSlot<MaterialSlot>(InputSlotId).concreteValueType)
            {
                case ConcreteSlotValueType.Vector3:           
                    return true;

                default: return false;
            }
        }
    }
}
