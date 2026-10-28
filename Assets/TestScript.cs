using TMPro;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private TMP_InputField Field;

    void Awake()
    {
        Field = GetComponent<TMP_InputField>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Field.onValueChanged.AddListener(OnTextValueChanged);
    }

    public void OnTextValueChanged(string NewValue)
    {
        Debug.Log(NewValue);
    }
}
