using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class GridItem : MonoBehaviour
{    
    // Delegates the matching logic to the GameGrid
    public Action<GridItem> itemClicked;
    private SpriteRenderer spriteRenderer;
    public Vector2Int GridPosition {get; private set;}
    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
    }
    public GridItemType Type {get; private set;}
    public void SetType(GridItemType type)
    {
        Type = type;
        spriteRenderer.sprite = Type.defaultSprite;
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    private void Awake()
    {   
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if(Type != null && spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = Type.defaultSprite;
        }
    }

    void OnMouseDown()
    {
        itemClicked?.Invoke(this);
    }

    public void checkSpecialEligibility(int groupSize)
    {
        Type.checkSpecialEligibility(this, groupSize);
    }

    public override string ToString()
    {
        return gameObject.name;
    }
}
