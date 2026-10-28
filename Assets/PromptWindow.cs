using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromptWindow : MonoBehaviour
{
    public GameObject ButtonTemplate;

    public GameObject InputBlocker;

    public GameObject ButtonHolder;
    public TextMeshProUGUI PromptText;

    public List<Button> Buttons;

    private void OnDisable()
    {
        foreach (Button CurrentButton in Buttons)
        {
            Destroy(CurrentButton.gameObject);
        }

        Buttons.Clear();

        InputBlocker.SetActive(false);
    }

    public void Display(int ButtonAmount, string Text, bool HasInputBlocker)
    {
        for (int i = 0; i < ButtonAmount; ++i)
        {
            GameObject NewButtonObject = Instantiate(ButtonTemplate, ButtonHolder.transform);

            Button NewButton = NewButtonObject.GetComponent<Button>();
            NewButton.onClick.AddListener(() => gameObject.SetActive(false));
            Buttons.Add(NewButton);
        }

        PromptText.text = Text;

        if (HasInputBlocker) { InputBlocker.SetActive(true); }

        gameObject.SetActive(true);
    }
}
