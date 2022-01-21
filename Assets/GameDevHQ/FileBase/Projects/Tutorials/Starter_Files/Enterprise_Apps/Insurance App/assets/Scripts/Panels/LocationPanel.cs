using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LocationPanel : MonoBehaviour, IPanel
{
    public RawImage mapImg;
    public InputField mapNotes;
    public Text caseNumberTitle;

    public string apiKey;
    public float xCord, yCord;
    public int zoom;
    public int imgSize;
    public string url = "https://maps.googleapis.com/maps/api/staticmap?";

    public void OnEnable()
    {
        caseNumberTitle.text = "CASE NUMBER " + UIManager.Instance.activeCase.caseID;
    }

    public IEnumerator Start()
    {
        if (Input.location.isEnabledByUser == true)
        {
            Input.location.Start();

            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1.0f);
                maxWait--;
            }

            if (maxWait < 1)
            {
                Debug.Log("Timed Out");
                yield break;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.LogError("Unable to determine device location");
            }
            else
            {
                xCord = Input.location.lastData.latitude;
                yCord = Input.location.lastData.longitude;
            }

            Input.location.Stop();
        }

        else
        {
            Debug.Log("Location Service are not enabled");
        }
        

        StartCoroutine(GetLocationRoutine());

        // https://maps.googleapis.com/maps/api/staticmap?center=40.714728,-73.998672&zoom=14&size=400x400&key=YOUR_API_KEY    
    }

    IEnumerator GetLocationRoutine()
    {
        url = url + "center=" + xCord + "," + yCord + "&zoom=" + zoom + "&size=" + imgSize + "x" + imgSize + "&key=" + apiKey;

        using (UnityWebRequest map = UnityWebRequestTexture.GetTexture(url))
        {
            yield return map.SendWebRequest();

            if (map.error != null)
            {
                Debug.LogError("Map Error: " + map.error);
            }

            mapImg.texture = DownloadHandlerTexture.GetContent(map);
        }
    }

    public void ProcessInfo()
    {
        if (string.IsNullOrEmpty(mapNotes.text) == false)
        {
            UIManager.Instance.activeCase.locationNotes = mapNotes.text;
        }
    }

}
