using Il2CppScheduleOne.GameTime; 
using Il2CppScheduleOne.PlayerScripts; 
using Il2CppTMPro; 
using MelonLoader; 
using System.Collections; 
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(DayCounterRedux.DayCounter), "DayCounterRedux", "1.0.131", "Denveous", "#FFFF00")]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: MelonColor(255, 255, 0, 255)]
[assembly: MelonAuthorColor(255, 0, 255, 0)]

#nullable disable
namespace DayCounterRedux {
  internal class ModConfig {
    public bool CounterVisible = true;
    public int CounterLocation = 2;
    public float CounterWidth = 130f;
    public float XOffset = 0f; 
    public float YOffset = 0f;
    public float IconXOffset = 0f;
    public float IconYOffset = 0f;
    public float UIScale = 1.0f;
  }
  public class DayCounter : MelonMod {
    private static GameObject iconObject;
    private static UnityEngine.UI.Image iconImage;
    private static ModConfig config = new ModConfig();
    private static bool HasPlayerSpawned; 
    private static GameObject DayCounterObj; 
    private static TextMeshProUGUI DayText;
    public override void OnInitializeMelon() { 
        MelonPreferences_Category category = MelonPreferences.CreateCategory("DayCounterRedux", "Settings");
        category.CreateEntry<bool>("CounterVisible", config.CounterVisible, "Enable Day Counter", "Toggle day counter.", false, false, null, null);
        category.CreateEntry<int>("CounterLocation", config.CounterLocation, "Counter Location", "1 - topleft, 2 - topright, 3 - bottomleft, 4 - bottomright", false, false, null, null);
        category.CreateEntry<float>("CounterWidth", config.CounterWidth, "Counter Width", "Width of the day counter UI element.", false, false, null, null);
        category.CreateEntry<float>("XOffset", config.XOffset, "X Offset", "Horizontal offset for the day counter.", false, false, null, null);
        category.CreateEntry<float>("YOffset", config.YOffset, "Y Offset", "Vertical offset for the day counter.", false, false, null, null);
        category.CreateEntry<float>("IconXOffset", config.IconXOffset, "Icon X Offset", "Horizontal offset for the day counter icon.", false, false, null, null);
        category.CreateEntry<float>("IconYOffset", config.IconYOffset, "Icon Y Offset", "Vertical offset for the day counter icon.", false, false, null, null);
        category.CreateEntry<float>("UIScale", config.UIScale, "UI Scale", "Scale factor for the day counter.", false, false, null, null);
        this.LoadConfig();
    }
    private void LoadConfig() {
        config.CounterVisible = MelonPreferences.GetEntryValue<bool>("DayCounterRedux", "CounterVisible");
        config.CounterLocation = Mathf.Clamp(MelonPreferences.GetEntryValue<int>("DayCounterRedux", "CounterLocation"), 1, 4);
        config.CounterWidth = MelonPreferences.GetEntryValue<float>("DayCounterRedux", "CounterWidth");
        config.XOffset = MelonPreferences.GetEntryValue<float>("DayCounterRedux", "XOffset");
        config.YOffset = MelonPreferences.GetEntryValue<float>("DayCounterRedux", "YOffset");
        config.IconXOffset = MelonPreferences.GetEntryValue<float>("DayCounterRedux", "IconXOffset");
        config.IconYOffset = MelonPreferences.GetEntryValue<float>("DayCounterRedux", "IconYOffset");
        config.UIScale = Mathf.Clamp(MelonPreferences.GetEntryValue<float>("DayCounterRedux", "UIScale"), 0.5f, 2.0f);
        if (config.CounterWidth < 110f) config.CounterWidth = 110f;
    }
    public override void OnPreferencesSaved() {
       bool newVisibility = MelonPreferences.GetEntryValue<bool>("DayCounterRedux", "CounterVisible");
       int newLocation = Mathf.Clamp(MelonPreferences.GetEntryValue<int>("DayCounterRedux", "CounterLocation"), 1, 4);
       float newWidth = Mathf.Clamp(MelonPreferences.GetEntryValue<float>("DayCounterRedux", "CounterWidth"), 50f, 300f);
       float newXOffset = MelonPreferences.GetEntryValue<float>("DayCounterRedux", "XOffset");
       float newYOffset = MelonPreferences.GetEntryValue<float>("DayCounterRedux", "YOffset");
       float newIconXOffset = MelonPreferences.GetEntryValue<float>("DayCounterRedux", "IconXOffset");
       float newIconYOffset = MelonPreferences.GetEntryValue<float>("DayCounterRedux", "IconYOffset");
       float newUIScale = Mathf.Clamp(MelonPreferences.GetEntryValue<float>("DayCounterRedux", "UIScale"), 0.5f, 2.0f);

       if (newVisibility != config.CounterVisible) { config.CounterVisible = newVisibility; UpdateDayCounterVisibility(newVisibility); }
       if (newLocation != config.CounterLocation && newLocation >= 1 && newLocation <= 4) { config.CounterLocation = newLocation; SetDayCounterPosition(newLocation, newXOffset, newYOffset); }
       if (newWidth != config.CounterWidth) { config.CounterWidth = newWidth < 110f ? 110f : newWidth; UpdateDayCounterWidth(config.CounterWidth); }
       if (newXOffset != config.XOffset || newYOffset != config.YOffset) { config.XOffset = newXOffset; config.YOffset = newYOffset; SetDayCounterPosition(newLocation, newXOffset, newYOffset); }
       if (newIconXOffset != config.IconXOffset || newIconYOffset != config.IconYOffset) { config.IconXOffset = newIconXOffset; config.IconYOffset = newIconYOffset; UpdateIconPosition(newIconXOffset, newIconYOffset); }
       if (newUIScale != config.UIScale) { config.UIScale = newUIScale; UpdateDayCounterScale(newUIScale); }
    }
    public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
      if (sceneName == "Main") MelonCoroutines.Start(WaitForPlayer()); 
      else HasPlayerSpawned = false; 
    }
    private static IEnumerator WaitForPlayer() { 
      while (Player.Local == null || Player.Local.gameObject == null) yield return null; 
      if (!HasPlayerSpawned) { 
        HasPlayerSpawned = true; 
        MelonCoroutines.Start(OnPlayerSpawned()); 
      } 
    }
    private static Texture2D LoadIconFromResources(Il2CppSystem.String resourceName) {
        using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
            if (stream == null) { MelonLogger.Error("Failed to load icon resource: " + resourceName); return null; } 
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            return texture;
        }
    }
    private static IEnumerator OnPlayerSpawned() {
        yield return new WaitForSeconds(1f);
        GameObject playerObj = Player.Local?.gameObject;
        if (playerObj == null) yield break;
        CreateDayCounterUI();
        ConfigureDayCounter();
        SetupDayTracking();
    }
    private static void CreateDayCounterUI() {
        GameObject hudBackground = GameObject.Find("UI/HUD/Background");
        Transform hudParent = GameObject.Find("UI/HUD/").transform;
        DayCounterObj = Object.Instantiate(hudBackground, hudParent);
        DayCounterObj.name = "DayCounterRedux";
        DayText = DayCounterObj.transform.Find("TopScreenText").GetComponent<TextMeshProUGUI>();
        DayText.rectTransform.anchoredPosition = new Vector2(10f, 0f);
        CreateDayIcon();
    }
    private static void CreateDayIcon() {
        iconObject = new GameObject("DayIcon");
        iconObject.transform.SetParent(DayCounterObj.transform);
        iconImage = iconObject.AddComponent<UnityEngine.UI.Image>();
        iconImage.rectTransform.sizeDelta = new Vector2(24f, 24f);
        Texture2D iconTexture = LoadIconFromResources("DayCounterRedux.Resources.dayicon.png");
        if (iconTexture != null) {
            Rect textureRect = new Rect(0, 0, iconTexture.width, iconTexture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            iconImage.sprite = Sprite.Create(iconTexture, textureRect, pivot);
        }
    }
    private static void ConfigureDayCounter() {
        DayCounterObj.SetActive(true);
        SetDayCounterPosition(config.CounterLocation, config.XOffset, config.YOffset);
        UpdateDayCounterWidth(config.CounterWidth);
        UpdateDayCounterVisibility(config.CounterVisible);
        UpdateIconPosition(config.IconXOffset, config.IconYOffset);
    }
    private static void SetupDayTracking() {
        TimeManager timeManager = Object.FindObjectOfType<TimeManager>();
        timeManager.onDayPass += new System.Action(ChangeDayText);
        DayText.text = "Day " + (timeManager.ElapsedDays + 1).ToString();
    }
    private static void SetDayCounterPosition(int location, float xOffset, float yOffset) {
        if (DayCounterObj == null) return;
        RectTransform canvasRect = GameObject.Find("UI/HUD").GetComponent<RectTransform>();
        if (canvasRect == null) return;
        float xPos = 0f, yPos = 0f;
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        switch (location) {
            case 1: xPos = -canvasWidth/2 + 100; yPos = canvasHeight/2 - 50; break;
            case 2: xPos = canvasWidth/2 - 100; yPos = canvasHeight/2 - 50; break;
            case 3: xPos = -canvasWidth/2 + 100; yPos = -canvasHeight/2 + 50; break;
            case 4: xPos = canvasWidth/2 - 100; yPos = -canvasHeight/2 + 50; break;
            default: xPos = canvasWidth/2 - 100; yPos = canvasHeight/2 - 50; break;
        }
        DayCounterObj.transform.localPosition = new Vector3(xPos + xOffset, yPos + yOffset, 0f);
    }
    private static void UpdateDayCounterScale(float scale) { if (DayCounterObj != null) { DayCounterObj.transform.localScale = new Vector3(scale, scale, scale); } }
    private static void UpdateIconPosition(float x, float y) { if (iconImage != null) iconImage.rectTransform.anchoredPosition = new Vector2(-38f + x, 0f + y); }
    private static void UpdateDayCounterWidth(float width) { var rectTransform = DayCounterObj?.GetComponent<RectTransform>(); if (rectTransform != null) rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y); }
    private static void UpdateDayCounterVisibility(bool isVisible) => DayCounterObj?.SetActive(isVisible);
    private static void ChangeDayText() => DayText.text = $"Day {Object.FindObjectOfType<TimeManager>().ElapsedDays + 1}";
  }
} 