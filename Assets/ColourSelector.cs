using AdvancedColorPicker;
using UnityEngine;

public class ColourSelector : MonoBehaviour
{
    Tile CurrentTile;
    BackgroundLight[] CurrentLights;

    private FlexibleColorPicker Picker;

    private void OnEnable()
    {
        Picker = GetComponentInChildren<FlexibleColorPicker>();
    }

    public void EnterCollision(Tile InTile)
    {

        gameObject.SetActive(true);
    }

    public void EnterBackgroundLight(BackgroundLight[] InLights)
    {
        CurrentLights = InLights;
        
        if (CurrentLights.Length == 1) 
        {
            GetComponentInChildren<FlexibleColorPicker>().color = InLights[0].Colour;
        }
        else
        {
            GetComponentInChildren<FlexibleColorPicker>().color = Color.white;
        }

        gameObject.SetActive(true);
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
    }

    public void Apply()
    {
        foreach (BackgroundLight IteratedLight in CurrentLights)
        {
            IteratedLight.ChangeColour(Picker.color);
        }

        gameObject.SetActive(false);
    }
}
