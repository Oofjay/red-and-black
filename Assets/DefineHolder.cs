using System.Collections.Generic;

public class Constant
{
    public string Name;
    public int Value;
    public string DisplayName;
    public string Description;
}


public class ConstantHolder
{
    public static Dictionary<string, List<Constant>> ConstantCollection = new();

    public static Dictionary<string, int> ConstantSizes = new();

    public static void ClearConstantCollection()
    {
        ConstantCollection.Clear();
        ConstantSizes.Clear();
    }

    public static List<Constant> AddConstantList(string ListName)
    {
        List<Constant> NewList = new();
        ConstantCollection.Add(ListName, NewList);
        return NewList;
    }

    public static List<Constant> GetConstantList(string ListName)
    {
        return ConstantCollection[ListName];
    }

    public static void AddConstant(string ConstantType, Constant InConstant)
    {
        ConstantCollection[ConstantType].Add(InConstant);
    }

    public static Constant GetConstant(string ConstantType, string ConstantName)
    {
        return ConstantCollection[ConstantType].Find(x => x.Name == ConstantName);
    }

    public static Constant GetConstant(string ConstantType, int ConstantNumber)
    {
        return ConstantCollection[ConstantType].Find(x => x.Value == ConstantNumber);
    }

    public static Constant GetConstant(string ConstantName)
    {
        string ConstantType = ConstantName[..ConstantName.IndexOf('_')];

        return ConstantCollection[ConstantType].Find(x => x.Name == ConstantName);
    }

    public static int GetConstantIndex(string ConstantType, Constant InConstant)
    {
        return ConstantCollection[ConstantType].Find(x => x.Value == InConstant.Value).Value;
    }
}
