using System.Collections;
using System.Collections.Generic;
using KSM._00.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeView : MonoBehaviour
{
    public RectTransform linesLayer;
    public RectTransform nodesLayer;
    public SkillNodeUI nodePrefab;
    public RectTransform linePrefab;

    public float spacing = 140f;
    public float lineThickness = 6f;

    private Dictionary<string, SkillNodeUI> _nodeUIs = new();
    
    private List<SkillLineUI> _lineUIs = new();
    

    private void Start()
    {
       
        Build();

        NodeManager.Instance.OnChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    { 
        if(NodeManager.Instance != null)
            NodeManager.Instance.OnChanged -= Refresh;
    }

    public void Refresh()
    {
        foreach (SkillNodeUI ui in _nodeUIs.Values)
        {
            

            ui.ApplyState(NodeManager.Instance.IsBought(ui.NodeId), NodeManager.Instance.IsUnlock(ui.NodeId), NodeManager.Instance.CanBuy(ui.NodeId));
        }
        foreach (var line in _lineUIs)
        {
            bool fromVisible = _nodeUIs[line.fromId].gameObject.activeSelf;
            bool toVisible = _nodeUIs[line.toId].gameObject.activeSelf;
            bool visible = fromVisible && toVisible;

            bool justShown = visible && !line.wasVisible;
            line.wasVisible = visible;

            line.image.gameObject.SetActive(visible);
            if (!visible) continue;

            bool highlighted = NodeManager.Instance.IsBought(line.fromId)
                               && (NodeManager.Instance.IsBought(line.toId) || NodeManager.Instance.IsUnlock(line.toId));

            line.image.color = highlighted
                ? new Color(1f, 0.65f, 0.2f, 1f)
                : new Color(0.25f, 0.22f, 0.18f, 1f);

            if (justShown)
                StartCoroutine(GrowLine(line.image, 0.25f));
            else
                line.image.fillAmount = 1f;
        }
    }
    private IEnumerator GrowLine(Image img, float duration)
    {
        img.fillAmount = 0f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            img.fillAmount = Mathf.SmoothStep(0f, 1f, t / duration);
            yield return null;
        }
        img.fillAmount = 1f;
    }

    public void Build()
    {
        ClearView();

        var grid = NodeManager.Instance.LayoutGrid;
        var nodes = NodeManager.Instance.Nodes;

        int rowCount = grid.GetLength(0);
        int colCount = grid.GetLength(1);
        foreach (NodeData data in nodes.Values)
        {
            SkillNodeUI ui = Instantiate(nodePrefab, nodesLayer);
            ui.Setup(data);

            RectTransform rt = (RectTransform)ui.transform;
            rt.anchoredPosition =
                GridToPos(data.row, data.col, rowCount, colCount);

            _nodeUIs[data.id] = ui;
        }
        foreach (NodeData data in nodes.Values)
        {
            foreach (string childId in data.children)
            {
                RectTransform fromNode = (RectTransform)_nodeUIs[data.id].transform;

                RectTransform toNode = (RectTransform)_nodeUIs[childId].transform;

                Vector2 from = fromNode.anchoredPosition;
                Vector2 to = toNode.anchoredPosition;

                RectTransform line = CreateLine(from, to);
                Image lineImage = line.GetComponent<Image>();
                SkillLineUI lines = new();
                lines.image = lineImage;
                lines.fromId = data.id;
                lines.toId = childId;

                _lineUIs.Add(lines);
            }
        }
    }

    private void ClearView()
    {
        foreach (SkillNodeUI ui in _nodeUIs.Values)
        {
            if (ui == null) continue;

            ui.gameObject.SetActive(false);
            Destroy(ui.gameObject);
        }

        foreach (var line in _lineUIs)
        {
            if (line.image == null) continue;

            line.image.gameObject.SetActive(false);
            Destroy(line.image.gameObject);
        }

        _nodeUIs.Clear();
        _lineUIs.Clear();
    }

    private Vector2 GridToPos(
        int row,
        int col,
        int rowCount,
        int colCount)
    {
        float x = (col - (colCount - 1) / 2f) * spacing;
        float y = -(row - (rowCount - 1) / 2f) * spacing;

        return new Vector2(x, y);
    }

    private RectTransform CreateLine(Vector2 a, Vector2 b)
    {
        RectTransform line = Instantiate(linePrefab, linesLayer);
        Vector2 diff = b - a;

        line.anchoredPosition = (a + b) / 2f;
        line.sizeDelta = new Vector2(diff.magnitude, lineThickness);

        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        line.localEulerAngles = new Vector3(0f, 0f, angle);
        line.GetComponent<Image>().raycastTarget = false;

        return line;
    }
}