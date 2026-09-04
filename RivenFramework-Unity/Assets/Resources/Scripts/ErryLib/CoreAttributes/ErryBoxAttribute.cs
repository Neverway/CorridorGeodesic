using UnityEngine;

[AttributeNeedsPolymorphicDrawerToIgnoreContentsButDrawDropdown]
public class ErryBoxAttribute : PropertyAttribute
{
    public bool label = true;
    public bool box = true;
    public bool foldout = true;

    public ErryBoxAttribute() { }
}

[AttributeNeedsPolymorphicDrawerToIgnoreContentsButDrawDropdown]
public class ErryUnboxAttribute : ErryBoxAttribute
{
    public ErryUnboxAttribute() 
    {
        box = false;
    }
}