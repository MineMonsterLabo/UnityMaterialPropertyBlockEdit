using System;
using UnityEngine;

namespace MaterialPropertyBlockEdit.MaterialComponents
{
    [Serializable]
    public abstract class MaterialParameterBase
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private bool isMissing;

        [NonSerialized]
        private int? nameId;

        public int NameId => nameId ??= Shader.PropertyToID(Name);
        public string Name => name;

        public bool IsMissing
        {
            get => isMissing;
            set => isMissing = value;
        }

        protected MaterialParameterBase(string name) => this.name = name;

        public abstract bool Apply(MaterialPropertyBlock materialPropertyBlock);
        public abstract void ClearCacheValue();

#if UNITY_EDITOR
        public abstract UnityEditor.MaterialProperty.PropType PropType { get; }

        public bool CheckSameElement(UnityEditor.MaterialProperty materialProperty)
            => materialProperty.name == Name && materialProperty.type == PropType;

        public void OnInspectorGUI(UnityEditor.SerializedProperty serializedProperty)
        {
            using (new UnityEditor.EditorGUILayout.HorizontalScope())
            {
                if (isMissing)
                {
                    var style = new GUIStyle(UnityEditor.EditorStyles.label);
                    style.normal.textColor = Color.yellow;
                    UnityEditor.EditorGUILayout.LabelField("[Missing]", style, GUILayout.Width(55));
                }

                OnInspectorGUIInternal(serializedProperty);
            }
        }

        public virtual void OnInspectorGUIInternal(UnityEditor.SerializedProperty serializedProperty)
        {
            UnityEditor.EditorGUILayout.PropertyField
                (serializedProperty.FindPropertyRelative("value"), new GUIContent(Name));
        }

        public virtual bool Update(UnityEditor.MaterialProperty materialProperty) => false;

        public static MaterialParameterBase Create(UnityEditor.MaterialProperty materialProperty)
        {
            return materialProperty.type switch
            {
                UnityEditor.MaterialProperty.PropType.Color => new ColorMaterialParameter
                    (materialProperty.name, materialProperty.colorValue),
                UnityEditor.MaterialProperty.PropType.Range =>
                    new RangeMaterialParameter
                    (
                        materialProperty.name,
                        value: materialProperty.floatValue,
                        minValue: materialProperty.rangeLimits.x,
                        maxValue: materialProperty.rangeLimits.y
                    ),
                UnityEditor.MaterialProperty.PropType.Float =>
                    new FloatMaterialParameter(materialProperty.name, materialProperty.floatValue),
                UnityEditor.MaterialProperty.PropType.Int =>
                    new IntMaterialParameter(materialProperty.name, materialProperty.intValue),
                UnityEditor.MaterialProperty.PropType.Vector =>
                    new VectorMaterialParameter(materialProperty.name, materialProperty.vectorValue),
                UnityEditor.MaterialProperty.PropType.Texture =>
                    new TextureMaterialParameter(materialProperty.name, materialProperty.textureValue),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

#endif
    }

    // Serializable Color Material Property ---------------
    [Serializable]
    public class ColorMaterialParameter : MaterialParameterBase
    {
        [SerializeField]
        private Color value;

        private Color? beforeValue;

        public ColorMaterialParameter(string name, Color value) : base(name)
        {
        }

        public override bool Apply(MaterialPropertyBlock materialPropertyBlock)
        {
            if (beforeValue != value)
            {
                materialPropertyBlock.SetColor(NameId, value);
                beforeValue = value;
                return true;
            }

            return false;
        }

        public override void ClearCacheValue()
        {
            beforeValue = null;
        }

#if UNITY_EDITOR
        public override UnityEditor.MaterialProperty.PropType PropType => UnityEditor.MaterialProperty.PropType.Color;
#endif
    }

    // Serializable Range Material Property ---------------
    [Serializable]
    public class RangeMaterialParameter : MaterialParameterBase
    {
        [SerializeField]
        private float value;

        private float? beforeValue;

        [SerializeField]
        private float minValue;

        [SerializeField]
        private float maxValue;

        public RangeMaterialParameter(string name, float value, float minValue, float maxValue) : base(name)
        {
            this.value = value;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override bool Apply(MaterialPropertyBlock materialPropertyBlock)
        {
            if (beforeValue.HasValue == false || Mathf.Approximately(beforeValue.Value, value) == false)
            {
                materialPropertyBlock.SetFloat(NameId, value);
                beforeValue = value;
                return true;
            }

            return false;
        }

        public override void ClearCacheValue()
        {
            beforeValue = null;
        }

#if UNITY_EDITOR
        public override UnityEditor.MaterialProperty.PropType PropType => UnityEditor.MaterialProperty.PropType.Range;

        public override void OnInspectorGUIInternal(UnityEditor.SerializedProperty serializedProperty)
        {
            UnityEditor.EditorGUILayout.Slider
                (serializedProperty.FindPropertyRelative("value"), minValue, maxValue, new GUIContent(Name));
        }

        public override bool Update(UnityEditor.MaterialProperty materialProperty)
        {
            var isNoChanged = Mathf.Approximately(minValue, materialProperty.rangeLimits.x) && Mathf.Approximately
                (maxValue, materialProperty.rangeLimits.y);
            if (isNoChanged)
            {
                return false;
            }

            minValue = materialProperty.rangeLimits.x;
            maxValue = materialProperty.rangeLimits.y;

            return true;
        }
#endif
    }

    // Serializable Float Material Property ---------------
    [Serializable]
    public class FloatMaterialParameter : MaterialParameterBase
    {
        [SerializeField]
        private float value;

        private float? beforeValue;

        public FloatMaterialParameter(string name, float value) : base(name) => this.value = value;

        public override bool Apply(MaterialPropertyBlock materialPropertyBlock)
        {
            if (beforeValue.HasValue == false || Mathf.Approximately(beforeValue.Value, value) == false)
            {
                materialPropertyBlock.SetFloat(NameId, value);
                beforeValue = value;
                return true;
            }

            return false;
        }

        public override void ClearCacheValue()
        {
            beforeValue = null;
        }

#if UNITY_EDITOR
        public override UnityEditor.MaterialProperty.PropType PropType => UnityEditor.MaterialProperty.PropType.Float;
#endif
    }

    // Serializable Int Material Property ---------------
    [Serializable]
    public class IntMaterialParameter : MaterialParameterBase
    {
        [SerializeField]
        private int value;

        private int? beforeValue;

        public IntMaterialParameter(string name, int value) : base(name) => this.value = value;

        public override bool Apply(MaterialPropertyBlock materialPropertyBlock)
        {
            if (beforeValue != value)
            {
                materialPropertyBlock.SetInt(NameId, value);
                beforeValue = value;
                return true;
            }

            return false;
        }

        public override void ClearCacheValue()
        {
            beforeValue = null;
        }

#if UNITY_EDITOR
        public override UnityEditor.MaterialProperty.PropType PropType => UnityEditor.MaterialProperty.PropType.Int;
#endif
    }

    // Serializable Vector Material Property ---------------
    [Serializable]
    public class VectorMaterialParameter : MaterialParameterBase
    {
        [SerializeField]
        private Vector4 value;

        private Vector4? beforeValue;

        public VectorMaterialParameter(string name, Vector4 value) : base(name) => this.value = value;

        public override bool Apply(MaterialPropertyBlock materialPropertyBlock)
        {
            if (beforeValue != value)
            {
                materialPropertyBlock.SetVector(NameId, value);
                beforeValue = value;
                return true;
            }

            return false;
        }

        public override void ClearCacheValue()
        {
            beforeValue = null;
        }

#if UNITY_EDITOR
        public override UnityEditor.MaterialProperty.PropType PropType => UnityEditor.MaterialProperty.PropType.Vector;
#endif
    }

    // Serializable Texture Material Property ---------------
    [Serializable]
    public class TextureMaterialParameter : MaterialParameterBase
    {
        [SerializeField]
        private Texture value;

        private Texture beforeValue;

        public TextureMaterialParameter(string name, Texture value) : base(name) => this.value = value;

        public override bool Apply(MaterialPropertyBlock materialPropertyBlock)
        {
            if (beforeValue != value)
            {
                materialPropertyBlock.SetTexture(NameId, value);
                beforeValue = value;
                return true;
            }

            return false;
        }

        public override void ClearCacheValue()
        {
            beforeValue = null;
        }

#if UNITY_EDITOR
        public override UnityEditor.MaterialProperty.PropType PropType => UnityEditor.MaterialProperty.PropType.Texture;

        public override void OnInspectorGUIInternal(UnityEditor.SerializedProperty serializedProperty)
        {
            UnityEditor.EditorGUILayout.ObjectField
                (serializedProperty.FindPropertyRelative("value"), new GUIContent(Name));
        }
#endif
    }
}