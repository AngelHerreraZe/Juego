using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Coins = 0;
    public bool Level1Completed = false;
    public bool Level2Completed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (FindAnyObjectByType<FirebaseFirestoreManager>() == null)
        {
            GameObject fbObj = new GameObject("FirebaseFirestoreManager");
            fbObj.AddComponent<FirebaseFirestoreManager>();
        }
    }

    private void Start()
    {
        LoadProgress();
        SpawnSceneCoins();
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        SaveProgress();
    }

    public void CompleteLevel(int level)
    {
        if (level == 1) Level1Completed = true;
        if (level == 2) Level2Completed = true;
        SaveProgress();
    }

    public void SaveProgress()
    {
        if (FirebaseFirestoreManager.Instance != null)
        {
            FirebaseFirestoreManager.Instance.SavePlayerData(Coins, Level1Completed, Level2Completed);
        }
    }

    public void LoadProgress()
    {
        if (FirebaseFirestoreManager.Instance != null)
        {
            FirebaseFirestoreManager.Instance.LoadPlayerData((loadedCoins, l1, l2) =>
            {
                Coins = loadedCoins;
                Level1Completed = l1;
                Level2Completed = l2;
            });
        }
    }

    private void SpawnSceneCoins()
    {
        Coin[] existingCoins = FindObjectsByType<Coin>(FindObjectsInactive.Exclude);
        if (existingCoins.Length == 0)
        {
            float[] xPositions = new float[] { -0.3f, 0.2f, 0.7f, 1.2f };
            for (int i = 0; i < xPositions.Length; i++)
            {
                GameObject coinObj = new GameObject("Coin_" + (i + 1));
                coinObj.transform.position = new Vector3(xPositions[i], 0.35f, 0f);
                coinObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                coinObj.AddComponent<SpriteRenderer>();
                CircleCollider2D col = coinObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                coinObj.AddComponent<Coin>();
            }
        }
    }

    private void OnGUI()
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 13;
        boxStyle.alignment = TextAnchor.UpperLeft;

        GUI.Box(new Rect(10, 10, 240, 190), "Firebase Firestore Data", boxStyle);

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        labelStyle.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(20, 35, 220, 22), "Monedas: " + Coins, labelStyle);
        GUI.Label(new Rect(20, 58, 220, 22), "Nivel 1: " + (Level1Completed ? "Completado" : "Pendiente"), labelStyle);
        GUI.Label(new Rect(20, 81, 220, 22), "Nivel 2: " + (Level2Completed ? "Completado" : "Pendiente"), labelStyle);

        if (GUI.Button(new Rect(20, 108, 100, 26), "Fin Nivel 1"))
        {
            CompleteLevel(1);
        }

        if (GUI.Button(new Rect(125, 108, 100, 26), "Fin Nivel 2"))
        {
            CompleteLevel(2);
        }

        if (GUI.Button(new Rect(20, 140, 100, 26), "Guardar"))
        {
            SaveProgress();
        }

        if (GUI.Button(new Rect(125, 140, 100, 26), "Cargar"))
        {
            LoadProgress();
        }

        if (FirebaseFirestoreManager.Instance != null && !string.IsNullOrEmpty(FirebaseFirestoreManager.Instance.LastStatus))
        {
            GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.fontSize = 10;
            GUI.Label(new Rect(20, 170, 220, 25), FirebaseFirestoreManager.Instance.LastStatus, statusStyle);
        }
    }
}