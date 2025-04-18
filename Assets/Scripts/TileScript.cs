using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TileScript : MonoBehaviour
{
    public TileTypes tileType;
    [SerializeField] public SnakeData snake;

    [SerializeField] Sprite neutralTile;
    [SerializeField] Sprite bananaTile;
    [SerializeField] Sprite damageTile;
    [SerializeField] Sprite giftTile;
    [SerializeField] Sprite healTile;
    [SerializeField] Sprite goalTile;

    void Start()
    {
        UpdateTile();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Application.isPlaying) return;
        UpdateTile();
    }
#endif

    void UpdateTile()
    {
        GetComponentInChildren<TextMeshPro>().text = tileType switch
        {
            TileTypes.Neutral => $"{transform.GetSiblingIndex() + 1}",
            _ => string.Empty,
        };
        GetComponent<SpriteRenderer>().sprite = tileType switch
        {
            TileTypes.Damage => damageTile,
            TileTypes.Reward => giftTile,
            TileTypes.Heal => healTile,
            TileTypes.Banana => bananaTile,
            TileTypes.Goal => goalTile,
            _ => neutralTile
        };
    }
}

public enum TileTypes
{
    Neutral, Snake, Damage, Reward, Heal, Banana, Goal
}