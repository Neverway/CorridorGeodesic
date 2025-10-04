using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RivenFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class AnimationTesterScript : MonoBehaviour
{
    public Animation animationControl;
    public AnimationClip select;
    public AnimationClip unselect;

    public bool selected = false;
    private bool lastSelected = false;
    public void Update()
    {
        if (lastSelected != selected)
        {
            lastSelected = selected;
            if (selected)
                animationControl.Play(select.name);
            else
                animationControl.Play(unselect.name);
        }
    }

    [Serializable]
    public class SomeTestSerializedClass
    {
        public int someNum;
        public int otherNum;
        public string someString;
        public SomeEnum someEnumValue;
        public List<string> moreStrings;
        public Vector3 someVector3;
        public SubClassTest subClassValue;
        public enum SomeEnum
        {
            ValueA, ValueB, ValueC
        }
    }

    [Serializable]
    public class SubClassTest
    {
        public string hehehe;
        public int iBetYouDidntKnowTHIS;
    }



    [Serializable]
    public class SomeJsonUtilityTest : ScriptableObject
    {
        [SerializeReference] public Base something;
    }
    [Serializable]
    public abstract class Base
    {
        public string baseName;
    }
    [Serializable]
    public class IntBase : Base
    {
        public int baseInt;
    }
    [Serializable]
    public class StringBase : Base
    {
        public string baseString;
    }




    [ContextMenu("Do The Thing")]
    public void DoThing()
    {
        var thing = ScriptableObject.CreateInstance<SomeJsonUtilityTest>();

        thing.something = new IntBase()
        {
            baseInt = 5
        };

        string json = JsonUtility.ToJson(thing);
        Debug.Log(json);

        var deJsoned = ScriptableObject.CreateInstance<SomeJsonUtilityTest>();
        JsonUtility.FromJsonOverwrite(json, deJsoned);

        Debug.Log(deJsoned.something.GetType());
        Debug.Log(((IntBase)deJsoned.something).baseInt);

        return;
        var test = new SomeTestSerializedClass();
        test.someNum = 42;
        test.someString = "hello";
        test.someEnumValue = SomeTestSerializedClass.SomeEnum.ValueB;
        test.moreStrings = new List<string>() { "eqefkqiuhwfiluhalhfrlaiuhrfierer", 
            "eqefkqiuhwfiluhalhfrlaiuhrfierer", "sdfaw;eoifhqwiurhiwehuriugfwert", "29873498017948hjf98h14lin", "asdfafwt" };
        test.someVector3 = Vector3.one;
        test.subClassValue = new SubClassTest()
        {
            hehehe = "hello",
            iBetYouDidntKnowTHIS = 42
        };








        string serialized = JsonConvert.SerializeObject(test, Formatting.Indented,
                        new JsonSerializerSettings()
                        {
                            ContractResolver = new NoGetOnlyResolver(),
                            TypeNameHandling = TypeNameHandling.Auto
                        });

        var deserialized = JsonConvert.DeserializeObject<SomeTestSerializedClass>(serialized);

        Debug.Log(serialized);

        return;


        Benchmark.TestOneSecond("JSON convert test", () =>
        {
            string serialized = JsonConvert.SerializeObject(test, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            var deserialized = JsonConvert.DeserializeObject<SomeTestSerializedClass>(serialized);
        });


    }


    public class NoGetOnlyResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            // Exclude properties that have no setter
            if (!prop.Writable)
            {
                prop.ShouldSerialize = _ => false;
            }

            return prop;
        }
    }
}
