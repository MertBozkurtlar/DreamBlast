using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

[CreateAssetMenu(fileName = "RocketType", menuName = "Scriptable Objects/RocketType")]
public class RocketType : GridItemType
{
    public override ItemType itemType { get { return ItemType.PowerUp; } }
    [Tooltip("Match size required for upgrade")] public int upgradeCount;
    [Tooltip("Is rocket horizontal or vertical")] public bool isHorizontal;
    [Tooltip("Left or Bottom rocket prefab")] public GameObject leftBottomRocket;
    [Tooltip("Right or Top rocket prefab")] public GameObject rightTopRocket;
    [Tooltip("Duration of the rocket in seconds")] public float rocketDuration = 1f;
    [Tooltip("Extra distance to travel beyond the grid edges")] public float extraDistance = 10f;

    public override void OnMatch(GridItem item, GameGrid grid)
    {
        grid.onMoveMade?.Invoke();
        ActivateRocket(item, grid);
    }
    
    public override void OnDamaged(GridItem item, GameGrid grid, int damage)
    {
        // When damaged by another rocket, this rocket should also explode
        ActivateRocket(item, grid);
    }
    
    private void ActivateRocket(GridItem item, GameGrid grid)
    {
        GameObject leftRocket, rightRocket;
        CreateRocketParts(item.transform.position, out leftRocket, out rightRocket);

        // Get particle systems from prefabs to handle cleanup later
        List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        particleSystems.AddRange(leftRocket.GetComponentsInChildren<ParticleSystem>());
        particleSystems.AddRange(rightRocket.GetComponentsInChildren<ParticleSystem>());
        
        // Play all particle effects
        foreach (var ps in particleSystems)
        {
            ps.Play();
        }

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
            // Detach particles from rocket parts before destroying rockets
            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps != null)
                {
                    // Stop emission but let existing particles live
                    var emission = ps.emission;
                    emission.enabled = false;
                    
                    // Unparent from rocket
                    ps.transform.SetParent(null);
                    
                    // Set up auto-destruction when particles finish
                    Object.Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
                }
            }
            
            // Destroy rocket parts
            Object.Destroy(leftRocket);
            Object.Destroy(rightRocket);
            
            // Continue with grid operations
            grid.CollapseItems();
            grid.PopulateGrid();
        });
    }

    private void CreateRocketParts(Vector3 startPos, out GameObject leftRocket, out GameObject rightRocket)
    {
        leftRocket = Object.Instantiate(leftBottomRocket, startPos, Quaternion.identity);
        rightRocket = Object.Instantiate(rightTopRocket, startPos, Quaternion.identity);
    }

    private void SetupRocketMovement(GameObject leftRocket, GameObject rightRocket, Vector2 startEdge, Vector2 endEdge, bool isHorizontal, Sequence sequence)
    {
        float startTarget = startEdge.x - extraDistance;
        float endTarget = endEdge.x + extraDistance;

        if (!isHorizontal)
        {
            startTarget = startEdge.y - extraDistance;
            endTarget = endEdge.y + extraDistance;
        }

        if (isHorizontal)
        {
            sequence.Join(leftRocket.transform.DOMoveX(startTarget, rocketDuration).SetEase(Ease.Linear));
            sequence.Join(rightRocket.transform.DOMoveX(endTarget, rocketDuration).SetEase(Ease.Linear));
        }
        else
        {
            sequence.Join(leftRocket.transform.DOMoveY(startTarget, rocketDuration).SetEase(Ease.Linear));
            sequence.Join(rightRocket.transform.DOMoveY(endTarget, rocketDuration).SetEase(Ease.Linear));
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
        
        // Calculate delay as a fraction of the total rocket duration
        float delayPerCell = rocketDuration / (length + extraDistance);

        for (int offset = 1; offset < length; offset++)
        {
            int negativePos = center - offset;
            int positivePos = center + offset;

            float delay = offset * delayPerCell;

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
        if (item != null)
        {
            // Obstacles are handled in ApplyRocketDamage in GameGrid
            if (item.Type.itemType == ItemType.PowerUp)
            {
                // For rockets and other power-ups, add them to the sequence with proper delay
                sequence.InsertCallback(delay, () => {
                    item.Type.OnDamaged(item, grid, 1);
                });
            }
            else if (item.Type.itemType == ItemType.Cube)
            {
                // Regular items just get removed with delay
                sequence.InsertCallback(delay, () => grid.RemoveGameItem(item));
            }
        }
    }
}
