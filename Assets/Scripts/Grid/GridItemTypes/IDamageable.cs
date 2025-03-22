// Interface for grid items that can take damage
public interface IDamageable 
{
    // Gets the maximum health of the item
    int MaxHealth { get; }
    
    // Gets the current health of the item
    int CurrentHealth { get; }
    
    // Applies damage to the item
    bool TakeDamage(int amount, bool isRocket, GridItem item);
    
    // Gets whether the item can fall
    bool CanFall { get; }
} 