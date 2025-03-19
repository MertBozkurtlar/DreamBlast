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
    public CubeType Type {get; private set;}
    public void SetType(CubeType type)
    {
        Type = type;
        spriteRenderer.sprite = Type.defaultSprite;
    }

    private void Awake()
    {   
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if(Type != null)
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
        // This function can be extended to check for other special items
        // For now, I only check the rocket because it is the only one
        if (groupSize >= 4)
        {
            spriteRenderer.sprite = Type.rocketSprite;
        }
        else
        {
            spriteRenderer.sprite = Type.defaultSprite;
        }
    }

    public override string ToString()
    {
        return gameObject.name;
    }
}
