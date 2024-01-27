using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // so we can interact from anywhere in the game
    public static GameManager Instance;

    public GameState State;

    public static event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        //TODO: read more into the Singleton pattern :3
        //basically want there to only be only one instance
        Instance = this;
    }

    private void Start()
    {
        //TODO Change to MenuScreen once menu is made 
        // for now just on move state
        UpdateGameState(GameState.Move);
    }

    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.MenuScreen:
                HandleMenuScreen();
                break;
            case GameState.Move:
                HandleMove();
                break;
            case GameState.Aim:
                HandleAim();
                break;
            case GameState.Fire:
                HandleFire();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }

    private void HandleMenuScreen()
    {

    }

    private void HandleMove()
    {

    }

    private void HandleAim()
    {

    }

    private void HandleFire()
    {

    }



}

    public enum GameState
{
    MenuScreen,
    Move,
    Aim,
    Fire
}
