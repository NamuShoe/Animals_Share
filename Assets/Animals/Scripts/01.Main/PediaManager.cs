using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine.Animations;
using UnityEngine.Serialization;

public class PediaManager : MonoBehaviour
{
    private enum IMAGE_TYPE
    {
        CharacterThumbnail,
        CharacterStanding,
        EnemyThumbnail,
        EnemyStanding
    }
    public static PediaManager instance;

    [SerializeField] private Transform animalKindContainer;
    [SerializeField] private Transform enemyKindContainer;
    [SerializeField] private GameObject animalKindPrefab;
    [SerializeField] private GameObject animalPrefab;
    [SerializeField] private List<Sprite> icon;

    [SerializeField] private Sprite basicBox;
    [SerializeField] private Sprite selectBox;
    //public List<CharacterData> pedia;
    public List<CharacterData> characters;
    public List<EnemyData> enemies;
    private Dictionary<int, GameObject> enemyDic = new Dictionary<int, GameObject>();
    private string characterSavePath;
    private string enemySavePath;
    
    [Header("Selected")]
    [SerializeField] private Image[] characterImage = new Image[2];
    [SerializeField] private Text[] characterName = new Text[2];
    [SerializeField] private Text[] characterDescription = new Text[2];
    [SerializeField] private Scrollbar[] descriptionScrollbar = new Scrollbar[2];
    [SerializeField] private GameObject stamp;
    private Sequence stampSeq;
    
    [Header("JustCheck")]
    [SerializeField] private List<GameObject> animalKinds = new List<GameObject>();
    [SerializeField] private List<GameObject> animals = new List<GameObject>();
    [SerializeField] private List<Image> animalIcons = new List<Image>();
    [SerializeField] private List<Text> animalKindName = new List<Text>();
    [SerializeField] private List<Transform> animalContainer = new List<Transform>();
    
    [Space(30f)]
    [SerializeField] private List<GameObject> enemyKinds = new List<GameObject>();
    [SerializeField] private List<GameObject> enemys = new List<GameObject>();
    [SerializeField] private List<Text> enemyKindName = new List<Text>();
    [SerializeField] private List<Transform> enemyContainer = new List<Transform>();

    [Header("PediaTurnTable")]
    [Tooltip("도감 페이지 상단 오른쪽 스왑")]
    [SerializeField] private GameObject[] Pedia_Book_Right_Obj; // 0 : 캐릭터 정보, 1 : 캐릭터 스토리

    [Header("SwapObjectList")] 
    private bool isCharacterPage = true;
    [SerializeField] private List<GameObject> characterObjects;
    [SerializeField] private List<GameObject> enemyObjects;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        //캐릭터
        for (int i = 0; i < System.Enum.GetValues(typeof(AnimalTypes)).Length; i++){
            CreateAnimalKind();
            animalIcons[i].sprite = icon[i];
            AnimalTypes animalTypes = (AnimalTypes)i;
            animalKindName[i].text = animalTypes.GetNameKr();
        }
        characterName[0].text = "";
        characterDescription[0].text = "";
        
        //적
        for (int i = 0; i < System.Enum.GetValues(typeof(EnmeyType)).Length; i++){
            CreateEnemyKind();
            EnmeyType enemyTypes = (EnmeyType)i;
            enemyKindName[i].text = enemyTypes.GetNameKr();
        }
        characterImage[1].gameObject.SetActive(false);
        characterName[1].text = "";
        characterDescription[1].text = "";
        
        LoadCharacterData();
        ReadSavedItems();
    }

    void CreateAnimalKind()
    {
        GameObject kindObject = Instantiate(animalKindPrefab, animalKindContainer);
        animalKinds.Add(kindObject);
        animalIcons.Add(kindObject.transform.GetChild(0).GetChild(0).GetComponent<Image>());
        animalKindName.Add(kindObject.transform.GetChild(0).GetChild(1).GetComponent<Text>());
        animalContainer.Add(kindObject.transform.GetChild(1));
    }

    void CreateEnemyKind()
    {
        GameObject kindObject = Instantiate(animalKindPrefab, enemyKindContainer);
        enemyKinds.Add(kindObject);
        enemyKindName.Add(kindObject.transform.GetChild(0).GetChild(1).GetComponent<Text>());
        enemyContainer.Add(kindObject.transform.GetChild(1));
        
        kindObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(false); // 아이콘 제거
    }

    // public void ResizeAnimalKindAll()
    // {
    //     for (int i = 0; i < animalKinds.Count; i++)
    //     {
    //         ResizeAnimalKind(animalKinds[i].GetComponent<RectTransform>(), animalContainer[i]);
    //     }
    //     
    //     for (int i = 0; i < enemyKinds.Count; i++)
    //     {
    //         ResizeAnimalKind(enemyKinds[i].GetComponent<RectTransform>(), enemyContainer[i]);
    //     }
    // }
    //
    // void ResizeAnimalKind(RectTransform animalKindTransform, Transform animalTransform)
    // {
    //     //if (aniamlsTransform.childCount % 5 == 1 && aniamlsTransform.childCount > 5)
    //     int count = animalTransform.childCount / 4 + 1;
    //     GridLayoutGroup gridLayoutGroup = animalTransform.gameObject.GetComponent<GridLayoutGroup>();
    //
    //     animalKindTransform.sizeDelta =
    //         new Vector2(animalKindTransform.sizeDelta.x,
    //             (gridLayoutGroup.cellSize.y * count) + (gridLayoutGroup.spacing.y * (count - 1)) +
    //             gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom);
    //
    //     Debug.LogError((gridLayoutGroup.cellSize.y * count) + "\t" + (gridLayoutGroup.spacing.y * (count - 1)) + "\t" +
    //                    gridLayoutGroup.padding.top + "\t" + gridLayoutGroup.padding.bottom + " = " + animalKindTransform.sizeDelta.y);
    //     Debug.LogError(count);
    // }
    
    public void InitTurnButton()
    {
        isCharacterPage = !isCharacterPage;

        foreach (var characterObject in characterObjects)
        {
            characterObject.SetActive(isCharacterPage);
        }

        foreach (var enemyObject in enemyObjects)
        {
            enemyObject.SetActive(!isCharacterPage);
        }
        
        SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Book);
        
        // if(isCharacterPage)
        //     animals[DataManager.instance.userData.CurrentCharacterId - 1].GetComponent<Button>().onClick.Invoke();
        // else
        //     enemyDic.First().Value.GetComponent<Button>().onClick.Invoke();
    }
    
    GameObject CreateAnimal(int num)
    {
        GameObject animal = Instantiate(animalPrefab, animalContainer[num]);
        return animal;
    }

    GameObject CreateEnemy(int num)
    {
        GameObject enemy = Instantiate(animalPrefab, enemyContainer[num]);
        return enemy;
    }
    
    private Sprite GetImage(IMAGE_TYPE imageType, int id)
    {
        var type = '\0';
        switch (imageType)
        {
            case IMAGE_TYPE.CharacterThumbnail:
            case IMAGE_TYPE.CharacterStanding:
                type = 'c';
                break;
            case IMAGE_TYPE.EnemyThumbnail:
            case IMAGE_TYPE.EnemyStanding:
                type = 'e';
                break;
            default:
                Debug.Log("잘못된 type이 입력되었습니다.");
                break;
        }
        
        string imagePath = "CharacterList/" + imageType.ToString() + "/" + type + id.ToString("D3");
        Sprite image = Resources.Load<Sprite>(imagePath);

        return image;
    }
    
    public void CharacterData(int id)
    {
        characterImage[0].sprite = GetImage(IMAGE_TYPE.CharacterStanding, id);
        characterName[0].text = characters[id - 1].name;
        characterDescription[0].text = characters[id - 1].description;
        StartCoroutine(ResetDescriptionScrollBarValue());
    }

    private void EnemyData(int id)
    {
        var enemy = enemies.Find(e => e.id == id);
        characterImage[1].gameObject.SetActive(true);
        characterImage[1].sprite = GetImage(IMAGE_TYPE.EnemyStanding, id);
        characterName[1].text = enemy.name;
        characterDescription[1].text = enemy.description;
        StartCoroutine(ResetDescriptionScrollBarValue());
    }

    private IEnumerator ResetDescriptionScrollBarValue()
    {
        yield return null;
        yield return null;
        descriptionScrollbar[0].value = 1f;
        descriptionScrollbar[1].value = 1f;
    }
    
    public void SelectCharacter(int id, bool isSound = true)
    {
        stampSeq.Pause();
        
        stampSeq = DOTween.Sequence();
        
        animals[DataManager.instance.userData.CurrentCharacterId - 1].transform.GetComponent<Image>().sprite = basicBox;

        animals[DataManager.instance.userData.CurrentCharacterId - 1].GetComponent<Button>().interactable = true;
        DataManager.instance.userData.CurrentCharacterId = id;
        animals[id - 1].GetComponent<Button>().interactable = false;
        
        
        DataManager.instance.SaveUserData();
        DataManager.instance.ChangeBasicStats();

        animals[id - 1].transform.GetComponent<Image>().sprite = selectBox;

        var source = new ConstraintSource { sourceTransform = animals[id - 1].transform, weight = 1 };
        stamp.GetComponent<PositionConstraint>().SetSource(0, source);
        stamp.GetComponent<PositionConstraint>().constraintActive = true;
        stamp.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        stamp.transform.localScale = Vector3.one * 1.1f;
        stampSeq.Append(stamp.GetComponent<Image>().DOFade(1.0f, 0.3f))
            .Join(stamp.GetComponent<RectTransform>().DOScale(Vector3.one * 1.2f, 0.3f))
            .Append(stamp.GetComponent<RectTransform>().DOScale(Vector3.one * 0.9f, 0.1f))
            .Append(stamp.GetComponent<RectTransform>().DOScale(Vector3.one * 1.0f, 0.05f));
        
        if(isSound)
            SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Select);
    }

    private void SelectEnemy(int id)
    {
        foreach (var value in enemyDic.Values)
        {
            value.transform.GetComponent<Image>().sprite = basicBox;
            value.GetComponent<Button>().interactable = false;
        }
        
        foreach (var enemyClear in DataManager.instance.userData.enemyClearCheck) {
            enemyDic[enemyClear].GetComponent<Button>().interactable = true;
        }
        
        enemyDic[id].transform.GetComponent<Image>().sprite = selectBox;
        enemyDic[id].GetComponent<Button>().interactable = false;

    }

    private void ReadSavedItems()
    {
        foreach (var character in characters)
        {
            GameObject buttonInstance = CreateAnimal((int)character.animalTypes); //임시
            buttonInstance.transform.GetChild(0).GetComponent<Image>().sprite = GetImage(IMAGE_TYPE.CharacterThumbnail, character.id);
            buttonInstance.GetComponent<Button>().onClick.AddListener(() => {
                CharacterData(character.id);
                SelectCharacter(character.id);
            });
            animals.Add(buttonInstance);
        }
        
        enemyDic.Clear();
        foreach (var enemy in enemies)
        {
            GameObject buttonInstance = CreateEnemy((int)enemy.enemyType);
            buttonInstance.transform.GetChild(0).GetComponent<Image>().sprite = GetImage(IMAGE_TYPE.EnemyThumbnail, enemy.id);
            buttonInstance.GetComponent<Button>().onClick.AddListener(() => {
                EnemyData(enemy.id);
                SelectEnemy(enemy.id);
            });
            enemyDic.Add(enemy.id, buttonInstance);
        }
    }

    public void CharacterPossession()
    {
        foreach (var animal in animals)
        {
            animal.transform.GetChild(0).GetComponent<Image>().color = Color.black;
            animal.transform.GetChild(1).gameObject.SetActive(true);
            animal.GetComponent<Button>().interactable = false;
        }
        
        foreach (var chara_id in DataManager.instance.userData.characterList)
        {
            Transform animal = animals[chara_id - 1].transform;
            animal.GetChild(0).GetComponent<Image>().color = Color.white;
            animal.GetChild(1).gameObject.SetActive(false);
            animal.GetComponent<Button>().interactable = true;
        }
    }

    public void EnemyPossession()
    {
        foreach (var enemy in enemyDic) {
            enemy.Value.transform.GetChild(0).GetComponent<Image>().color = Color.black;
            enemy.Value.transform.GetChild(1).gameObject.SetActive(true);
            enemy.Value.GetComponent<Button>().interactable = false;
        }

        foreach (var enemy_id in DataManager.instance.userData.enemyClearCheck) {
            Transform enemy = enemyDic[enemy_id].transform;
            enemy.GetChild(0).GetComponent<Image>().color = Color.white;
            enemy.GetChild(1).gameObject.SetActive(false);
            enemy.GetComponent<Button>().interactable = true;
        }
    }

    public void GetCharacter(int id)
    {
        if (DataManager.instance.userData.characterList.Contains(id))
            return;
        
        DataManager.instance.userData.characterList.Add(id);
        DataManager.instance.userData.characterInforms.Add(new CharacterSpecific(id));
        
        DataManager.instance.SaveUserData();
        CharacterPossession();
        MainManager.instance.ProfileSet();
    }
    
    private void CreateCharacterJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("GameData/CharacterData");
        //string txtData = Path.Combine(Application.dataPath, "Resources/GameData/CharacterData.json");
        File.WriteAllText(characterSavePath, jsonFile.ToString());
    }
    
    private void LoadCharacterData()
    {
        TextAsset jsonFile = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.CharacterData);// GetCharacterDataAsset();
        if (jsonFile != null)
        {
            characters = JsonConvert.DeserializeObject<List<CharacterData>>(jsonFile.text);
        }

        jsonFile = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.EnemyData); //GetEnemyDataAsset();
        if (jsonFile != null)
        {
            enemies = JsonConvert.DeserializeObject<List<EnemyData>>(jsonFile.text);
        }
        // if (!File.Exists(characterSavePath))
        // {
        //     CreateCharacterJson();
        // }
        // string jsonFile = File.ReadAllText(characterSavePath);
        // characters = JsonHelper.FromJson<CharacterData>(jsonFile);
        //
        // if (File.Exists(enemySavePath))
        // {
        //     jsonFile = File.ReadAllText(enemySavePath);
        //     var enemyDatas = JsonHelper.FromJson<EnemyData>(jsonFile);
        //     enemies = enemyDatas.ToList();
        // }
    }
}