# UnityMaterialPropertyBlockEdit
unityのマテリアルを複製せずにインスペクターからプロパティを操作 and シリアライズ保存できるやつ。

これを使えば、Unity組み込みのマテリアルだってプロパティを上書きして編集できます。

そう、、MaterialPropertyBlockならね！

Unity 2022からは、アニメーションでプロパティを操作することもできる。

(Unity 2022からSerialized Referenceがアニメーションで参照できるようになったからね)

## Install
Unity->Window->Package Manager -> "+" DropDown -> Add package from git URL
`https://github.com/MineMonsterLabo/UnityMaterialPropertyBlockEdit.git?path=Packages/MaterialPropertyBlockEdit`

## How To

Use the MaterialPropertyBlockController component.

Then set the Renderer for that component.
