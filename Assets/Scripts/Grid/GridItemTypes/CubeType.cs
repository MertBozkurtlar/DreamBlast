using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

[CreateAssetMenu(fileName = "CubeType", menuName = "Scriptable Objects/CubeType")]
public class CubeType : GridItemType
{
    [Tooltip("Sprite for rocket hint")] public Sprite rocketSprite;
    [Tooltip("Group size required to match")] public int matchCount = 2;
    [Tooltip("Rocket types")] public RocketType[] rocketTypes;

    public override ItemType itemType { get { return ItemType.Cube; } }

    public override void CheckSpecialEligibility(GridItem item, int groupSize)
    {
        // This function can be extended to check for other special items
        // For now, I only check the rocket because it is the only one
        if (groupSize >= rocketTypes[0].upgradeCount)
        {
            item.SetSprite(rocketSprite);
        }
        else
        {
            item.SetSprite(defaultSprite);
        }
    }
    
    public override void OnMatch(GridItem item, GameGrid grid)
    {
        List<GridItem> group = grid.GetConnectedGroup(item);
        
        if (group.Count >= matchCount)
        {
            // Apply blast damage for all matched items
            foreach(GridItem match in group)
            {
                grid.ApplyBlastDamage(match.GridPosition, 1);
            }
            
            grid.ResetBlastFlags();
            
            // Handle matched items - either upgrade to rocket or remove
            foreach(GridItem match in group)
            {
                if (group.Count >= rocketTypes[0].upgradeCount && match == item)
                {
                    int direction = Random.Range(0, 2);
                    match.SetType(rocketTypes[direction]);
                }
                else {
                    grid.RemoveGameItem(match);
                }
            }
            
            grid.CollapseItems();
            grid.PopulateGrid();
            grid.onMoveMade?.Invoke();
        }
        else if (!DOTween.IsTweening(item.gameObject.transform))
        {
            item.gameObject.transform.DOShakeRotation(0.2f, Vector3.forward * 15, 20, 1, false,
                ShakeRandomnessMode.Harmonic);
        }
    }
}
