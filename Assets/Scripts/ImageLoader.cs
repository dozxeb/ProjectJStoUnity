using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageLoader : MonoBehaviour
{
    private static ImageLoader instance;
    public static ImageLoader Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ImageLoader");
                instance = go.AddComponent<ImageLoader>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    public void LoadImage(string url, Image targetImage, Action<bool> onComplete = null)
    {
        StartCoroutine(LoadImageCoroutine(url, targetImage, onComplete));
    }

    public void LoadSprite(string url, Action<Sprite> onComplete)
    {
        StartCoroutine(LoadSpriteCoroutine(url, onComplete));
    }

    private IEnumerator LoadImageCoroutine(string url, Image targetImage, Action<bool> onComplete)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download image: {request.error}");
                onComplete?.Invoke(false);
                yield break;
            }

            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            
            if (targetImage != null)
            {
                targetImage.sprite = sprite;
            }
            
            onComplete?.Invoke(true);
        }
    }

    private IEnumerator LoadSpriteCoroutine(string url, Action<Sprite> onComplete)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download image: {request.error}");
                onComplete?.Invoke(null);
                yield break;
            }

            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            
            onComplete?.Invoke(sprite);
        }
    }
} 