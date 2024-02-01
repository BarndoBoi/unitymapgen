using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuPanel;



    void Start()
    {
        // subscribe to game state changes event
        GameManager.Instance.OnGameStateChanged += GameManagerOnGameStateChanged;
    }

    private void OnDestroy()
    {
        //unsubscribe
        GameManager.Instance.OnGameStateChanged -= GameManagerOnGameStateChanged;
    }

    private void GameManagerOnGameStateChanged(GameState state)
    {
        MainMenuPanel.SetActive(state == GameState.MenuScreen);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlay()
    {
        SceneManager.LoadScene("LexiScene");

    }
}
