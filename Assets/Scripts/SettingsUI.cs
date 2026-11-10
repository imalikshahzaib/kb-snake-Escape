using UnityEngine;
using UnityEngine.UI;

namespace ArrowMaze
{
    public class SettingsUI : MonoBehaviour
    {
        public static SettingsUI Instance { get; private set; }
        public GameObject settingsPanel;
        public Slider musicSlider;
        public Slider sfxSlider;

        public bool IsOpen => settingsPanel != null && settingsPanel.activeSelf;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            // Auto-create UI if not assigned
            if (settingsPanel == null)
            {
                CreateSettingsUI();
            }
            
            // Sync sliders with current volumes
            if (musicSlider != null) 
            {
                musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
                musicSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetMusicVolume(val));
            }
            
            if (sfxSlider != null) 
            {
                sfxSlider.value = PlayerPrefs.GetFloat("SfxVolume", 1f);
                sfxSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetSfxVolume(val));
            }

            // Start hidden
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void ToggleSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
                if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            }
        }

        private void CreateSettingsUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // 1. Panel Background
            settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform));
            settingsPanel.transform.SetParent(canvas.transform, false);
            RectTransform pRt = settingsPanel.GetComponent<RectTransform>();
            pRt.sizeDelta = new Vector2(400, 300);
            pRt.anchoredPosition = Vector2.zero;
            settingsPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.9f);

            // 2. Title
            CreateText("TITLE", settingsPanel.transform, "SETTINGS", new Vector2(0, 110), 30);

            // 3. Music Label & Slider
            CreateText("MusicLabel", settingsPanel.transform, "MUSIC", new Vector2(-100, 40), 20);
            musicSlider = CreateSlider("MusicSlider", settingsPanel.transform, new Vector2(50, 40));

            // 4. SFX Label & Slider
            CreateText("SfxLabel", settingsPanel.transform, "SOUNDS", new Vector2(-100, -20), 20);
            sfxSlider = CreateSlider("SfxSlider", settingsPanel.transform, new Vector2(50, -20));

            // 5. Close Button
            GameObject closeBtn = new GameObject("CloseButton", typeof(RectTransform));
            closeBtn.transform.SetParent(settingsPanel.transform, false);
            RectTransform cRt = closeBtn.GetComponent<RectTransform>();
            cRt.anchoredPosition = new Vector2(0, -90);
            cRt.sizeDelta = new Vector2(150, 40);
            closeBtn.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);
            Button b = closeBtn.AddComponent<Button>();
            b.onClick.AddListener(ToggleSettings);
            CreateText("CloseText", closeBtn.transform, "CLOSE", Vector2.zero, 18);

            // 6. Settings Toggle Button (Top Left)
            GameObject toggleBtn = new GameObject("SettingsToggle", typeof(RectTransform));
            toggleBtn.transform.SetParent(canvas.transform, false);
            RectTransform tRt = toggleBtn.GetComponent<RectTransform>();
            tRt.anchorMin = tRt.anchorMax = new Vector2(0, 1);
            tRt.pivot = new Vector2(0, 1);
            tRt.anchoredPosition = new Vector2(20, -20);
            tRt.sizeDelta = new Vector2(60, 60);
            toggleBtn.AddComponent<Image>().color = new Color(1, 1, 1, 0.5f); // Semi-transparent gear placeholder
            Button tb = toggleBtn.AddComponent<Button>();
            tb.onClick.AddListener(ToggleSettings);
            CreateText("ToggleIcon", toggleBtn.transform, "⚙", Vector2.zero, 40).color = Color.black;
        }

        private Text CreateText(string name, Transform parent, string content, Vector2 pos, int size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Text t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text = content;
            t.fontSize = size;
            t.alignment = TextAnchor.MiddleCenter;
            go.GetComponent<RectTransform>().anchoredPosition = pos;
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
            return t;
        }

        private Slider CreateSlider(string name, Transform parent, Vector2 pos)
        {
            GameObject sliderGo = new GameObject(name, typeof(RectTransform));
            sliderGo.transform.SetParent(parent, false);
            Slider slider = sliderGo.AddComponent<Slider>();
            RectTransform rt = sliderGo.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(200, 20);

            // Background
            GameObject bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(sliderGo.transform, false);
            bg.AddComponent<Image>().color = Color.gray;
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;

            // Fill Area
            GameObject fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one; faRt.sizeDelta = new Vector2(-20, 0);

            GameObject fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            fill.AddComponent<Image>().color = new Color(0.2f, 0.6f, 1f);
            slider.fillRect = fill.GetComponent<RectTransform>();

            // Handle Area
            GameObject handleArea = new GameObject("HandleArea", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            RectTransform haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one; haRt.sizeDelta = new Vector2(-20, 0);

            GameObject handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            handle.AddComponent<Image>().color = Color.white;
            RectTransform hRt = handle.GetComponent<RectTransform>();
            hRt.sizeDelta = new Vector2(20, 20);
            slider.handleRect = hRt;

            slider.transition = Selectable.Transition.None;
            return slider;
        }
    }
}
