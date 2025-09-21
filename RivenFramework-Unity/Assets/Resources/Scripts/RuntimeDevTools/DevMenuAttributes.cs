using RivenFramework.Utils.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DevMenuAttributes
{
    public static void TestMethod()
    {
        ReflectionCache.GetAttributeUsages<DevMenuItem>();
    }
}

public abstract class DevMenuItem : Attribute
{
    public string name = "";
    public string tab = "";
    public string group = "";
}

public class DevMenuButton : DevMenuItem
{

}
public class DevMenuToggle : DevMenuItem
{

}
public class DevMenuSlider : DevMenuItem
{

}