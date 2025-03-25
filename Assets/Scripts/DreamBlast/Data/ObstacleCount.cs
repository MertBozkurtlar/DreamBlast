using UnityEngine;

namespace DreamBlast.Data
{
    public class ObstacleCount
    {
        public string Name { get; private set; }
        public int Count { get; private set; }

        public Sprite Icon { get; private set; }
        
        public ObstacleCount(string name, Sprite icon)
        {
            Name = name;
            Count = 0;
            Icon = icon;
        }
        
        public void Increase()
        {
            Count++;
        }
        
        public void Decrease()
        {
            Count = Mathf.Max(0, Count - 1);
        }
        
        public void Reset()
        {
            Count = 0;
        }
    }
} 