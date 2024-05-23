using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [Header("시작 버튼")]
    private Button _startBtn;

    [Header("타이머")]
    [SerializeField] private Text _uiTimerTxt;

    [Header("GameOver")]
    [SerializeField] private Image _resultImg;

    [Header("Restart")]
    [SerializeField] private Button _restartBtn;

    [Header("종료")]
    [SerializeField] private Button _quitBtn;

    [Header("팝업")]
    [SerializeField] private Text _uiPopTxt;

    [Header("캐릭터 정보 UI")]
    [SerializeField] private GameObject _characterInfoContent;
    [SerializeField] private PanelCell _panelCell;
    public GameObject CharacterInfoContent => _characterInfoContent;
    public void SetTimeTxt(string text) => _uiTimerTxt.text = text;

    private void Start()
    {
        if (!GameManager.Instance.gameOver)
        {
            if (GameManager.Instance.UIManager == null) GameManager.Instance.SetUiManager(this);

            _startBtn = transform.GetChild(0).GetComponent<Button>();
            _startBtn?.onClick.AddListener(HandleStartButtonClicked);

            _uiPopTxt.gameObject.SetActive(false);

            CreatePanelCells();
        }
        else
        {
            InitializeGameOverUI();
        }
    }

    private void InitializeGameOverUI()
    {
        _restartBtn = transform.GetComponentInChildren<Button>();
        _quitBtn = transform.GetComponentInChildren<Button>();
        _restartBtn?.onClick.AddListener(GameManager.Instance.Restart);
        _quitBtn?.onClick.AddListener(GameManager.Instance.Quit);

        switch (GameManager.Instance.winnerTeam)
        {
            case TeamType.KNIGHT:
                _resultImg.sprite = GameManager.Instance.WinImg;
                break;
            case TeamType.ENEMY:
                _resultImg.sprite = GameManager.Instance.LoseImg;
                break;
        }
    }

    private void CreatePanelCells()
    {
        for (int i = 0; i < GameManager.Instance.CharacterCnt / 2; i++)
        {
            var panelCell = Instantiate(_panelCell, _characterInfoContent.transform);
            panelCell.gameObject.SetActive(false);
        }
    }

    private void HandleStartButtonClicked()
    {
        if (GameManager.Instance.IsValidToStart)
        {
            _uiPopTxt.gameObject.SetActive(false);
            _startBtn.interactable = false;

            GameManager.Instance.SetTimer();
            GameManager.Instance.PositionEnemies();
        }
        else
        {
            _uiPopTxt.gameObject.SetActive(true);
        }
    }

    public PanelCell FindPanelCell(Sprite sprite)
    {
        foreach (var cell in _characterInfoContent.GetComponentsInChildren<PanelCell>())
        {
            if (sprite == cell.IconImg.sprite)
            {
                return cell;
            }
        }
        return null;
    }

    public void UpdateFillIcon(bool isMove, Image icon)
    {
        if (icon == null) return;
        icon.fillAmount = 0;

        if (!isMove && !GameManager.Instance.gameOver) StartCoroutine(FillIcon(icon));
    }

    private IEnumerator FillIcon(Image icon)
    {
        float timer = 0f;
        while(timer < 1f)
        {
            timer += Time.deltaTime;
            icon.fillAmount = Mathf.InverseLerp(0f, 1f, timer);
            yield return new WaitForFixedUpdate();
        }
    }

}
