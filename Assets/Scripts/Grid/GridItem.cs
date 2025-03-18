using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class GridItem : MonoBehaviour
{    
    // Delegates the matching logic to the GameGrid
    public Action<GridItem> itemClicked;
    private CubeType type;
    private SpriteRenderer spriteRenderer;
    private Vector2Int gridPosition;
    public Vector2Int GridPosition
    {
        get
        {
            return gridPosition;
        }
    }
    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }
    public CubeType Type
    {
        get
        {
            return type;
        }
    }
    public void SetType(CubeType type)
    {
        this.type = type;
        spriteRenderer.sprite = type.defaultSprite;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if(type != null)
        {
            spriteRenderer.sprite = type.defaultSprite;
        }
    }


    void OnMouseDown()
    {
        itemClicked?.Invoke(this);
    }
    


    public override string ToString()
    {
        return gameObject.name;
    }
}
