using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card")]
public class Card : ScriptableObject
{
    public CardTypesEnum cardType;
    [TextArea] public string title;
    [TextArea] public string subTitle;
    [TextArea] public string description;
    public int value;
}


public enum CardTypesEnum
{
    Damage, Heal, Steal, Block, Reflect, InstantDeath, Reveal
}