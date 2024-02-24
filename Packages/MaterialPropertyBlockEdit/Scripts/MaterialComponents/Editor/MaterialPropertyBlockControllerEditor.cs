using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MaterialPropertyBlockEdit.MaterialComponents.Editor
{
    [CustomEditor(typeof(MaterialPropertyBlockController))]
    class MaterialPropertyBlockControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            var controller = (MaterialPropertyBlockController)target;
            var renderers = controller.GetRenderers().Where(renderer => renderer);

            var materials = renderers.Select(renderer => (Object)renderer.sharedMaterial).ToArray();
            var materialProperties = MaterialEditor.GetMaterialProperties(materials);

            if (materialProperties == null)
            {
                return;
            }

            controller.materialParameters ??= new List<MaterialParameterBase>();

            UpdateDiffParameters(controller, materialProperties);
            UpdateParameters(controller, materialProperties);

            if (GUILayout.Button("Delete Missing"))
            {
                DeleteMissingParameters(controller);
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            RenderParameters(serializedObject.FindProperty(nameof(MaterialPropertyBlockController.materialParameters)));

            if (EditorGUI.EndChangeCheck())
            {
                controller.OnValidate();
                EditorUtility.SetDirty(controller);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateDiffParameters
            (MaterialPropertyBlockController controller, IReadOnlyList<MaterialProperty> materialProperties)
        {
            var materialParameters = controller.materialParameters;

            for (int index = 0; index < materialParameters.Count; index++)
            {
                var parameter = materialParameters[index];

                if (materialProperties.Any(parameter.CheckSameElement) == false)
                {
                    parameter.IsMissing = true;
                    EditorUtility.SetDirty(controller);
                }
                else if (parameter.IsMissing && materialProperties.Any(parameter.CheckSameElement))
                {
                    parameter.IsMissing = false;
                    EditorUtility.SetDirty(controller);
                }
            }


            var addParameters =
                materialProperties
                    .Where
                        (property => materialParameters.Any(parameter => parameter.CheckSameElement(property)) == false)
                    .Select(MaterialParameterBase.Create);

            foreach (var addParameter in addParameters)
            {
                materialParameters.Add(addParameter);
                EditorUtility.SetDirty(controller);
            }
        }

        private static void UpdateParameters
            (MaterialPropertyBlockController controller, IReadOnlyList<MaterialProperty> materialProperties)
        {
            foreach (var materialParameter in controller.materialParameters)
            {
                if (
                    materialParameter.IsMissing == false &&
                    materialParameter.Update(materialProperties.First(materialParameter.CheckSameElement)))
                {
                    EditorUtility.SetDirty(controller);
                }
            }
        }

        private static void DeleteMissingParameters(MaterialPropertyBlockController controller)
        {
            var parameters = controller.materialParameters;
            var removeParameters = parameters.Where(parameter => parameter.IsMissing).ToArray();

            foreach (var removeParameter in removeParameters)
            {
                parameters.Remove(removeParameter);
                EditorUtility.SetDirty(controller);
            }
        }

        private void RenderParameters(SerializedProperty materialParametersSerializedProperty)
        {
            var arraySize = materialParametersSerializedProperty.arraySize;

            for (int index = 0; index < arraySize; index++)
            {
                var serializedProperty = materialParametersSerializedProperty.GetArrayElementAtIndex(index);
                var materialParameter = (MaterialParameterBase)serializedProperty.managedReferenceValue;
                materialParameter.OnInspectorGUI(serializedProperty);
            }
        }
    }
}