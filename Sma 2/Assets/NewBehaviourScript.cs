using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    public Image image;
    private Texture2D texture;
    private Sprite imageResult;

    // Start is called before the first frame update
   
    void Start()
    {
        StartCoroutine(DownloadImage("https://img.itch.zone/aW1nLzEyNjY2Nzc2LnBuZw==/35x35%23/cl5gI%2F.png", FilterMode.Point));
        Debug.Log("Start");
    }

    
   
    IEnumerator DownloadImage(string MediaUrl, FilterMode filterMode)
    {
        Debug.Log("Start");
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            Debug.LogError(request.error);
        else
          texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
          imageResult = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        imageResult.texture.filterMode = filterMode;
          Debug.Log(imageResult);
          image.sprite = imageResult;
          
    }
}
