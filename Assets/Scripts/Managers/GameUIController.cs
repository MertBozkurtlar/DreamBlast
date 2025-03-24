using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameUIController : MonoBehaviour
{
    [Header("Goal UI")]
    [SerializeField] private Transform goalPanel;
    [SerializeField] private GameObject goalItemPrefab;
    [SerializeField] private Sprite boxIconSprite;
    [SerializeField] private Sprite stoneIconSprite;
    [SerializeField] private Sprite vaseIconSprite;

    [Header("Move Count UI")]
    [SerializeField] private TextMeshProUGUI moveCountText;

    // This method can be called by the LevelManager or by a GameManager event when the level is loaded or an obstacle changes.
    public void UpdateGoalUI(Dictionary<string, LevelManager.ObstacleCount> obstacleCounts)
    {
        // Clear previous items
        foreach (Transform child in goalPanel)
        {
            Destroy(child.gameObject);
        }

        
        // For each obstacle type, instantiate a new goal item if count > 0.
        foreach (var obstacleCount in obstacleCounts)
        {
            if (obstacleCount.Value.Count > 0)
            {
                GameObject goalItem = Instantiate(goalItemPrefab, goalPanel);
                
                Image iconImage = goalItem.GetComponent<Image>();
                TextMeshProUGUI countText = goalItem.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

                countText.text = obstacleCount.Value.Count.ToString();

                // Set sprite based on obstacle type key
                switch (obstacleCount.Key)
                {
                    case "bo":
                        iconImage.sprite = boxIconSprite;
                        break;
                    case "s":
                        iconImage.sprite = stoneIconSprite;
                        break;
                    case "v":
                        iconImage.sprite = vaseIconSprite;
                        break;
                    default:
                        Debug.LogWarning("Unrecognized obstacle type: " + obstacleCount.Key);
                        break;
                }
            }
        }
    }

    public void UpdateMoveCountUI(int moveCount)
    {
        moveCountText.text = moveCount.ToString();
    }
}
