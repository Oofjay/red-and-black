using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using System.ComponentModel;

public class ScrollableList : MonoBehaviour
{
    public GameObject ListItemType;

    public List<ListItem> ListItems = new();
    public ListItem SelectedItem;

    private Transform Content;

    public event ListEvent ListItemSelected;

    public delegate void ListEvent(ListItem SelectedItem);

    // Start is called before the first frame update
    void Awake()
    {
        Content = transform.Find("Viewport/Content");
    }

    public void AddToList(List<string> ItemNames)
    {
        Clear();

        foreach (string CurrentItemName in ItemNames)
        {
            GameObject NewListItem = Instantiate(ListItemType, Content);

            NewListItem.name = "ListItem" + ListItems.Count.ToString();
            NewListItem.GetComponent<ListItem>().ID = ListItems.Count;
            NewListItem.GetComponentInChildren<TextMeshProUGUI>().text = CurrentItemName;

            ListItems.Add(NewListItem.GetComponent<ListItem>());
        }
    }

    public void AddToList(List<string> ItemNames, List<object> Objects)
    {
        if (ItemNames.Count != Objects.Count)
        {
            throw new ArgumentException("Error adding items to ScrollableList, ItemNames and Objects do not have the same amount of variables");
        }

        Clear();

        for (int i = 0; i < ItemNames.Count; ++i)
        {
            string CurrentItemName = ItemNames[i];
            object CurrentObject = Objects[i];

            GameObject NewListItem = Instantiate(ListItemType, Content);

            NewListItem.name = "ListItem" + ListItems.Count.ToString();
            NewListItem.GetComponent<ListItem>().ID = ListItems.Count;
            NewListItem.GetComponentInChildren<TextMeshProUGUI>().text = CurrentItemName;
            NewListItem.GetComponent<ListItem>().AssociatedObject = CurrentObject;

            ListItems.Add(NewListItem.GetComponent<ListItem>());
        }
    }

    public void AddItemAtEnd(string ItemName, bool SelectNewItem)
    {
        GameObject NewListItem = Instantiate(ListItemType, Content);

        NewListItem.name = ItemName;
        NewListItem.GetComponentInChildren<TextMeshProUGUI>().text = ItemName;

        ListItem ListItemComponent = NewListItem.GetComponent<ListItem>();
        ListItemComponent.ID = ListItems.Count;
        ListItems.Add(ListItemComponent);

        if (SelectNewItem)
        {
            ListItemComponent.Select();

            RectTransform ContentRectTransform = Content.GetComponent<RectTransform>();
            RectTransform RootRectTransform = GetComponent<RectTransform>();

            Vector3 HalfHeight = new(0, RootRectTransform.sizeDelta.y / 2 - 15, 0);

            ContentRectTransform.anchoredPosition = 30 * ListItemComponent.ID * Vector3.up - HalfHeight;
        }
    }

    public void AddItemAtIndex(int Index, string ItemName)
    {
        GameObject NewListItem = Instantiate(ListItemType, Content);
        NewListItem.transform.SetSiblingIndex(Index);

        NewListItem.name = ItemName;
        NewListItem.GetComponentInChildren<TextMeshProUGUI>().text = ItemName;

        ListItem ListItemComponent = NewListItem.GetComponent<ListItem>();
        ListItemComponent.ID = Index;
        ListItems.Insert(Index, ListItemComponent);

        for (int i = Index + 1; i < ListItems.Count; ++i)
        {
            ListItems[i].ID += 1;
        }

        ListItemComponent.Select();

        RectTransform ContentRectTransform = Content.GetComponent<RectTransform>();
        RectTransform RootRectTransform = GetComponent<RectTransform>();

        Vector3 HalfHeight = new(0, RootRectTransform.sizeDelta.y / 2 - 15, 0);

        Debug.Log("ID: " + ListItemComponent.ID + " 30*ID = " + (30 * ListItemComponent.ID));
        Debug.Log(30 * ListItemComponent.ID * Vector3.up);

        ContentRectTransform.anchoredPosition = (30 * ListItemComponent.ID + 30)* Vector2.up;// - HalfHeight;

        //Debug.Log(ContentRectTransform.anchoredPosition);
    }

    public void RemoveSelectedItem()
    {
        int Index = ListItems.IndexOf(SelectedItem);

        ListItems.Remove(SelectedItem);
        Destroy(SelectedItem.gameObject);

        if (ListItems.Count == 0)
        {
            return;
        }

        for (int i = Index; i < ListItems.Count; ++i)
        {
            ListItems[i].ID -= 1;
        }

        if (Index == ListItems.Count)
        {
            ListItems[Index - 1].Select();
        }
        else
        {
            ListItems[Index].Select();
        }
    }

    public void MoveSelectedItemUp()
    {
        if (SelectedItem == null || SelectedItem.ID == 0)
        {
            return;
        }

        SelectedItem.transform.SetSiblingIndex(SelectedItem.transform.GetSiblingIndex() - 1);
        SelectedItem.ID -= 1;

        SelectedItem.transform.parent.GetChild(SelectedItem.transform.GetSiblingIndex() + 1).GetComponent<ListItem>().ID += 1;

        return;
    }

    public void MoveSelectedItemDown()
    {
        if (SelectedItem == null || SelectedItem.ID == ListItems.Count - 1)
        {
            return;
        }

        SelectedItem.transform.SetSiblingIndex(SelectedItem.transform.GetSiblingIndex() + 1);
        SelectedItem.ID += 1;

        SelectedItem.transform.parent.GetChild(SelectedItem.transform.GetSiblingIndex() - 1).GetComponent<ListItem>().ID -= 1;

        return;
    }

    public void Clear()
    {
        if (Content != null) 
        {
            foreach (Transform Child in Content)
            {
                Destroy(Child.gameObject);
            }

            Vector3 ListPosition = Content.GetComponent<RectTransform>().position;
            ListPosition.y = 0;
            Content.GetComponent<RectTransform>().position = ListPosition;
        }

        ListItems.Clear();
    }

    public void OnListItemSelected(ListItem SelectedItem)
    {
        ListItemSelected?.Invoke(SelectedItem);
    }
}
