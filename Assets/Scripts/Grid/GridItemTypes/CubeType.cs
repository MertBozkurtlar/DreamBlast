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
    
    [Header("Animation Settings")]
    [Tooltip("Distance for cubes to bounce away")] public float bounceDistance = 0.5f;
    [Tooltip("Duration of the bounce animation")] public float bounceDuration = 0.3f;
    [Tooltip("Duration of return to center animation")] public float returnDuration = 0.4f;
    [Tooltip("Duration of merge animation")] public float mergeDuration = 0.2f;

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
            if (group.Count >= rocketTypes[0].upgradeCount)
            {
                // Create sequence for the merge animation
                Sequence mergeSequence = DOTween.Sequence();
                
                // Animate all cubes in the group
                foreach (GridItem match in group)
                {
                    if (match != item)
                    {
                        AnimateCubeMerge(match, item.transform.position, mergeSequence);
                    }
                }
                
                // After all animations complete, upgrade to rocket
                mergeSequence.OnComplete(() => {
                    foreach (GridItem match in group)
                    {
                        if (match == item)
                        {
                            int direction = Random.Range(0, 2);
                            match.SetType(rocketTypes[direction]);
                        }
                        else
                        {
                            grid.RemoveGameItem(match);
                        }
                    }
                    
                    grid.CollapseItems();
                    grid.PopulateGrid();
                    grid.onMoveMade?.Invoke();
                });
            }
            else
            {
                foreach(GridItem match in group)
                {
                    grid.RemoveGameItem(match);
                }
                
                grid.CollapseItems();
                grid.PopulateGrid();
                grid.onMoveMade?.Invoke();
            }
        }
        else if (!DOTween.IsTweening(item.gameObject.transform))
        {
            item.gameObject.transform.DOShakeRotation(0.2f, Vector3.forward * 15, 20, 1, false,
                ShakeRandomnessMode.Harmonic);
        }
    }
    
    private void AnimateCubeMerge(GridItem cube, Vector3 targetPosition, Sequence sequence)
    {
        Vector3 originalPos = cube.transform.position;
        Vector3 bounceDirection = (originalPos - targetPosition).normalized;
        Vector3 bounceTarget = originalPos + bounceDirection * bounceDistance;
        
        // Kill any existing tweens on this cube
        cube.transform.DOKill();
        
        // Store the original sorting order and set to high value
        SpriteRenderer spriteRenderer = cube.GetComponent<SpriteRenderer>();
        int originalSortingOrder = spriteRenderer.sortingOrder;
        spriteRenderer.sortingOrder = 99;
        
        // Create a sequence for this specific cube
        Sequence cubeSequence = DOTween.Sequence();
        
        // First bounce out
        cubeSequence.Append(cube.transform
            .DOMove(bounceTarget, bounceDuration)
            .SetEase(Ease.OutQuad));
            
        // Then return to center
        cubeSequence.Append(cube.transform
            .DOMove(targetPosition, returnDuration)
            .SetEase(Ease.InQuad));
            
        // Reset sorting order when animation completes
        cubeSequence.OnComplete(() => {
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = originalSortingOrder;
            }
        });
            
        // Join the cube's sequence to the main sequence
        sequence.Join(cubeSequence);
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
