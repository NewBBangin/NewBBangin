using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTooltip : MonoBehaviour
{
    public static SkillTooltip Instance;

    public CanvasGroup group;
    public RectTransform rect;
    public TMP_Text titleText;
    public TMP_Text effectText;
    public TMP_Text priceText;
    public TMP_Text stateText;

    public float gap = 16f;   

    private Canvas _canvas;

    private void Awake()
    {
        Instance = this;
        _canvas = GetComponentInParent<Canvas>();

        group.alpha = 0f;
        group.blocksRaycasts = false; 
        group.interactable = false;
    }

    public void Show(NodeData data, RectTransform target)
    {
        Fill(data);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        Place(target);

        group.DOKill();
        rect.DOKill();
        group.DOFade(1f, 0.12f).SetUpdate(true);
        rect.localScale = Vector3.one * 0.9f;
        rect.DOScale(1f, 0.15f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void Hide()
    {
        group.DOKill();
        group.DOFade(0f, 0.1f).SetUpdate(true);
    }
    private void Fill(NodeData data)
    {
        var m = NodeManager.Instance;
        bool bought = m.IsBought(data.id);
        bool unlocked = m.IsUnlock(data.id);
        int lack = data.price - m.coins;

        titleText.text = data.name;
        effectText.text = data.effect;

        string priceColor = (!bought && lack > 0) ? "#FF6B5A" : "#F2C46A";
        priceText.text = $"가격  <color={priceColor}>{data.price} 코인</color>";

        if (bought)
            stateText.text = "구매 완료";
        else if (!unlocked)
            stateText.text = "잠김";
        else if (lack > 0)
            stateText.text = $"코인 {lack} 부족";
        else
            stateText.text = "클릭해서 구매";
    }
    private void Place(RectTransform target)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        float scale = _canvas.scaleFactor;
        Vector2 size = rect.rect.size * scale;  
        float centerY = (corners[0].y + corners[1].y) / 2f;
        rect.pivot = new Vector2(0f, 0.5f);
        float x = corners[2].x + gap * scale;
        if (x + size.x > Screen.width)
        {
            rect.pivot = new Vector2(1f, 0.5f);
            x = corners[0].x - gap * scale;
        }
        float y = Mathf.Clamp(centerY, size.y / 2f, Screen.height - size.y / 2f);

        rect.position = new Vector2(x, y);
    }
}