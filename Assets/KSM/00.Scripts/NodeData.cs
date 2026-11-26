using System.Collections.Generic;
using UnityEngine;
public class NodeData
{
    public UpgradeTarget target;
    public UpgradeMethod method;
    public float value;
    public string id;
    public string name;
    public int price;
    public string effect;
    public string[] directions;   
    public int row;
    public int col;
    public List<string> children = new();  
    public List<string> parents = new();    
}

public enum UpgradeTarget
{
    None,
    MaxSpeed,
    SawBlade,
    MaxHP
}

public enum UpgradeMethod
{
    Add,
    Multiply,
    Unlock
}


