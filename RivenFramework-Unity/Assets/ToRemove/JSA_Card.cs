using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "New Card", fileName = "New Card")]
public class JSA_Card : ScriptableObject
{
    public string cardName;
    public int cardCost;
    public string[] typeTags;
    [Space]
    [Polymorphic, SerializeReference] 
    public CardEffect[] effects;


    public string cardEffectDescription;

    public void OnValidate()
    {
        cardEffectDescription = string.Join(". Then, ", 
            effects.Select((e) => { return e.GetDescription(); }));
    }
}



[Serializable]
public abstract class CardEffect
{
    public abstract string GetDescription();
}
[Serializable]
public class DrawCards : CardEffect
{
    public CardTarget targetCards = new();
    public override string GetDescription() => $"Draw {targetCards}";

}
[Serializable]
public class DiscardCards : CardEffect
{
    public CardTarget targetCards = new();
    public override string GetDescription() => $"Discard {targetCards}";
}
[Serializable]
public class TapCard : CardEffect
{
    public CardTarget targetCards = new();
    public override string GetDescription() => $"Tap {targetCards}";
}
[Serializable]
public class DealDamage : CardEffect
{
    public int damage;
    public CardTarget targetCards = new();
    public override string GetDescription() => $"Deals {damage} damage to {targetCards}";
}

[Serializable]
public class CardTarget
{
    public TargetType targetType;
    public int targetAmount;
    [Polymorphic, SerializeReference]
    public CardFilter targetFilter = new AnyCard();

    public enum TargetType { Single, All, Amount }

    public override string ToString()
    {
        if (targetType == TargetType.Single)
            return $"target {targetFilter} card";
        else if (targetType == TargetType.All)
            return $"all target {targetFilter} cards";
        else if (targetType == TargetType.Amount)
            return $"{targetAmount} target {targetFilter} cards";
        else
            return "UNKNOWN DESCRIPTION";
    }
}


[Serializable]
public abstract class CardFilter
{
    public abstract string DescribeFilter();
    public abstract bool PassesFilter(JSA_Card card);

    public override string ToString() => DescribeFilter();
}
[Serializable]
public class MultiFilter : CardFilter
{
    public MultiFilterPassType filterType;
    [Polymorphic, SerializeReference]
    public CardFilter[] filters;
    public enum MultiFilterPassType { AND, OR }

    public override string DescribeFilter()
    {
        string delimeter = " ";
        if (filterType == MultiFilterPassType.AND) delimeter = " and ";
        if (filterType == MultiFilterPassType.OR) delimeter = " or ";
        return string.Join(delimeter,
            filters.Select((f) => { return f.DescribeFilter(); }));
    }
    public override bool PassesFilter(JSA_Card card)
    {
        foreach(CardFilter filter in filters)
        {
            if (filterType == MultiFilterPassType.AND)
            {
                if (filter.PassesFilter(card) is false)
                    return false;
            }
            else if (filterType == MultiFilterPassType.OR)
            {
                if (filter.PassesFilter(card))
                    return true;
            }
            else
                throw new NotImplementedException("Unimplemented MultiFilterPassType");
        }
        return true;
    }
}
[Serializable]
public class AnyCard : CardFilter
{
    public override string DescribeFilter() => "any";
    public override bool PassesFilter(JSA_Card card) => true;
}
[Serializable]
public class MatchesTag : CardFilter
{
    public string tagToMatch;
    public override string DescribeFilter() => tagToMatch;
    public override bool PassesFilter(JSA_Card card) => card.typeTags.Contains(tagToMatch);
}
[Serializable]
public class CostsExactly : CardFilter
{
    public int costToMatch;
    public override string DescribeFilter() => $"{costToMatch}-cost";
    public override bool PassesFilter(JSA_Card card) => card.cardCost == costToMatch;
}
[Serializable]
public class IsCard : CardFilter
{
    public JSA_Card cardToMatch;
    public override string DescribeFilter() => $"{cardToMatch.cardName}";
    public override bool PassesFilter(JSA_Card card) => card == cardToMatch;
}


public class TextDisplayer
{
    string textbox;
    Texture characterPortrait;
    public struct TextFrameInfo
    {
        public string text;
        public Texture talkingCharacter;
    }

    public IEnumerator DisplayText(TextFrameInfo[] frames)
    {
        foreach(TextFrameInfo currentFrame in frames)
        {
            //Update character portrait
            UpdatePortrait(currentFrame);

            //Type the text into the field
            yield return TypeTextFrame(currentFrame);

            //Wait for the player to press continue
            yield return WaitForPlayerToContinue();
        }
    }



    public void UpdatePortrait(TextFrameInfo frame)
    {
        characterPortrait = frame.talkingCharacter;
    }
    public IEnumerator TypeTextFrame(TextFrameInfo frame)
    {
        textbox = "";
        foreach (char c in frame.text)
        {
            //print next character, and wait a bit
            textbox += c;
            yield return new WaitForSeconds(0.1f);
        }
    }
    public IEnumerator WaitForPlayerToContinue()
    {
        //Wait if player is NOT pressing continue
        while (PlayerPressesContinue() is false)
            yield return null;
    }
    public bool PlayerPressesContinue()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
}


public class SomeWeirdClass : IEnumerable<int>
{
    public int a;
    public Vector2Int b;
    public int[] c;
    public int d;

    public IEnumerator<int> GetEnumerator()
    {
        List<int> list = new List<int>();

        list.Add(a);
        list.Add(5);
        list.Add(b.x);
        list.Add(b.y);
        list.AddRange(c);
        list.Add(d);

        return list.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class SomeComponent
{
    public void SomeMethod()
    {
        SomeWeirdClass someWeirdObject = new SomeWeirdClass();

        foreach(int i in someWeirdObject)
        {

        }












        JSA_Card[] deck = { };
        CardFilter filter = new MatchesTag() { tagToMatch = "creature" };

        foreach (JSA_Card card in DrawCardsUntil(deck, filter))
        {

        }
    }



    public IEnumerable<JSA_Card> DrawCardsUntil(JSA_Card[] deck, CardFilter filter)
    {
        foreach(JSA_Card card in deck)
        {
            yield return card;
            if (filter.PassesFilter(card))
                break;
        }
    }
}


