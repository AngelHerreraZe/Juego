using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class TestRenderer
{
    public static void RenderTestFrame()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        
        RenderTexture rt = new RenderTexture(1280, 720, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        screenShot.Apply();
        
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        
        byte[] bytes = screenShot.EncodeToPNG();
        Object.DestroyImmediate(screenShot);
        
        File.WriteAllBytes("D:/test_unity_render.png", bytes);
        Debug.Log("RENDER_TEST_SUCCESS_WRITTEN");
    }
}
