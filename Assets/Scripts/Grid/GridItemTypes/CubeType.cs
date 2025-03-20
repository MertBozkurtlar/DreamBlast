using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

[CreateAssetMenu(fileName = "CubeType", menuName = "Scriptable Objects/CubeType")]
public class CubeType : GridItemType
{
    public Sprite rocketSprite;
    public int matchCount = 2;
    public RocketType[] rocketTypes;

    public override ItemType itemType { get { return ItemType.Cube; } }

    public override void checkSpecialEligibility(GridItem item, int groupSize)
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
        
        // If more than the rocket upgrade count items are matched, turn one into a rocket
        if (group.Count >= rocketTypes[0].upgradeCount)
        {
            foreach(GridItem match in group)
            {
                if (match == item)
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
        }
        else if (group.Count >= matchCount)
        {
            foreach(GridItem match in group)
            {
                grid.RemoveGameItem(match);
            }
            grid.CollapseItems();
            grid.PopulateGrid();
        }
        else {
            if (!DOTween.IsTweening(item.gameObject.transform))
                item.gameObject.transform.DOShakeRotation(0.2f, Vector3.forward * 15, 20, 1, false, 
                    ShakeRandomnessMode.Harmonic);
        }
    }
}
