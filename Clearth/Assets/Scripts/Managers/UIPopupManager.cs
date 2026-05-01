using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//각 팝업의 함수를 관리하는 코드
public class UIPopupManager
{
    //게임 정지 유아이
    private GamePausePopup _GamePausePopup;
    private bool _isGamePauseOpen = false;

    //게임오버 유아이
    private GameOverPopup _GameOverPopup;
    private bool _isGameOver = false;

    //로딩중 유아이 관련
    private GameLoadingPopup _gameLoadingPopup;
    private bool _isGameLoadingOpen = false;

    //게임클리어 유아이 관련
    private GameClearPopup _gameClearPopup;
    private bool _isgameClearOpen = false;

  
    public bool IsGamePauseOpen => _isGamePauseOpen;
    public bool IsGameClearOpen => _isgameClearOpen;
    public bool IsGameLoadingOpen => _isGameLoadingOpen;

    //============================게임정지 UI_Popup==============================

    public void ToggleGamePause()
    {
        if (_isGamePauseOpen == false )
        {
            OpenGamePause();
        }
        else
        {
            CloseGamePause();
        }
    }

    public void OpenGamePause()
    {
        if (_GamePausePopup == null)
        {
            _GamePausePopup = Managers.UI.ShowPopupUI<GamePausePopup>("GamePausePopup");
            Time.timeScale = 0;  // 게임 시간 멈춤
        }
        else
        {
            _GamePausePopup.gameObject.SetActive(true);
        }

        if (_GamePausePopup != null)
        {
            _isGamePauseOpen = true;
        }
        else
        {
            Debug.LogError("Popup을 생성하지 못했습니다.");
        }
    }

    public void CloseGamePause()
    {
        if (_GamePausePopup != null)
        {
            GameObject.Destroy(_GamePausePopup.gameObject);
            _GamePausePopup.gameObject.SetActive(false);
            _isGamePauseOpen = false;
            Time.timeScale = 1;  // 게임 시간 재개
        }
    }
    //========================================================================

    //============================게임오버 UI_Popup==============================

    public void ToggleGameOver()
    {
        if (_isGamePauseOpen)
        {
            OpenGameOver();
        }
        else
        {
            CloseGameOver();
        }
    }

    public void OpenGameOver()
    {
        if (_GameOverPopup == null)
        {
            _GameOverPopup = Managers.UI.ShowPopupUI<GameOverPopup>("GameOverPopup");
            Time.timeScale = 0;  // 게임 시간 멈춤
        }
        else
        {
            _GameOverPopup.gameObject.SetActive(true);
        }

        if (_GameOverPopup != null)
        {
            _isGameOver = true;
        }
        else
        {
            Debug.LogError("Popup을 생성하지 못했습니다.");
        }
    }

    public void CloseGameOver()
    {
        if (_GameOverPopup != null)
        {
            GameObject.Destroy(_GameOverPopup.gameObject);
            _GameOverPopup.gameObject.SetActive(false);
            _isGameOver = false;
            Time.timeScale = 1;  // 게임 시간 재개
        }
    }
    //========================================================================

    //===========================게임 로딩 UI=================================
    //UI 창 파괴생성 기능 담고있음(게임 시간 정지는 들어가 있지 않음)
    public void ToggleGameLoading()
    {
        if (_isGameLoadingOpen)
        {
            CloseGameLoading();
        }
        else
        {
            OpenGameLoading();
        }
    }

    public void OpenGameLoading()
    {
        if (_gameLoadingPopup == null)
        {
            _gameLoadingPopup = Managers.UI.ShowPopupUI<GameLoadingPopup>("GameLoadingPopup");
        }
        else
        {
            _gameLoadingPopup.gameObject.SetActive(true);
        }

        if (_gameLoadingPopup != null)
        {
            _isGameLoadingOpen = true;
        }
        else
        {
            Debug.LogError("InventoryPopup을 생성하지 못했습니다.");
        }
    }

    public void CloseGameLoading()
    {
        if (_gameLoadingPopup != null)
        {
            GameObject.Destroy(_gameLoadingPopup.gameObject);
            _isGameLoadingOpen = false;
            _gameLoadingPopup = null;
            Debug.Log("게임 로딩 끝");
        }
        else
        {

        }
    }
    //=========================================================================

    //=========================게임 Clear UI===================================
    public void ToggleGameClear()
    {
        if (_gameClearPopup)
        {
            CloseGameClear();
        }
        else
        {
            OpenGameClear();
        }
    }

    public void OpenGameClear()
    {
        if (_gameClearPopup == null)
        {
            _gameClearPopup = Managers.UI.ShowPopupUI<GameClearPopup>("GameClear");
            _gameClearPopup.gameObject.SetActive(true);
        }
        else
        {
            _gameClearPopup.gameObject.SetActive(true);
        }

        if (_gameClearPopup != null)
        {
            _isgameClearOpen = true;
        }
        else
        {
            Debug.LogError("InventoryPopup을 생성하지 못했습니다.");
        }
    }

    public void CloseGameClear()
    {
        if (_gameClearPopup != null)
        {
            _isgameClearOpen = false;
            _gameClearPopup.gameObject.SetActive(false);

        }
        else
        {

        }
    }

    //모든 팝업을 닫는 코드 게임이 종료되는 코드에는 이걸 무조건 실행시켜줘야 하며 모든 팝업을 닫는 코드는 여기다 넣어주세요
    public void RealAllClosePopup()
    {
        
        if (_GameOverPopup != null) CloseGameOver();
        if (_gameLoadingPopup != null) CloseGameLoading();
        if (_gameClearPopup != null) CloseGameClear();
        if (_GamePausePopup != null) CloseGamePause();

    }

}


