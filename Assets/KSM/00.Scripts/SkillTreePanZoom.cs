using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KSM._00.Scripts
{
    public class SkillTreePanZoom : MonoBehaviour ,IDragHandler ,IScrollHandler
    {
        public RectTransform content;
        private Canvas _canvas;
        [SerializeField] private float zoomPower;
        public float minZoom = 0.5f;  
        public float maxZoom = 1.5f;
        private Vector2 centerPos;

        private void Awake()
        {
           
            _canvas = GetComponentInParent<Canvas>();
        }

        public void CenterOn(Vector2 point)
        {
            content.anchoredPosition = -point * content.localScale.x;
            centerPos = point;

        }

        private void Update()
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                content.anchoredPosition = -centerPos * content.localScale.x;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 move =  eventData.delta / _canvas.scaleFactor;
             content.anchoredPosition += move;
        }

        public void OnScroll(PointerEventData eventData)
        {
            float nowScale = content.localScale.x;
            float newSclae = nowScale +(eventData.scrollDelta.y * zoomPower);
            newSclae = Mathf.Clamp(newSclae, minZoom, maxZoom);
            content.localScale = Vector3.one * newSclae; 
        }
    }
}