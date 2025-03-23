using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

[CreateAssetMenu(fileName = "CubeType", menuName = "Scriptable Objects/CubeType")]
public class CubeType : GridItemType
{
    [Tooltip("Sprite for rocket hint")] public Sprite rocketSprite;
    [Tooltip("Group size required to match")] public int matchCount = 2;
    [Tooltip("Rocket types")] public RocketType[] rocketTypes;
    [Tooltip("Particles to play when the cube is destroyed")] public ParticleSystem destroyParticles;
    [Tooltip("Material for destroy particles effect")] public Material destroyParticleMaterial;

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
                    // Remove particle playing from here - it will be handled in OnDestroy
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
    
    public override void OnItemDestroyed(GridItem item, GameGrid grid)
    {
        // Play particles when cube is destroyed (by any means)
        PlayDestroyParticles(item.transform.position);
    }
    
    private void PlayDestroyParticles(Vector3 position)
    {
        if (destroyParticles != null)
        {
            // Create position with z = -1
            Vector3 particlePosition = new Vector3(position.x, position.y, -1f);
            ParticleSystem particles = Object.Instantiate(destroyParticles, particlePosition, Quaternion.identity);
            particles.GetComponent<ParticleSystemRenderer>().material = destroyParticleMaterial;
            particles.Play();
            
            // Auto-destroy particle system after it finishes
            Object.Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        }
    }
}
