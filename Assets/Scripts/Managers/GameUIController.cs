using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DreamBlast.Data;

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

    public void InstantiateGoalUI(Dictionary<string, DreamBlast.Data.ObstacleCount> obstacleCounts)
    {
        // For each obstacle type, instantiate a new goal item if count > 0.
        foreach (var obstacleCount in obstacleCounts)
        {
            if (obstacleCount.Value.Count > 0)
            {
                GameObject goalItem = Instantiate(goalItemPrefab, goalPanel);
                goalItem.name = obstacleCount.Key;
                
                Image iconImage = goalItem.GetComponent<Image>();
                TextMeshProUGUI countText = goalItem.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

                countText.text = obstacleCount.Value.Count.ToString();
                iconImage.sprite = obstacleCount.Value.Icon;
            }
        }
    }

    public void UpdateGoalUI(Dictionary<string, DreamBlast.Data.ObstacleCount> obstacleCounts)
    {
        foreach (var obstacleCount in obstacleCounts)
        {
            // Find child with name of obstacleCount.Key
            GameObject goalItem = goalPanel.Find(obstacleCount.Key)?.gameObject;

            if (goalItem != null)
            {
                TextMeshProUGUI countText = goalItem.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
                if (obstacleCount.Value.Count > 0)
                {
                    countText.text = obstacleCount.Value.Count.ToString();
                }
                else
                {
                    countText.gameObject.SetActive(false);
                    goalItem.transform.Find("Checkmark").gameObject.SetActive(true);
                }
            }
        }
    }

    public void UpdateMoveCountUI(int moveCount)
    {
        moveCountText.text = moveCount.ToString();
    }

    public void TryAgainButtonClicked(){
        GameManager.Instance.RestartLevel();
    }

    public void CloseButtonClicked(){
        LevelManager.Instance.LoadMainMenu();
    }
}
