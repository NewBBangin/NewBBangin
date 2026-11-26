using System;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
   public static NodeManager Instance;
   public TextAsset layoutCsv;
   public TextAsset nodesCsv;
   private string[,] _layoutGrid;
   private Dictionary<string, NodeData> _nodeDict = new();

   private HashSet<string> _bought = new();
   public string[,] LayoutGrid => _layoutGrid;
   public Dictionary<string, NodeData> Nodes => _nodeDict;
   public int coins = 100;
   public event Action OnChanged;

   private readonly Dictionary<string, Vector2Int> _dirOffset = new()
   {
      { "U",  new Vector2Int(-1,  0) },
      { "D",  new Vector2Int( 1,  0) },
      { "L",  new Vector2Int( 0, -1) },
      { "R",  new Vector2Int( 0,  1) },
      { "UL", new Vector2Int(-1, -1) },
      { "UR", new Vector2Int(-1,  1) },
      { "DL", new Vector2Int( 1, -1) },
      { "DR", new Vector2Int( 1,  1) },
   };

   private void Awake()
   {
      if (Instance == null)
         Instance = this;
      else
      {
         Destroy(gameObject);
         return;
      }
      ParseLayout();
      ParseNodes();
      BuildLinks();
   }

   public bool TryBuy(string id)
   {
      if(!CanBuy(id)) return false;
      coins -= _nodeDict[id].price;
      _bought.Add(id);
      Debug.Log($"현재 속도 증가량: {GetAddBonus(UpgradeTarget.MaxSpeed)}");
      Debug.Log(id +$"노드를{_nodeDict[id].price}가격에 구매하였습니다 남은 코인 {coins}");
      OnChanged?.Invoke();
      return true;
   }

   public bool IsBought(string id)
   {
      return _bought.Contains(id);
   }

   public bool IsUnlock(string id)
   {
      if(!_nodeDict.ContainsKey(id))   return false;
      NodeData node = _nodeDict[id];
      if (node.parents.Count == 0) return true;
      foreach (var a in node.parents)
      {
         if (IsBought(a))
         {
            return true;
         }
      }

      return false;


   }

   public bool CanBuy(string id)
   {
      if (!_nodeDict.ContainsKey(id)) return false;
      if (IsBought(id)) return false;
      if (!IsUnlock(id)) return false;
      NodeData node = _nodeDict[id];
      if(node.price > coins) return false;
      return true;
   }


   private List<string> SplitLines(string text)
   {
      string[] rawLines = text.Split('\n');
      List<string> lines = new();

      foreach (string raw in rawLines)
      {
         string clean = raw.Trim('\r');
         if (string.IsNullOrWhiteSpace(clean)) continue;
         lines.Add(clean);
      }
      return lines;
   }

   public void ParseLayout()
   {
      List<string> lines = SplitLines(layoutCsv.text);
      int rowCount = lines.Count;
      int colCount = 0;

      foreach (string line in lines)
      {
         int count = line.Split(',').Length;
         if (count > colCount)
         {
            colCount = count;
         }
      }

      _layoutGrid = new string[rowCount, colCount];
      for (int r = 0; r < rowCount; r++)
      {
         string[] cells = lines[r].Split(',');
         for (int c = 0; c < colCount; c++)
         {
            if (c < cells.Length)
               _layoutGrid[r, c] = cells[c].Trim();
            else
            {
               _layoutGrid[r, c] = "";
            }

            if (_layoutGrid[r, c] != "")
            {
               Debug.Log($"[{r},{c}] = {_layoutGrid[r, c]}");
            }
         }
      }
   }
   public void ParseNodes()
   {
      List<string> lines = SplitLines(nodesCsv.text);

      for (int i = 1; i < lines.Count; i++)   
      {
         string[] cell = lines[i].Split(',');
         NodeData data = new();
         data.id = cell[0].Trim();
         data.name = cell[1].Trim();
         data.price = int.Parse(cell[2].Trim());
         data.effect = cell[3].Trim();
         data.target = Enum.Parse<UpgradeTarget>(cell[5].Trim());
         data.method = Enum.Parse<UpgradeMethod>(cell[6].Trim());
         data.value = float.Parse(cell[7].Trim(),System.Globalization.CultureInfo.InvariantCulture);
         Debug.Log($" 아이디 {data.id}: 타겟 {data.target} 메소드 {data.method} 값 {data.value}");
         

         if (cell.Length > 4)
            data.directions = cell[4].Split(' ', StringSplitOptions.RemoveEmptyEntries);
         else
         {
            data.directions = new string[0];
         }

         _nodeDict.Add(data.id, data);
      }
   }

   public void BuildLinks()
   {
      int rowCount = _layoutGrid.GetLength(0);
      int colCount = _layoutGrid.GetLength(1);

      for (int r = 0; r < rowCount; r++)
      {
         for (int c = 0; c < colCount; c++)
         {
            string id = _layoutGrid[r, c];
            if (!_nodeDict.ContainsKey(id))
            {
               continue;
            }

            _nodeDict[id].row = r;
            _nodeDict[id].col = c;
         }
      }

      foreach (NodeData node in _nodeDict.Values)
      {
         foreach (string dir in node.directions)
         {
            if (!_dirOffset.ContainsKey(dir))
            {
               continue;
            }

            Vector2Int offset = _dirOffset[dir];
            int targetRow = node.row + offset.x;
            int targetCol = node.col + offset.y;

            if (targetRow < 0 || targetRow >= rowCount || targetCol < 0 || targetCol >= colCount)
            {
               continue;
            }

            string targetId = _layoutGrid[targetRow, targetCol];
            if (targetId == "" || !_nodeDict.ContainsKey(targetId))
            {
               continue;
            }

            node.children.Add(targetId);
            _nodeDict[targetId].parents.Add(node.id);
         }
      }

      foreach (NodeData node in _nodeDict.Values)
      {
         string start = node.parents.Count == 0 ? " ← 시작 노드" : "";
         Debug.Log($"{node.id} 사면 열림: [{string.Join(", ", node.children)}]{start}");
      }

  
   }

   public float GetAddBonus(UpgradeTarget target)
   {
      float total = 0f;
      foreach (var id in _bought)
      {
         NodeData data =  _nodeDict[id];
         if (data.target == target && data.method == UpgradeMethod.Add)
         {
            total += data.value;
         }
         
      }
   return total;

   }
   public float GetMultiplier(UpgradeTarget target)
   {
      float total = 1f;
      foreach (var id in _bought)
      {
         NodeData data =  _nodeDict[id];
         if (data.target == target && data.method == UpgradeMethod.Multiply)
         {
            total *= data.value;
         }
         
      }
       

      return total;
   }
   
   public float CalculateStat(UpgradeTarget target, float baseValue)
   {
      return (baseValue + GetAddBonus(target)) * GetMultiplier(target);
   }

   public bool IsUpgradeUnlocked(UpgradeTarget target)
   {
      if (target == UpgradeTarget.None)
      {
         return false;
      }

      foreach (string id in _bought)
      {
         NodeData data = _nodeDict[id];

         if (data.target == target &&
             data.method == UpgradeMethod.Unlock)
         {
            return true;
         }
      }

      return false;
   }


}