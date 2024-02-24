using System.Collections.Generic;
using UnityEngine;

namespace MaterialPropertyBlockEdit.MaterialComponents.Editor
{
    /// <summary>
    /// serialize された MaterialのParameter を、 MaterialPropertyBlock に変換し、Materialに送るコンポーネントです。
    /// エディタ実行中でも、ランタイム中でも利用することが出来ます。
    /// </summary>
    [ExecuteInEditMode]
    public class MaterialPropertyBlockController : MonoBehaviour
    {
        /// <summary>
        /// MaterialPropertyBlock 適用対称の renderers。
        /// </summary>
        [SerializeField]
        private List<Renderer> renderers = new();

        /// <summary>
        /// serialize された Material の Parameter。
        /// </summary>
        [SerializeField, SerializeReference, HideInInspector]
        public List<MaterialParameterBase> materialParameters = new();

        /// <summary>
        /// Update Event で MaterialのParameter を毎回更新するかどうかを決めます。
        /// falseにするとパフォーマンス向上に繋がりますが、serialize された MaterialのParameter を動的に更新しても 対象Material には反映されません
        /// 手動で SetMaterialPropertyBlock を呼び出して更新することもできます。
        /// </summary>
        [SerializeField]
        private bool enableAutoUpdate = false;

        private MaterialPropertyBlock materialPropertyBlock;

        /// <summary>
        /// 動的にMaterialPropertyBlock 適用対称の rendererを追加します。
        /// </summary>
        public void AddRenderers(Renderer renderer)
        {
            renderer.SetPropertyBlock(materialPropertyBlock);
            renderers.Add(renderer);
        }

        /// <summary>
        /// 動的にMaterialPropertyBlock 適用対称の rendererを削除します。
        /// </summary>
        public void RemoveRenderers(Renderer renderer)
        {
            if (renderers.Remove(renderer))
            {
                renderer.SetPropertyBlock(null);
            }
        }

        public IReadOnlyList<Renderer> GetRenderers() => renderers;

        private void Update()
        {
            if (enableAutoUpdate)
            {
                SetMaterialPropertyBlock();
            }
        }

        public void OnValidate()
        {
            SetMaterialPropertyBlock();
        }

        public void OnEnable()
        {
            SetMaterialPropertyBlock();
        }

        private void OnDisable()
        {
            Clear();
        }

        private void OnDestroy()
        {
            Clear();
        }

        /// <summary>
        /// serialize された MaterialのParameter を、 MaterialPropertyBlock に変換し、対象Material に送る処理を実行します
        /// </summary>
        public void SetMaterialPropertyBlock()
        {
            bool isChanged = materialPropertyBlock == null;
            materialPropertyBlock ??= new MaterialPropertyBlock();

            for (var index = 0; index < materialParameters.Count; index++)
            {
                var materialParameter = materialParameters[index];
                if (materialParameter.IsMissing == false)
                {
                    isChanged |= materialParameter.Apply(materialPropertyBlock);
                }
            }

            if (isChanged)
            {
                for (int index = 0; index < renderers.Count; index++)
                {
                    renderers[index].SetPropertyBlock(materialPropertyBlock);
                }
            }
        }

        /// <summary>
        /// MaterialPropertyBlockをクリアして、rendererとの紐づきを解除します。
        /// </summary>
        public void Clear()
        {
            if (materialPropertyBlock != null)
            {
                materialPropertyBlock = null;

                foreach (var materialParameter in materialParameters)
                {
                    materialParameter.ClearCacheValue();
                }

                foreach (var renderer in renderers)
                {
                    renderer.SetPropertyBlock(null);
                }
            }
        }
    }
}