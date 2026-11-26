using DG.Tweening;
using TMPro;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
   public Image background;
   public TMP_Text label;
   public GameObject glow;
   public Color purchasedColor = new Color(0.18f, 0.24f, 0.32f, 1f);

   [Header("연출")]
   public float popDelay = 0.25f;    
   public float popDuration = 0.35f;
   public float punchDuration = 0.3f;
   private NodeData _data;

   public string NodeId { get; private set; }

   private bool _wasVisible;
   private bool _wasBought;
   private bool _wasGlowing;
   private Color _baseColor;

   public void Setup(NodeData data)
   {
      _data = data;      
      NodeId = data.id;
      label.text = data.id;
      gameObject.name = "Node" + data.id;
   }

   public void ApplyState(bool bought, bool unlocked, bool canBuy)
   {
      bool visible = bought || unlocked;
      bool glowing = visible && !bought && canBuy;

      bool justShown = visible && !_wasVisible;
      bool justBought = bought && !_wasBought;
      bool glowChanged = glowing != _wasGlowing;
      _wasVisible = visible;
      _wasBought = bought;
      _wasGlowing = glowing;

      gameObject.SetActive(visible);
      if (!visible) return;
      if (bought)
      {
         _baseColor = purchasedColor;
         label.color = new Color(0.75f, 0.58f, 0.32f, 1f);
      }
      else
      {
         _baseColor = new Color(0.08f, 0.07f, 0.08f, 1f);
         label.color = canBuy ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
      }
      if (justBought) PlayPunch();
      else if (justShown) PlayPopIn();
      else background.color = _baseColor;

      if (glowChanged) SetGlow(glowing);
   }
   private void PlayPopIn()
   {
      transform.DOKill();
      background.color = _baseColor;
      transform.localScale = Vector3.zero;

      transform.DOScale(1f, popDuration)
         .SetDelay(popDelay)
         .SetEase(Ease.OutBack)
         .SetUpdate(true)      
         .SetLink(gameObject); 
   }
   private void PlayPunch()
   {
      transform.DOKill(true);
      background.DOKill();
      transform.localScale = Vector3.one;

      transform.DOPunchScale(Vector3.one * 0.25f, punchDuration, 6, 0.5f)
         .SetUpdate(true)
         .SetLink(gameObject);

      background.color = Color.white;
      background.DOColor(_baseColor, punchDuration)
         .SetUpdate(true)
         .SetLink(gameObject);
   }
   private void SetGlow(bool on)
   {
      if (glow == null) return;

      glow.transform.DOKill();
      glow.SetActive(on);
      if (!on) return;

      glow.transform.localScale = Vector3.one;
      glow.transform.DOScale(1.08f, 0.6f)
         .SetEase(Ease.InOutSine)
         .SetLoops(-1, LoopType.Yoyo)  
         .SetUpdate(true)
         .SetLink(glow);
   }

   public void OnPointerClick(PointerEventData eventData)
   {
      if (eventData.button != PointerEventData.InputButton.Left) return;
      if(eventData.dragging) return;
      NodeManager.Instance.TryBuy(NodeId);
      SkillTooltip.Instance.Show(_data, (RectTransform)transform); 
   }

   public void OnPointerEnter(PointerEventData eventData)
   {
      SkillTooltip.Instance.Show(_data, (RectTransform)transform);
   }

   public void OnPointerExit(PointerEventData eventData)
   {
      SkillTooltip.Instance.Hide();
   }
}