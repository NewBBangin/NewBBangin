using System.Collections.Generic;
using UnityEngine;
public class NodeData 
{
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


