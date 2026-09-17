using UnityEditor.Rendering;
using UnityEngine;

public class BuildShaderStripper : IShaderVariantStripper
{
    public bool active => true;
    
    public bool CanRemoveVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data)
    {
        string pass = snippet.passName;
        if (pass == "BuiltIn Forward" || pass == "BuiltIn ForwardAdd"|| pass == "BuiltIn Deferred" || pass == "Always")
        {
            return true;
        }
        return false;
    }
}
