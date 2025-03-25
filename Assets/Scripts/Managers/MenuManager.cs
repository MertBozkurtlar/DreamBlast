using TMPro;
using UnityEngine;

public class MenuManager : Singleton<MenuManager>
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private PlayerProgressSO playerProgress;
    private LevelManager levelManager;
    private bool gameCompleted = false;

    void Start()
    {
        levelManager = LevelManager.Instance;
    }
    void Update(){
        if (playerProgress.CurrentLevel >= LevelManager.Instance.LevelJsonFiles.Length){
            currentLevelText.text = "Completed";
            gameCompleted = true;
        }
        else{
            currentLevelText.text = "Level " + (playerProgress.CurrentLevel + 1);
            gameCompleted = false;
        }
    }

    public void OnLevelSelectionButtonClick(){
        if (gameCompleted){
            Debug.Log("Game Completed");
            return;
        }
        Debug.Log("Level Selection Button Clicked");
        Debug.Log(levelManager);
        levelManager.StartCurrentLevel();
    }
}
