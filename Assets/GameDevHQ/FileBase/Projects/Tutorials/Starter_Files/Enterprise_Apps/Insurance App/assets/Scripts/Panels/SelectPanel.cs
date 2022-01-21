using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectPanel : MonoBehaviour, IPanel
{
    public Text informationText;
    public RawImage photoTaken;

    public void OnEnable()
    {
        informationText.text = UIManager.Instance.activeCase.name + "\n" + UIManager.Instance.activeCase.locationNotes + "\n" + UIManager.Instance.activeCase.date;

        Texture2D reconstructedImage = new Texture2D(1, 1);
        reconstructedImage.LoadImage(UIManager.Instance.activeCase.photoTaken);
        photoTaken.texture = (Texture)reconstructedImage;
    }
    public void ProcessInfo()
    {
        throw new System.NotImplementedException();
    }

    public void AcceptButton()
    {
        SceneManager.LoadScene(0);
    }
}
