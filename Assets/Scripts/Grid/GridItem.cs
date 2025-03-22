using System;
using UnityEngine;
using UnityEngine.Events;

// Represents a single item in the game grid
[RequireComponent(typeof(SpriteRenderer))]
public class GridItem : MonoBehaviour
{    
    // Event triggered when this item is clicked
    public Action<GridItem> itemClicked;

    private SpriteRenderer spriteRenderer;

    // The position of this item on the grid
    public Vector2Int GridPosition { get; private set; }
    
    public GridItemType Type { get; private set; }

    private void Awake()
    {   
        TryGetComponent(out spriteRenderer);
    }

    private void Start()
    {
        if(Type != null && spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = Type.defaultSprite;
        }
    }

    private void OnMouseDown()
    {
        itemClicked?.Invoke(this);
    }

    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
    }

    public void SetType(GridItemType type)
    {
        Type = type;
        
        if (spriteRenderer != null && Type != null)
        {
            spriteRenderer.sprite = Type.defaultSprite;
        
            // Initialize obstacle if needed
            if (Type is ObstacleType obstacleType)
            {
                obstacleType.Initialize(this);
            }
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
    public void CheckSpecialEligibility(int groupSize)
    {
        if (Type != null)
        {
            Type.CheckSpecialEligibility(this, groupSize);
        }
    }

    public bool IsObstacle()
    {
        return Type != null && Type.itemType == ItemType.Obstacle;
    }
    
    public bool CanFall()
    {
        if (IsObstacle())
        {
            return (Type as ObstacleType).CanFall;
        }
        return true; // Non-obstacles can fall by default
    }
    
    public bool TakeDamage(int amount, bool isRocket)
    {
        if (IsObstacle())
        {
            ObstacleType obstacleType = Type as ObstacleType;
            bool destroyed = obstacleType.TakeDamage(amount, isRocket, this);
            
            if (!destroyed)
            {
                obstacleType.UpdateVisuals(this);
            }
            
            return destroyed;
        }
        return false;
    }
}

