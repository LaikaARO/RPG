#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor script para validar tags e layers ao iniciar o editor
/// </summary>
[InitializeOnLoad]
public class TagsAndLayersValidator
{
    static TagsAndLayersValidator()
    {
        EditorApplication.delayCall += () =>
        {
            TagsAndLayers.ValidateTagsAndLayers();
        };
    }
}
#endif