using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagsEditor : MonoBehaviour
{
    Prop CurrentProp;
    AICommandInstance CurrentCommandInstance;
    int CurrentParameterID;
    BitFlagCollection CurrentBitFlags;

    private Transform FlagListHolder;

    public GameObject FlagListTemplate;
    public GameObject ToggleTemplate;

    private bool UINeedsUpdating;

    private void Awake()
    {
        FlagListHolder = transform.Find("FlagLists");
    }

    private void LateUpdate()
    {
        if (UINeedsUpdating)
        {
            foreach (Transform Child in FlagListHolder)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Child.GetComponent<RectTransform>());
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(FlagListHolder.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            UINeedsUpdating = false;
        }
    }

    private void OnDisable()
    {
        Reset();
    }

    public void MarkAsNeedUpdating()
    {
        UINeedsUpdating = true;
        GetComponentInParent<FloatingWindow>().MarkAsNeedUpdating();
    }

    private void Reset()
    {
        CurrentProp = null;
        CurrentCommandInstance = null;
        CurrentBitFlags = null;

        foreach (Transform OldFlagList in transform.Find("FlagLists"))
        {
            Destroy(OldFlagList.gameObject);
        }
    }

    public void EnterProp(Prop InProp)
    {
        gameObject.SetActive(true);

        Reset();

        CurrentProp = InProp;

        if (CurrentProp.Type == PropType.Character)
        {
            AddFlagList("SPAWNFLAG");
            AddFlagList("CHRFLAG0");
            AddFlagList("CHRFLAG1");
        }
        else
        {
            AddFlagList("OBJFLAG");
            AddFlagList("OBJFLAG2");
            AddFlagList("OBJFLAG3");

            if (CurrentProp.Type == PropType.Door)
            {
                AddFlagList("DOORFLAG");
            }
        }

        foreach (Transform FlagList in FlagListHolder)
        {
            BitFlagCollection BitFlagList = CurrentProp.GetFlagList(FlagList.name);

            foreach (Transform Child in FlagList)
            {
                Toggle BitFlagToggle = Child.GetComponentInChildren<Toggle>();

                if (BitFlagList.HasBitFlag(Child.name))
                {
                    BitFlagToggle.SetIsOnWithoutNotify(true);
                }
            }
        }
    }

    public void EnterPropWithLockFlags(Prop InProp)
    {
        gameObject.SetActive(true);

        Reset();

        CurrentProp = InProp;

        AddFlagList("LOCKFLAG");

        foreach (Transform FlagList in FlagListHolder)
        {
            BitFlagCollection BitFlagList = CurrentProp.GetFlagList(FlagList.name);

            foreach (Transform Child in FlagList)
            {
                Toggle BitFlagToggle = Child.GetComponentInChildren<Toggle>();

                if (BitFlagList.HasBitFlag(Child.name))
                {
                    BitFlagToggle.SetIsOnWithoutNotify(true);
                }
            }
        }
    }

    public void EnterAICommandInstance(AICommandInstance InCommandInstance, int ParameterID)
    {
        gameObject.SetActive(true);

        Reset();

        CurrentCommandInstance = InCommandInstance;
        CurrentParameterID = ParameterID;

        CurrentBitFlags = (BitFlagCollection)InCommandInstance.Parameters[ParameterID];
        ParameterType CurrentParameterType = InCommandInstance.Command.ParameterTypes[ParameterID];

        GameObject FlagList = AddFlagList(CurrentParameterType.ConstantName);

        foreach(Transform Child in FlagList.transform)
        {
            Toggle BitFlagToggle = Child.GetComponentInChildren<Toggle>();

            if (CurrentBitFlags.HasBitFlag(Child.name))
            {
                BitFlagToggle.SetIsOnWithoutNotify(true);
            }
        }
    }
    
    public GameObject AddFlagList(string BitFlagName)
    {
        GameObject FlagList = Instantiate(FlagListTemplate, transform.Find("FlagLists"));
        FlagList.name = BitFlagName;
        List<BitFlag> FlagCollection;


        FlagCollection = GameBitFlagHolder.GetBitFlagList(BitFlagName);

        foreach (BitFlag CurrentBitFlag in FlagCollection)
        {
            MakeNewToggle(CurrentBitFlag, FlagList.transform);
        }

        if (!UINeedsUpdating)
        {
            MarkAsNeedUpdating();
        }

        return FlagList;
    }

    void MakeNewToggle(BitFlag CurrentBitFlag, Transform FlagList)
    {
        GameObject NewToggle = Instantiate(ToggleTemplate, FlagList);
        NewToggle.name = CurrentBitFlag.Constant;

        Toggle ToggleComponent = NewToggle.GetComponentInChildren<Toggle>();
        ToggleComponent.onValueChanged.AddListener(x => OnToggleClicked(ToggleComponent));

        TextMeshProUGUI Text = NewToggle.GetComponentInChildren<TextMeshProUGUI>();
        Text.text = CurrentBitFlag.Name ?? CurrentBitFlag.Constant;
        Text.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 280);
    }

    public void OnToggleClicked(Toggle EventToggle)
    {
        string ToggleName = EventToggle.transform.parent.name;
        BitFlag CurrentBitFlag = GameBitFlagHolder.GetBitFlag(ToggleName);
        //string ListName = EventToggle.transform.parent.name;

        if (CurrentProp != null)
        {
            if (EventToggle.isOn)
            {
                CurrentProp.AddFlags(CurrentBitFlag);
                Debug.Log(CurrentBitFlag.Bits);
                Debug.Log(CurrentBitFlag.Bits.ToString("X"));
            }
            else 
            {
                CurrentProp.RemoveFlags(CurrentBitFlag);
            }
        }
        else if (CurrentCommandInstance != null)
        {
            if (EventToggle.isOn)
            {
                CurrentBitFlags.Add(CurrentBitFlag);
                CurrentBitFlag.ConnectedAICommands.Add(new() { CommandInstance = CurrentCommandInstance, ParameterID = CurrentParameterID });
            }
            else
            {
                CurrentBitFlags.Remove(CurrentBitFlag);
                CurrentBitFlag.ConnectedAICommands.RemoveAll(x => x.CommandInstance == CurrentCommandInstance && x.ParameterID == CurrentParameterID);
            }
        }
    }
}
