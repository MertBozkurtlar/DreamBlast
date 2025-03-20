using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public enum RocketDirection {
    horizontal,
    vertical
}

[CreateAssetMenu(fileName = "RocketType", menuName = "Scriptable Objects/RocketType")]
public class RocketType : GridItemType
{
    public override ItemType itemType { get { return ItemType.PowerUp; } }
    public int upgradeCount;
    public RocketDirection direction;
    public Sprite leftDownPart;
    public Sprite rightUpPart;
    public Sprite smokeParticle;
    public Sprite startParticle;

    public override void OnMatch(GridItem item, GameGrid grid)
    {
        // Create rocket parts
        GameObject leftRocket = new GameObject("LeftRocket");
        GameObject rightRocket = new GameObject("RightRocket");
        SpriteRenderer leftRenderer = leftRocket.AddComponent<SpriteRenderer>();
        SpriteRenderer rightRenderer = rightRocket.AddComponent<SpriteRenderer>();
        leftRenderer.sprite = leftDownPart;
        rightRenderer.sprite = rightUpPart;
        
        Vector3 startPos = item.transform.position;
        leftRocket.transform.position = startPos;
        rightRocket.transform.position = startPos;

        // Remove original rocket
        grid.RemoveGameItem(item);

        Sequence sequence = DOTween.Sequence();

        if (direction == RocketDirection.horizontal)
        {
            // Calculate extended positions beyond grid bounds
            float gridLeftEdge = grid.GridToWorldPosition(0, item.GridPosition.y).x;
            float gridRightEdge = grid.GridToWorldPosition(grid.GridDimensions.x - 1, item.GridPosition.y).x;
            float extraDistance = 5f; // Units to travel beyond grid
            float leftTarget = gridLeftEdge - extraDistance;
            float rightTarget = gridRightEdge + extraDistance;
            
            sequence.Join(leftRocket.transform.DOMoveX(leftTarget, 0.5f).SetEase(Ease.Linear));
            sequence.Join(rightRocket.transform.DOMoveX(rightTarget, 0.5f).SetEase(Ease.Linear));

            // Break cubes sequentially from center to edges
            int centerX = item.GridPosition.x;
            for (int offset = 1; offset < grid.GridDimensions.x; offset++)
            {
                int leftX = centerX - offset;
                int rightX = centerX + offset;
                float delay = offset * 0.05f;

                if (leftX >= 0)
                {
                    GridItem leftItem = grid.GetItemAt(leftX, item.GridPosition.y);
                    if (leftItem != null)
                    {
                        sequence.InsertCallback(delay, () => grid.RemoveGameItem(leftItem));
                    }
                }

                if (rightX < grid.GridDimensions.x)
                {
                    GridItem rightItem = grid.GetItemAt(rightX, item.GridPosition.y);
                    if (rightItem != null)
                    {
                        sequence.InsertCallback(delay, () => grid.RemoveGameItem(rightItem));
                    }
                }
            }
        }
        else // vertical
        {
            // Calculate extended positions beyond grid bounds
            float gridBottomEdge = grid.GridToWorldPosition(item.GridPosition.x, 0).y;
            float gridTopEdge = grid.GridToWorldPosition(item.GridPosition.x, grid.GridDimensions.y - 1).y;
            float extraDistance = 5f; // Units to travel beyond grid
            float bottomTarget = gridBottomEdge - extraDistance;
            float topTarget = gridTopEdge + extraDistance;
            
            sequence.Join(leftRocket.transform.DOMoveY(bottomTarget, 0.5f).SetEase(Ease.Linear));
            sequence.Join(rightRocket.transform.DOMoveY(topTarget, 0.5f).SetEase(Ease.Linear));

            // Break cubes sequentially from center to edges
            int centerY = item.GridPosition.y;
            for (int offset = 1; offset < grid.GridDimensions.y; offset++)
            {
                int bottomY = centerY - offset;
                int topY = centerY + offset;
                float delay = offset * 0.05f;

                if (bottomY >= 0)
                {
                    GridItem bottomItem = grid.GetItemAt(item.GridPosition.x, bottomY);
                    if (bottomItem != null)
                    {
                        sequence.InsertCallback(delay, () => grid.RemoveGameItem(bottomItem));
                    }
                }

                if (topY < grid.GridDimensions.y)
                {
                    GridItem topItem = grid.GetItemAt(item.GridPosition.x, topY);
                    if (topItem != null)
                    {
                        sequence.InsertCallback(delay, () => grid.RemoveGameItem(topItem));
                    }
                }
            }
        }

        // Cleanup and finish
        sequence.OnComplete(() => {
            Object.Destroy(leftRocket);
            Object.Destroy(rightRocket);
            grid.CollapseItems();
            grid.PopulateGrid();
        });
    }
}
