using MarkupAttributes;
using System;
using UnityEngine;

public class TESTEST : MonoBehaviour
{

    [SerializeReference, Polymorphic] public TestClass testClass;
    [ReadOnly] public int someField;

    [Box("Group")]
    public int one;
    [TitleGroup("Group/Nested Group 1")]
    public int two;
    public int three;
    [TitleGroup("Group/Nested Group 2")]
    public int four;
    public int five;


}

[Serializable]
public class TestClass
{
    public string test;
}

[Serializable]
public class TestClass2 : TestClass
{
    public int testtest;
}