using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainUIManager : MonoBehaviour
{
    [Header("--- Panels ---")]
    [SerializeField]
    private GameObject GamemodeSeltionMenu;
    [Header("--- Values ---")]
    [SerializeField]
    private int[] Singleplayer_OpenScenes;
    [SerializeField]
    private int[] Multiplayer_OpenScenes;
    private bool GamemodeSelectPanelActive;
    private Animator Anim_BlackOutPanel;
    private void Awake()
    {
        Anim_BlackOutPanel = gameObject.transform.GetChild(gameObject.transform.childCount - 1).GetComponent<Animator>();
    }
    public void ToggleGamePanel()
    {
        GamemodeSelectPanelActive = !GamemodeSelectPanelActive;
        GamemodeSeltionMenu.SetActive(GamemodeSelectPanelActive);
    }
    public void StartMultiplayer()
    {
        StartCoroutine(LoadScenes(Multiplayer_OpenScenes, 1f));        
    }
    IEnumerator LoadScenes(int[] scenes, float delay)
    {
        Anim_BlackOutPanel.SetBool("BlackOut", true);
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.Scene MainMenuScene = SceneManager.GetActiveScene();
        int i = 0;
        AsyncOperation Scene1 = new AsyncOperation();
        foreach (int scene in scenes)
        {         
            if(i == 0)
            {
                Scene1 = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);            
            }
            i++;
        }
        while (!Scene1.isDone)
        {
            yield return null;
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(scenes[0]));
        SceneManager.UnloadSceneAsync(MainMenuScene);
    }
}
