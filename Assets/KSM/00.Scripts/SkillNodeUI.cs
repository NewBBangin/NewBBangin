using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillNodeUI : MonoBehaviour ,IPointerClickHandler
{
    public Image background;
   public TMP_Text label;
   public GameObject glow;
   public Color purchasedColor = new Color(0.18f, 0.24f, 0.32f, 1f);

   [Header("연출")]
   public float popDelay = 0.25f;     
   public float popDuration = 0.35f;
   public float punchDuration = 0.25f;

   public string NodeId { get; private set; }

   private bool _wasVisible; 
   private bool _wasBought;  
   private Color _baseColor;   

   public void Setup(NodeData data)
   {
      NodeId = data.id;
      label.text = data.id;
      gameObject.name = "Node" + data.id;
   }

   public void ApplyState(bool bought, bool unlocked, bool canBuy)
   {
      bool visible = bought || unlocked;

      bool justShown = visible && !_wasVisible;
      bool justBought = bought && !_wasBought;
      _wasVisible = visible;
      _wasBought = bought;

      gameObject.SetActive(visible);
      if (!visible) return;

      if (glow != null)
         glow.SetActive(!bought && canBuy);

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
      background.color = _baseColor;
      if (justBought)
      {
         StopAllCoroutines();
         StartCoroutine(Punch());
      }
      else if (justShown)
      {
         StopAllCoroutines();
         StartCoroutine(PopIn(popDelay));
      }
   }
   private IEnumerator PopIn(float delay)
   {
      transform.localScale = Vector3.zero;
      yield return new WaitForSecondsRealtime(delay);

      float t = 0f;
      while (t < popDuration)
      {
         t += Time.unscaledDeltaTime;
         float s = EaseOutBack(Mathf.Clamp01(t / popDuration));
         transform.localScale = Vector3.one * s;
         yield return null;
      }
      transform.localScale = Vector3.one;
   }
   private IEnumerator Punch()
   {
      float t = 0f;
      while (t < punchDuration)
      {
         t += Time.unscaledDeltaTime;
         float p = Mathf.Clamp01(t / punchDuration);

         float bump = Mathf.Sin(p * Mathf.PI);             
         transform.localScale = Vector3.one * (1f + 0.25f * bump);
         background.color = Color.Lerp(Color.white, _baseColor, p);
         yield return null;
      }
      transform.localScale = Vector3.one;
      background.color = _baseColor;
   }
   private void Update()
   {
      if (glow != null && glow.activeSelf)
      {
         float s = 1f + 0.08f * Mathf.Sin(Time.unscaledTime * 4f);
         glow.transform.localScale = Vector3.one * s;
      }
   }
   private float EaseOutBack(float x)
   {
      const float c1 = 1.70158f;
      const float c3 = c1 + 1f;
      return 1f + c3 * Mathf.Pow(x - 1f, 3) + c1 * Mathf.Pow(x - 1f, 2);
   }

   public void OnPointerClick(PointerEventData eventData)
   {
      if (eventData.button != PointerEventData.InputButton.Left) return;
      NodeManager.Instance.TryBuy(NodeId);
   }
}
