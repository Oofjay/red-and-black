using UnityEngine;
using UnityEngine.UI;

public class Tab : MonoBehaviour
{
    public GameObject TabWindow;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnTabClicked);
    }

    void OnTabClicked()
    {
        SwitchTo();
    }

    public void SwitchTo()
    {
        foreach (Tab CurrentTab in GetComponentInParent<FloatingWindow>().Tabs)
        {
            if (CurrentTab.TabWindow != null)
            {
                CurrentTab.TabWindow.SetActive(false);
                CurrentTab.GetComponent<Image>().color = new Color32(46, 46, 46, 255);
            }
        }
        if (TabWindow != null)
        {
            TabWindow.SetActive(true);
            GetComponent<Image>().color = new Color32(54, 54, 54, 255);
        }
    }    
}
