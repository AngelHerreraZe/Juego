using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseFirestoreManager : MonoBehaviour
{
    public static FirebaseFirestoreManager Instance { get; private set; }

    public string ApiKey = "AIzaSyCK_ZvRPMi82jVB_HkSFJOA8B7hJvHzYlY";
    public string ProjectId = "juego-c0";
    public string CollectionName = "users";
    public string DocumentId = "player_1";

    public bool IsLoading { get; private set; }
    public bool IsSaving { get; private set; }
    public string LastStatus = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private string GetDocumentUrl()
    {
        return $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents/{CollectionName}/{DocumentId}?key={ApiKey}";
    }

    public void AddData()
    {
        if (GameManager.Instance != null)
        {
            SavePlayerData(GameManager.Instance.Coins, GameManager.Instance.Level1Completed, GameManager.Instance.Level2Completed);
        }
    }

    public void GetData()
    {
        if (GameManager.Instance != null)
        {
            LoadPlayerData((coins, level1, level2) =>
            {
                GameManager.Instance.Coins = coins;
                GameManager.Instance.Level1Completed = level1;
                GameManager.Instance.Level2Completed = level2;
            });
        }
    }

    public void SavePlayerData(int coins, bool level1Completed, bool level2Completed, Action onSuccess = null, Action<string> onError = null)
    {
        StartCoroutine(SaveCoroutine(coins, level1Completed, level2Completed, onSuccess, onError));
    }

    private IEnumerator SaveCoroutine(int coins, bool level1Completed, bool level2Completed, Action onSuccess, Action<string> onError)
    {
        IsSaving = true;
        LastStatus = "Guardando en Firestore...";

        string url = GetDocumentUrl();
        string jsonBody = "{\"fields\":{" +
                          "\"coins\":{\"integerValue\":\"" + coins + "\"}," +
                          "\"level1_completed\":{\"booleanValue\":" + (level1Completed ? "true" : "false") + "}," +
                          "\"level2_completed\":{\"booleanValue\":" + (level2Completed ? "true" : "false") + "}" +
                          "}}";

        byte[] rawData = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(rawData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            IsSaving = false;

            if (request.result == UnityWebRequest.Result.Success)
            {
                LastStatus = "Datos guardados correctamente en Firestore";
                Debug.Log("Firestore Save Success: " + request.downloadHandler.text);
                onSuccess?.Invoke();
            }
            else
            {
                LastStatus = "Error al guardar en Firestore: " + request.error;
                Debug.LogError("Firestore Save Error: " + request.error + " | " + request.downloadHandler.text);
                onError?.Invoke(request.error);
            }
        }
    }

    public void LoadPlayerData(Action<int, bool, bool> onLoaded, Action<string> onError = null)
    {
        StartCoroutine(LoadCoroutine(onLoaded, onError));
    }

    private IEnumerator LoadCoroutine(Action<int, bool, bool> onLoaded, Action<string> onError)
    {
        IsLoading = true;
        LastStatus = "Cargando desde Firestore...";

        string url = GetDocumentUrl();

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            IsLoading = false;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log("Firestore Load Success: " + json);

                int coins = 0;
                bool level1 = false;
                bool level2 = false;

                Match mCoins = Regex.Match(json, "\"coins\"\\s*:\\s*\\{\\s*\"integerValue\"\\s*:\\s*\"?(\\d+)\"?");
                if (mCoins.Success && int.TryParse(mCoins.Groups[1].Value, out int parsedCoins))
                {
                    coins = parsedCoins;
                }

                Match mL1 = Regex.Match(json, "\"level1_completed\"\\s*:\\s*\\{\\s*\"booleanValue\"\\s*:\\s*(true|false)");
                if (mL1.Success)
                {
                    level1 = bool.Parse(mL1.Groups[1].Value);
                }

                Match mL2 = Regex.Match(json, "\"level2_completed\"\\s*:\\s*\\{\\s*\"booleanValue\"\\s*:\\s*(true|false)");
                if (mL2.Success)
                {
                    level2 = bool.Parse(mL2.Groups[1].Value);
                }

                LastStatus = "Datos cargados: " + coins + " monedas, N1: " + level1 + ", N2: " + level2;
                onLoaded?.Invoke(coins, level1, level2);
            }
            else if (request.responseCode == 404)
            {
                LastStatus = "Jugador no encontrado en Firestore. Inicializando nuevo registro...";
                SavePlayerData(0, false, false, () =>
                {
                    onLoaded?.Invoke(0, false, false);
                });
            }
            else
            {
                LastStatus = "Error al cargar de Firestore: " + request.error;
                Debug.LogError("Firestore Load Error: " + request.error);
                onError?.Invoke(request.error);
            }
        }
    }
}