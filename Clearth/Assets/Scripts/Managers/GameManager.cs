using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // 임시 고치기

public class GameManager
{
    [HideInInspector] public GameState gameState; // 현재 게임 상태
    [HideInInspector] public GameState previousGameState; // 이전 게임 상태

    // 초기 던전 레벨 값 (스테이지 번호)
    private int currentDungeonLevelListIndex = 0;

    private bool _CookAbleTime = false;

    private bool _isMoving = true;

    //플레이어의 무적 상태를 관리
    private bool _isInvincible = false;

    public bool IsInvincible { get { return _isInvincible; } set { _isInvincible = value; } }

    public bool IsMoving { get { return _isMoving; } set { _isMoving = value; } }

    public bool CookAbleTime { get { return _CookAbleTime; } set { _CookAbleTime = value; } }

    public void Init()
    {
        previousGameState = GameState.title;
        gameState = GameState.title;
    }

    public void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.title:
                break;
            case GameState.gameStarted:
                Scene scene = SceneManager.GetActiveScene();
                if (scene.name == "GameScene")
                {
                    GameStart();
                }
                break;
            case GameState.restartGame:

            default:
                break;
        }
    }


    void GameStart()
    {

        // 캐릭터 생성
        GameObject Player = Managers.Resource.Instantiate("Player/Player");


        gameState = GameState.playingLevel; // 게임 상태를 진행 중으로 변경
    }

    /*
    public Player GetPlayer()
    {
        return player;
    }
    */

    private IEnumerator LevelCompleted()
    {
        // 스테이트를 다시 플레이로 바꿈
        gameState = GameState.playingLevel;

        // 2초 기다림
        yield return new WaitForSeconds(2f);


        // 레벨 클리어 출력?

        // 스크린을 페이드 아웃
        //yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        // 현제 던전 레벨을 증가시킴
        currentDungeonLevelListIndex++;

    }
}