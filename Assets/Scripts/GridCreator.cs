using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridCreator : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] Vector2 tileSize;
    [SerializeField] Vector2Int dimensions;

#if UNITY_EDITOR
    [ContextMenu("Generate Grid")]
    void GenerateGrid()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        int x = 0;
        int y = 0;

        while (y < dimensions.y)
        {
            while (x < dimensions.x)
            {
                SpawnTile(x, y);
                x++;
            }
            y++;
            x--;
            while (x >= 0)
            {
                SpawnTile(x, y);
                x--;
            }

            x++;
            y++;
        }
    }

    [ContextMenu("Set random types")]
    void SetRandomTypes()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            TileScript t = transform.GetChild(i).GetComponent<TileScript>();
            if (t == null) continue;

            var types = Enum.GetValues(typeof(TileTypes));
            do
            {
                t.tileType = (TileTypes)UnityEngine.Random.Range(0, types.Length);
            } while (t.tileType == TileTypes.Snake);
            EditorUtility.SetDirty(t);
        }
    }

    void SpawnTile(int x, int y)
    {
        GameObject g = PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
        g.transform.localPosition = new Vector3(tileSize.x * x, tileSize.y * y, 0);
    }
#endif
}
