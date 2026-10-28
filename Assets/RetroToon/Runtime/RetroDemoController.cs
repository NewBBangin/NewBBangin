using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BBang.RetroToon
{
    [ExecuteAlways]
    public sealed class RetroDemoController : MonoBehaviour
    {
        public RetroStyleProfile profile;
        public Material printMaterial;
        public Light sun;
        public Camera viewCamera;
        public bool printEnabled = true;
        public bool showHUD = true;
        public Vector3 orbitTarget = new Vector3(0, 3.4f, 2);
        [FormerlySerializedAs("orbitSensitivity")]
        [Range(0.02f, 1)] public float lookSensitivity = 0.16f;
        [Min(0.1f)] public float moveSpeed = 9;
        [Min(1)] public float fastMoveMultiplier = 3;
        [Min(0.1f)] public float scrollMoveSpeed = 1.5f;
        private bool looking;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;
        private GUIStyle titleStyle, smallStyle;
        private void OnEnable() => ApplyStyle();
        private void OnValidate() => ApplyStyle();
        public void ApplyStyle()
        {
            if (!profile) return;
            profile.Apply(printMaterial, printEnabled);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = profile.fogColor;
            RenderSettings.fogStartDistance = profile.fogStart;
            RenderSettings.fogEndDistance = Mathf.Max(profile.fogStart + 1, profile.fogEnd);
            if (sun) { sun.color = profile.sunColor; sun.intensity = profile.sunIntensity; }
            if (viewCamera) viewCamera.backgroundColor = profile.fogColor;
        }
        private void Update()
        {
            ApplyStyle();
            if (!Application.isPlaying || !viewCamera) { EndMouseLook(); return; }
            if (!Application.isFocused) { EndMouseLook(); return; }
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null)
            {
                if (keyboard.f1Key.wasPressedThisFrame) printEnabled = !printEnabled;
                if (keyboard.hKey.wasPressedThisFrame) showHUD = !showHUD;
                if (keyboard.digit1Key.wasPressedThisFrame) SetView(0);
                if (keyboard.digit2Key.wasPressedThisFrame) SetView(1);
                if (keyboard.digit3Key.wasPressedThisFrame) SetView(2);
                if (keyboard.escapeKey.wasPressedThisFrame) EndMouseLook();
                float horizontal = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
                float forward = (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);
                float vertical = (keyboard.eKey.isPressed ? 1 : 0) - (keyboard.qKey.isPressed ? 1 : 0);
                Vector3 direction = viewCamera.transform.right * horizontal
                    + viewCamera.transform.forward * forward + Vector3.up * vertical;
                float boost = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed ? fastMoveMultiplier : 1;
                viewCamera.transform.position += Vector3.ClampMagnitude(direction, 1)
                    * (moveSpeed * boost * Time.unscaledDeltaTime);
            }
            if (mouse != null)
            {
                bool beginLook = mouse.rightButton.wasPressedThisFrame
                    && (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame);
                if (beginLook) BeginMouseLook();
                if (!mouse.rightButton.isPressed) EndMouseLook();
                // Skip the lock transition frame to avoid cursor-warp deltas.
                if (looking && !beginLook)
                {
                    var delta = mouse.delta.ReadValue() * lookSensitivity;
                    Vector3 angles = viewCamera.transform.eulerAngles;
                    float pitch = Mathf.Clamp(Mathf.DeltaAngle(0, angles.x) - delta.y, -85, 85);
                    viewCamera.transform.rotation = Quaternion.Euler(pitch, angles.y + delta.x, 0);
                }
                var scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    viewCamera.transform.position += viewCamera.transform.forward * (scroll / 120f * scrollMoveSpeed);
            }
            else EndMouseLook();
#endif
        }
        private void BeginMouseLook()
        {
            if (looking) return;
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            looking = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        private void EndMouseLook()
        {
            if (!looking) return;
            looking = false;
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }
        private void OnApplicationFocus(bool focused)
        {
            if (!focused) EndMouseLook();
        }
        public void SetView(int index)
        {
            if (!viewCamera) return;
            Vector3 position;
            if (index == 1) { position = new Vector3(-14, 7, -19); orbitTarget = new Vector3(0, 3.7f, 3); }
            else if (index == 2) { position = new Vector3(6.8f, 5.2f, -11); orbitTarget = new Vector3(0, 5.5f, 2); }
            else { position = new Vector3(10.8f, 6.8f, -17); orbitTarget = new Vector3(0, 4.3f, 2.5f); }
            viewCamera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(orbitTarget - position));
            viewCamera.fieldOfView = index == 2 ? 53 : 52;
        }
        private void OnDisable()
        {
            EndMouseLook();
            if (profile) profile.Apply(printMaterial, true);
        }
        private void OnGUI()
        {
            if (!Application.isPlaying || !showHUD) return;
            float scale = Mathf.Clamp(Screen.width / 1440f, 0.7f, 1.8f);
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 23, fontStyle = FontStyle.Bold };
                titleStyle.normal.textColor = new Color(1, 0.88f, 0.66f);
                smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true };
                smallStyle.normal.textColor = new Color(1, 0.9f, 0.77f);
            }
            var old = GUI.color;
            GUI.color = new Color(0.12f, 0.07f, 0.14f, 0.88f);
            GUI.DrawTexture(new Rect(24, 22, 408, 111), Texture2D.whiteTexture);
            GUI.color = old;
            GUI.Label(new Rect(38, 29, 380, 31), "BBANG / BURGER CLUB", titleStyle);
            GUI.Label(new Rect(39, 65, 370, 23), "RETRO INK / A LITTLE CORNER OF THE CITY", smallStyle);
            if (GUI.Button(new Rect(39, 96, 128, 24), printEnabled ? "PRINT ON [F1]" : "PRINT OFF [F1]")) printEnabled = !printEnabled;
            if (GUI.Button(new Rect(176, 96, 72, 24), "VIEW 1")) SetView(0);
            if (GUI.Button(new Rect(256, 96, 72, 24), "VIEW 2")) SetView(1);
            if (GUI.Button(new Rect(336, 96, 72, 24), "VIEW 3")) SetView(2);
            GUI.color = new Color(0.12f, 0.07f, 0.14f, 0.88f);
            float helpWidth = Mathf.Min(1080, Screen.width / scale - 48);
            GUI.DrawTexture(new Rect(24, Screen.height / scale - 76, helpWidth, 54), Texture2D.whiteTexture);
            GUI.color = old;
            string help = "WASD: fly    Q / E: down / up    Shift: faster    RMB: look    Scroll: move    1 / 2 / 3: views    H: hide";
            GUI.Label(new Rect(36, Screen.height / scale - 70, helpWidth - 24, 46), help, smallStyle);
            GUI.matrix = Matrix4x4.identity;
        }
    }
}
