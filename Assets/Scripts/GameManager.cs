using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GridManager GridManager { get; private set; }

    #region UI
    [SerializeField] private UIManager _uiManager;
    public UIManager UIManager { get { return _uiManager; } }
    public void SetUiManager(UIManager ui) => this._uiManager = ui;
    #endregion

    #region 캐릭터
    [Header("캐릭터")]
    [SerializeField] private GameObject _characterObj; // 캐릭터 프리팹
    public List<Character> Characters { get; private set; } = new List<Character>(); // 캐릭터 리스트 
    public List<Cell> CharacterNextCell { get; private set; } = new List<Cell>(); // 각 캐릭터의 다음 이동 칸 리스트
    public int CharacterCnt => _characterData.Count * 2; // 총 캐릭터 수
    public bool IsValidToStart => CharacterCnt == Characters.Count; // 게임 시작 가능 여부 확인

    [SerializeField] private List<CharacterData> _characterData; // 캐릭터 데이터 목록
    #endregion

    #region 타이머
    private const int _duration = 60; // 게임 시간(초)
    public int remainingDuration { get; private set; } // 남은 시간
    #endregion

    #region 게임 오버
    [Header("GameOver")]
    public Sprite WinImg; // 승리 시 표시할 이미지
    public Sprite LoseImg; // 패배 시 표시할 이미지

    public bool gameOver { get; private set; } // 게임 오버 여부
    public TeamType winnerTeam { get; private set; } // 승리 팀 유형
    public void StartGameOver(TeamType team) => StartCoroutine(GameOverCoroutine(team)); // 게임 오버 씬 이동
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        DestroyImmediate(gameObject);
    }

    private void Start()
    {
        if (!gameOver)
        {
            Initialized();
            CreateCharacter();
        }
        SceneManager.sceneLoaded += RestartEvent;
    }

    private void Update()
    {
        if (Characters.Count == CharacterCnt && !gameOver)
        {
            foreach (var character in Characters)
            {
                if (character.IsValid && character.HP <= 0) character.Die();
            }
        }
    }

    public void Restart()
    {
        gameOver = false;
        SceneManager.LoadScene(0);
    }

    // 씬 로드 이벤트 처리
    private void RestartEvent(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            CreateCharacter();
            GridManager.ActiveGrid();
        }
    }


    private void Initialized()
    {
        GridManager = GetComponent<GridManager>();
    }

    private void CreateCharacter()     // 캐릭터 생성
    {
        Characters.Clear();

        Vector3 pos = Vector3.zero;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < _characterData.Count; j++)
            {
                var obj = Instantiate(_characterObj, _characterObj.transform.position, Quaternion.identity);

                #region #캐릭터 속성 세팅
                Character character = AssignCharacterType(obj, _characterData[j].JobType);
                character?.Create(_characterData[j], (TeamType)i);
                #endregion

                if (TeamType.ENEMY == character.TeamType)
                {
                    character.name = $"Enemy {j}";
                    Destroy(character.GetComponent<PlayerController>());
                    character.gameObject.SetActive(false);
                    Characters.Add(character);
                }
                else
                {
                    character.name = $"Knight {j}";
                    character.transform.position = pos + new Vector3(-1, j, -5);
                }
            }
        }
    }

    private Character AssignCharacterType(GameObject obj, Type jobType) // 캐릭터 속성 세팅
    {
        switch (jobType)
        {
            case Type.MELEE:
                obj.AddComponent<MeleeController>();
                break;
            case Type.MELEEAREA:
                obj.AddComponent<MeleeAreaController>();
                break;
            case Type.RANGED:
                obj.AddComponent<RangedController>();
                break;
        }
        return obj.GetComponent<Character>();
    }


    public void PositionEnemies() // 적 캐릭터 랜덤으로 배치
    {
        if (Characters.Count == 0) return;

        foreach (var character in Characters)
        {
            if (character.TeamType == TeamType.ENEMY)
            {
                PlaceEnemyRandomly(character);
            }
        }
    }

    private void PlaceEnemyRandomly(Character character)
    {
        Cell cell = null;
        do
        {
            int x = Random.Range(0, GridManager.Width);
            int y = Random.Range(0, GridManager.Height);
            cell = GridManager.GetTileAtPosition(new Vector2(x, y));
        }
        while (cell == null || !cell.IsWalkable);

        cell.IsWalkable = false;
        character.transform.position = new Vector3(cell.transform.position.x, cell.transform.position.y, character.transform.position.z);
        character.gameObject.SetActive(true);
    }

    private void CharacterAction()  // 캐릭터 행동 처리
    {
        CharacterNextCell.Clear();
        foreach (var character in Characters)
        {
            if (character.IsValid)
            {
                character.ThinkAction();
            }
        }
    }

    public void SetTimer() // 타이머 설정
    {
        remainingDuration = _duration;
        StartCoroutine(UpdateTimer());
    }  

    private void TimeOver() //남은 유닛의 총 체력을 기준으로 승패 여부 결정
    {
        float knight = 0f;
        float enemy = 0f;
        foreach (var character in Characters)
        {
            if (character.IsValid)
            {
                if (character.TeamType == TeamType.KNIGHT) knight += character.HP;
                else enemy += character.HP;
            }
        }

        StartGameOver(knight > enemy ? TeamType.KNIGHT : TeamType.ENEMY);
    }


    private IEnumerator UpdateTimer() //타이머
    {
        while (remainingDuration > 0 && !gameOver)
        {
            UIManager.SetTimeTxt($"{remainingDuration / 60:00}:{remainingDuration % 60:00}");
            remainingDuration--;
            yield return new WaitForSeconds(1f);
            CharacterAction();
        }

        if (remainingDuration <= 0)
        {
            TimeOver();
        }
    }

    public void UpdateCharacterInfo(Character character) // 아군 정보창에 캐릭터 정보 업데이트
    {
        if (!Characters.Contains(character))
        {
            for (int i = 0; i < _characterData.Count; i++)
            {
                var panelcell = UIManager.CharacterInfoContent.transform.GetChild(i);
                if (!panelcell.gameObject.activeSelf)
                {
                    panelcell.GetComponent<PanelCell>().CreatePanelCell(character); //아군 정보창 셀 생성
                    panelcell.gameObject.SetActive(true);
                    Characters.Add(character);
                    return;
                }
            }
        }
    }

    public void RemovePanelCell(Character character) => UIManager.FindPanelCell(character.Icon).gameObject.SetActive(false);
    public Vector2 NormalizePosition(Vector2 position) => new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));

    public Character GetCharacterAtPosition(Vector3 position) // 특정 위치에 있는 캐릭터 가져오기
    {
        return Characters.Find(c => c.IsValid && (NormalizePosition(c.transform.position) == NormalizePosition(position)));
    }  

    private IEnumerator GameOverCoroutine(TeamType team)
    {
        gameOver = true;
        winnerTeam = team;

        yield return new WaitForSeconds(1.5f);

        foreach (Character character in Characters)
        {
            if (character.IsValid) character.gameObject.SetActive(false);
        }

        int sceneNum = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneNum + 1);

        foreach (Cell cells in this.GetComponentsInChildren<Cell>())
        {
            cells.gameObject.SetActive(false);
        }
    }

    public void Quit() => Application.Quit();
}
