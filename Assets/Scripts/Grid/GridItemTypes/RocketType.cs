using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

[CreateAssetMenu(fileName = "RocketType", menuName = "Scriptable Objects/RocketType")]
public class RocketType : GridItemType
{
    public override ItemType itemType { get { return ItemType.PowerUp; } }
    [Tooltip("Match size required for upgrade")] public int upgradeCount;
    [Tooltip("Is rocket horizontal or vertical")] public bool isHorizontal;
    [Tooltip("Left or Top rocket sprite")] public Sprite leftBottomPart;
    [Tooltip("Right or Bottom rocket sprite")] public Sprite rightTopPart;

    public override void OnMatch(GridItem item, GameGrid grid)
    {
        GameObject leftRocket, rightRocket;
        CreateRocketParts(item.transform.position, out leftRocket, out rightRocket);

        List<Vector2Int> rocketPath = new List<Vector2Int>();
        grid.RemoveGameItem(item);
        Sequence sequence = DOTween.Sequence();

        Vector2 startEdge, endEdge;
        if (isHorizontal)
        {
            startEdge = grid.GridToWorldPosition(0, item.GridPosition.y);
            endEdge = grid.GridToWorldPosition(grid.GridDimensions.x - 1, item.GridPosition.y);
        }
        else
        {
            startEdge = grid.GridToWorldPosition(item.GridPosition.x, 0);
            endEdge = grid.GridToWorldPosition(item.GridPosition.x, grid.GridDimensions.y - 1);
        }

        SetupRocketMovement(leftRocket, rightRocket, startEdge, endEdge, isHorizontal, sequence);
        rocketPath = GetRocketPath(item.GridPosition, grid.GridDimensions, isHorizontal);
        AddSequentialDestructions(grid, item.GridPosition, grid.GridDimensions, isHorizontal, sequence);
        
        grid.ApplyRocketDamage(rocketPath, 1);

        sequence.OnComplete(() => {
            Object.Destroy(leftRocket);
            Object.Destroy(rightRocket);
            grid.CollapseItems();
            grid.PopulateGrid();
        });
    }

    private void CreateRocketParts(Vector3 startPos, out GameObject leftRocket, out GameObject rightRocket)
    {
        leftRocket = new GameObject("LeftRocket");
        rightRocket = new GameObject("RightRocket");
        
        SpriteRenderer leftRenderer = leftRocket.AddComponent<SpriteRenderer>();
        SpriteRenderer rightRenderer = rightRocket.AddComponent<SpriteRenderer>();
        
        leftRenderer.sprite = leftBottomPart;
        rightRenderer.sprite = rightTopPart;
        
        leftRocket.transform.position = startPos;
        rightRocket.transform.position = startPos;
    }

    private void SetupRocketMovement(GameObject leftRocket, GameObject rightRocket, Vector2 startEdge, Vector2 endEdge, bool isHorizontal, Sequence sequence)
    {
        float extraDistance = 5f;
        float startTarget = startEdge.x - extraDistance;
        float endTarget = endEdge.x + extraDistance;

        if (!isHorizontal)
        {
            startTarget = startEdge.y - extraDistance;
            endTarget = endEdge.y + extraDistance;
        }

        if (isHorizontal)
        {
            sequence.Join(leftRocket.transform.DOMoveX(startTarget, 0.5f).SetEase(Ease.Linear));
            sequence.Join(rightRocket.transform.DOMoveX(endTarget, 0.5f).SetEase(Ease.Linear));
        }
        else
        {
            sequence.Join(leftRocket.transform.DOMoveY(startTarget, 0.5f).SetEase(Ease.Linear));
            sequence.Join(rightRocket.transform.DOMoveY(endTarget, 0.5f).SetEase(Ease.Linear));
        }
    }

    private List<Vector2Int> GetRocketPath(Vector2Int position, Vector2Int gridDimensions, bool isHorizontal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        int center = isHorizontal ? position.x : position.y;
        int length = isHorizontal ? gridDimensions.x : gridDimensions.y;

        for (int i = 0; i < length; i++)
        {
            if (i != center)
            {
                path.Add(isHorizontal ? new Vector2Int(i, position.y) : new Vector2Int(position.x, i));
            }
        }

        return path;
    }

    private void AddSequentialDestructions(GameGrid grid, Vector2Int position, Vector2Int gridDimensions, bool isHorizontal, Sequence sequence)
    {
        int center = isHorizontal ? position.x : position.y;
        int length = isHorizontal ? gridDimensions.x : gridDimensions.y;

        for (int offset = 1; offset < length; offset++)
        {
            int negativePos = center - offset;
            int positivePos = center + offset;
            float delay = offset * 0.05f;

            if (negativePos >= 0)
            {
                Vector2Int checkPos = isHorizontal ? new Vector2Int(negativePos, position.y) : new Vector2Int(position.x, negativePos);
                AddDestructionCallback(grid, checkPos, delay, sequence);
            }

            if (positivePos < length)
            {
                Vector2Int checkPos = isHorizontal ? new Vector2Int(positivePos, position.y) : new Vector2Int(position.x, positivePos);
                AddDestructionCallback(grid, checkPos, delay, sequence);
            }
        }
    }

    private void AddDestructionCallback(GameGrid grid, Vector2Int position, float delay, Sequence sequence)
    {
        GridItem item = grid.GetItemAt(position.x, position.y);
        if (item != null && !item.IsObstacle())
        {
            sequence.InsertCallback(delay, () => grid.RemoveGameItem(item));
        }
    }
}
