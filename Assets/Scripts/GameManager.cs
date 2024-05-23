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

    #region ĳ����
    [Header("ĳ����")]
    [SerializeField] private GameObject _characterObj; // ĳ���� ������
    public List<Character> Characters { get; private set; } = new List<Character>(); // ĳ���� ����Ʈ 
    public List<Cell> CharacterNextCell { get; private set; } = new List<Cell>(); // �� ĳ������ ���� �̵� ĭ ����Ʈ
    public int CharacterCnt => _characterData.Count * 2; // �� ĳ���� ��
    public bool IsValidToStart => CharacterCnt == Characters.Count; // ���� ���� ���� ���� Ȯ��

    [SerializeField] private List<CharacterData> _characterData; // ĳ���� ������ ���
    #endregion

    #region Ÿ�̸�
    private const int _duration = 60; // ���� �ð�(��)
    public int remainingDuration { get; private set; } // ���� �ð�
    #endregion

    #region ���� ����
    [Header("GameOver")]
    public Sprite WinImg; // �¸� �� ǥ���� �̹���
    public Sprite LoseImg; // �й� �� ǥ���� �̹���

    public bool gameOver { get; private set; } // ���� ���� ����
    public TeamType winnerTeam { get; private set; } // �¸� �� ����
    public void StartGameOver(TeamType team) => StartCoroutine(GameOverCoroutine(team)); // ���� ���� �� �̵�
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

    // �� �ε� �̺�Ʈ ó��
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

    private void CreateCharacter()     // ĳ���� ����
    {
        Characters.Clear();

        Vector3 pos = Vector3.zero;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < _characterData.Count; j++)
            {
                var obj = Instantiate(_characterObj, _characterObj.transform.position, Quaternion.identity);

                #region #ĳ���� �Ӽ� ����
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

    private Character AssignCharacterType(GameObject obj, Type jobType) // ĳ���� �Ӽ� ����
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


    public void PositionEnemies() // �� ĳ���� �������� ��ġ
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

    private void CharacterAction()  // ĳ���� �ൿ ó��
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

    public void SetTimer() // Ÿ�̸� ����
    {
        remainingDuration = _duration;
        StartCoroutine(UpdateTimer());
    }  

    private void TimeOver() //���� ������ �� ü���� �������� ���� ���� ����
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


    private IEnumerator UpdateTimer() //Ÿ�̸�
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

    public void UpdateCharacterInfo(Character character) // �Ʊ� ����â�� ĳ���� ���� ������Ʈ
    {
        if (!Characters.Contains(character))
        {
            for (int i = 0; i < _characterData.Count; i++)
            {
                var panelcell = UIManager.CharacterInfoContent.transform.GetChild(i);
                if (!panelcell.gameObject.activeSelf)
                {
                    panelcell.GetComponent<PanelCell>().CreatePanelCell(character); //�Ʊ� ����â �� ����
                    panelcell.gameObject.SetActive(true);
                    Characters.Add(character);
                    return;
                }
            }
        }
    }

    public void RemovePanelCell(Character character) => UIManager.FindPanelCell(character.Icon).gameObject.SetActive(false);
    public Vector2 NormalizePosition(Vector2 position) => new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));

    public Character GetCharacterAtPosition(Vector3 position) // Ư�� ��ġ�� �ִ� ĳ���� ��������
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
