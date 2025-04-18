// Ignore Spelling: Dialogue

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Snake")]
public class SnakeData : ScriptableObject
{
    public string snakeName;
    public Sprite snakeNormal;
    public Sprite snakeDamage;
    public Sprite snakeDead;
    public Card[] snakeCards;
    public int snakeEndPosition;
    public int maxHP;
    [TextArea] public string[] encounterDialogue;
    [TextArea] public string[] battleDialogue;
    [TextArea] public string[] deathDialogue;
    [TextArea] public string[] winDialogue;
    [TextArea] public string[] reEncounterDialogue;
}
