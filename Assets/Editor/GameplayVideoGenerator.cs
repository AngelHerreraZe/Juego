using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class GameplayVideoGenerator
{
    public static void GenerateGameplayFrames()
    {
        string framesDir = "D:/Juego/Frames";
        if (Directory.Exists(framesDir))
        {
            Directory.Delete(framesDir, true);
        }
        Directory.CreateDirectory(framesDir);

        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

        GameObject john = GameObject.Find("John");
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        int width = 1280;
        int height = 720;
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

        int totalFrames = 600; // 20 seconds at 30 fps for the game section
        Vector3 initialJohnPos = john != null ? john.transform.position : Vector3.zero;
        Vector3 initialCamPos = cam.transform.position;

        // UI Mockup Texture / Overlay
        Texture2D uiTex = new Texture2D(250, 280, TextureFormat.RGBA32, false);
        Color boxBg = new Color(0.12f, 0.12f, 0.14f, 0.92f);
        for (int x = 0; x < 250; x++)
        {
            for (int y = 0; y < 280; y++)
            {
                if (x == 0 || x == 249 || y == 0 || y == 279)
                    uiTex.SetPixel(x, y, new Color(0.3f, 0.6f, 1.0f, 1.0f));
                else
                    uiTex.SetPixel(x, y, boxBg);
            }
        }
        uiTex.Apply();

        int coinsCollected = 0;

        for (int i = 0; i < totalFrames; i++)
        {
            float t = (float)i / 30.0f; // time in seconds

            if (john != null)
            {
                // Simulate running right
                float moveX = initialJohnPos.x + t * 2.2f;
                float moveY = initialJohnPos.y;

                // Simulate 2 jumps
                if (t >= 3.0f && t <= 4.2f)
                {
                    float jumpPhase = (t - 3.0f) / 1.2f;
                    moveY += Mathf.Sin(jumpPhase * Mathf.PI) * 1.8f;
                }
                else if (t >= 7.5f && t <= 8.7f)
                {
                    float jumpPhase = (t - 7.5f) / 1.2f;
                    moveY += Mathf.Sin(jumpPhase * Mathf.PI) * 1.5f;
                }

                john.transform.position = new Vector3(moveX, moveY, initialJohnPos.z);

                // Coin collection logic
                if (t > 4.0f && coinsCollected == 0) coinsCollected = 1;
                if (t > 8.5f && coinsCollected == 1) coinsCollected = 2;
                if (t > 12.0f && coinsCollected == 2) coinsCollected = 3;

                // Camera follow
                cam.transform.position = new Vector3(moveX + 1.5f, initialCamPos.y, initialCamPos.z);
            }

            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            // Stamp UI Box onto screenShot
            for (int ux = 0; ux < 250; ux++)
            {
                for (int uy = 0; uy < 280; uy++)
                {
                    int px = 20 + ux;
                    int py = height - 300 + uy;
                    if (px < width && py < height && py >= 0)
                    {
                        Color c = uiTex.GetPixel(ux, uy);
                        screenShot.SetPixel(px, py, c);
                    }
                }
            }
            screenShot.Apply();

            byte[] bytes = screenShot.EncodeToPNG();
            string filename = string.Format("{0}/frame_{1:D4}.png", framesDir, i);
            File.WriteAllBytes(filename, bytes);
        }

        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(screenShot);
        Object.DestroyImmediate(uiTex);

        Debug.Log("GAMEPLAY_FRAMES_GENERATED: " + totalFrames);
    }
}
