using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextureEditor : MonoBehaviour
{
    public RawImage TextureImage;
    private int TextureNumber;

    // Start is called before the first frame update
    void Start()
    {
        TextureImage.texture = DataHolder.Materials[0].mainTexture;
        TextureNumber = 0;
    }

    public void Next()
    {
        TextureImage.texture = DataHolder.Materials[++TextureNumber].mainTexture;
        TextureImage.SetNativeSize();
    }

    public void Previous()
    {
        TextureImage.texture = DataHolder.Materials[--TextureNumber].mainTexture;
        TextureImage.SetNativeSize();
    }

    public void SetTexture()
    {
        if (int.TryParse(GetComponentInChildren<TMP_InputField>().text, System.Globalization.NumberStyles.HexNumber, null, out int Result))
        {
            TextureNumber = Result;
        }

        TextureImage.texture = DataHolder.Materials[TextureNumber].mainTexture;
        TextureImage.SetNativeSize();
    }
}
