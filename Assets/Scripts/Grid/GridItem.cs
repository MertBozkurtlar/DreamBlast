using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GridItem : MonoBehaviour
{    
    private CubeType type;
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

    private SpriteRenderer spriteRenderer;

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
    public override string ToString()
    {
        return gameObject.name;
    }
}
