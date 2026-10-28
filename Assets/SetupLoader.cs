using TMPro;
using UnityEngine;

public class SetupLoader : MonoBehaviour
{
    public BetterDropdown SelectSetupDropdown;
    public TextMeshProUGUI BackgroundFile;
    public TextMeshProUGUI CollisionFile;
    public TextMeshProUGUI SetupFile;
    public TextMeshProUGUI PadsFile;

    public DataHolder DataHolderInstance;

    Stage CurrentStage;

    // Start is called before the first frame update
    void Start()
    {
        SelectSetupDropdown.OptionList = DropdownOptions.Stages;
    }

    private void OnEnable()
    {
        SelectSetupDropdown.CurrentIndex = 0;
    }

    public void OnSetupSelected()
    {
        CurrentStage = DataHolder.Stages[SelectSetupDropdown.CurrentIndex];

        BackgroundFile.text = "Background file: " + CurrentStage.BackgroundFile;
        CollisionFile.text = "Collision file: " + CurrentStage.CollisionFile;
        SetupFile.text = "Setup file: " + CurrentStage.SetupFile;
        PadsFile.text = "Pads file: " + CurrentStage.PadsFile;
    }

    public void LoadStage()
    {
        /*if (CurrentStage != null)
        {
            DataHolderInstance.LoadStage(CurrentStage);
            gameObject.SetActive(false);
        }*/
    }
}
