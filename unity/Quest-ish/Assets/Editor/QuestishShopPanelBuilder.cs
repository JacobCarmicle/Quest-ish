using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class QuestishShopPanelBuilder
{
    [MenuItem("Tools/Quest-ish/Create Shop Panel (UGUI)")]
    public static void CreateShopPanel()
    {
        // Find Canvas (prefer selected's parent canvas)
        Canvas canvas = null;
        if (Selection.activeGameObject != null)
            canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            EditorUtility.DisplayDialog("No Canvas found",
                "Please create/select a Canvas first (or select any UI element under a Canvas).",
                "OK");
            return;
        }

        if (canvas.transform.Find("ShopPanel") != null)
        {
            EditorUtility.DisplayDialog("ShopPanel already exists",
                "Canvas already contains a 'ShopPanel'. Delete/rename it first if you want to regenerate.",
                "OK");
            return;
        }

        // Root overlay panel (disabled by default)
        var shopPanel = CreateUIObject("ShopPanel", canvas.transform);
        Stretch(shopPanel.GetComponent<RectTransform>());
        var overlayImg = shopPanel.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.55f); // dim background
        shopPanel.SetActive(false);

        // Card (the visual "panel background" - assign wooden sprite here later)
        var card = CreateUIObject("Card", shopPanel.transform);
        var cardRT = card.GetComponent<RectTransform>();
        AnchorCenter(cardRT);
        cardRT.sizeDelta = new Vector2(950, 820);

        var cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.12f, 0.12f, 0.12f, 0.88f); // tint until you set sprite
        cardImg.raycastTarget = false; // card itself shouldn't block button clicks

        // Card layout
        var cardVLG = card.AddComponent<VerticalLayoutGroup>();
        cardVLG.childAlignment = TextAnchor.UpperCenter;
        cardVLG.spacing = 18;
        cardVLG.padding = new RectOffset(60, 60, 50, 50);
        cardVLG.childControlWidth = true;
        cardVLG.childControlHeight = true;
        cardVLG.childForceExpandWidth = true;
        cardVLG.childForceExpandHeight = false;

        // Title
        var titleGO = CreateTMP(card.transform, "TitleText", "Shop", 56, TextAlignmentOptions.Center);
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 80;

        // Subtitle
        var subtitleGO = CreateTMP(card.transform, "SubtitleText",
            "Support development & unlock extras", 28, TextAlignmentOptions.Center);
        var subtitle = subtitleGO.GetComponent<TextMeshProUGUI>();
        subtitle.color = new Color(1, 1, 1, 0.85f);
        var subLE = subtitleGO.AddComponent<LayoutElement>();
        subLE.preferredHeight = 55;

        // Spacer
        CreateSpacer(card.transform, 10);

        // Scroll area (product list)
        var scrollRoot = CreateUIObject("ProductScroll", card.transform);
        var scrollLE = scrollRoot.AddComponent<LayoutElement>();
        scrollLE.preferredHeight = 520;

        var scrollRT = scrollRoot.GetComponent<RectTransform>();
        scrollRT.sizeDelta = new Vector2(0, 520);

        var scrollRect = scrollRoot.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        // Scroll viewport
        var viewport = CreateUIObject("Viewport", scrollRoot.transform);
        var viewportRT = viewport.GetComponent<RectTransform>();
        Stretch(viewportRT);
        var viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0.06f);
        viewportImg.raycastTarget = true; // needed for scrolling drag
        var mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content
        var content = CreateUIObject("Content", viewport.transform);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = new Vector2(0, 0);

        var contentVLG = content.AddComponent<VerticalLayoutGroup>();
        contentVLG.childAlignment = TextAnchor.UpperCenter;
        contentVLG.spacing = 16;
        contentVLG.padding = new RectOffset(0, 0, 0, 0);
        contentVLG.childControlWidth = true;
        contentVLG.childControlHeight = true;
        contentVLG.childForceExpandWidth = true;
        contentVLG.childForceExpandHeight = false;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRT;
        scrollRect.content = contentRT;

        // Example products
        CreateProductCard(content.transform,
            "Quest-ish Plus (Monthly)",
            "$4.99 / month",
            "Remove ads • bonus cosmetics • support dev",
            "BuyMonthlyButton");

        CreateProductCard(content.transform,
            "Quest-ish Plus (Yearly)",
            "$49.99 / year",
            "Best value • everything in monthly",
            "BuyYearlyButton");

        CreateProductCard(content.transform,
            "Remove Ads",
            "$2.99 one-time",
            "Keep the game clean & uncluttered",
            "RemoveAdsButton");

        // Spacer
        CreateSpacer(card.transform, 8);

        // Close button row
        var closeRow = CreateUIObject("CloseRow", card.transform);
        var closeRowLE = closeRow.AddComponent<LayoutElement>();
        closeRowLE.preferredHeight = 90;

        var closeHLG = closeRow.AddComponent<HorizontalLayoutGroup>();
        closeHLG.childAlignment = TextAnchor.MiddleCenter;
        closeHLG.spacing = 20;
        closeHLG.childControlWidth = true;
        closeHLG.childControlHeight = true;
        closeHLG.childForceExpandWidth = true;
        closeHLG.childForceExpandHeight = false;

        var closeBtn = CreateButtonTMP(closeRow.transform, "CloseButton", "Back");
        var closeBtnLE = closeBtn.AddComponent<LayoutElement>();
        closeBtnLE.preferredWidth = 360;
        closeBtnLE.preferredHeight = 78;

        // Make sure ShopPanel renders on top of other panels
        shopPanel.transform.SetAsLastSibling();

        Selection.activeGameObject = shopPanel;
        EditorGUIUtility.PingObject(shopPanel);

        EditorUtility.DisplayDialog(
            "ShopPanel created ✅",
            "Created ShopPanel under Canvas (disabled by default).\n\nNext:\n1) Assign your wooden panel sprite to ShopPanel/Card Image.\n2) Wire Shop button to open ShopPanel.\n3) Wire Back to close ShopPanel.\n4) Later: hook purchase buttons to your IAP/subscription code.",
            "OK"
        );
    }

    // ---------- UI helpers ----------

    static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    static void AnchorCenter(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    static GameObject CreateTMP(Transform parent, string name, string text, float size, TextAlignmentOptions align)
    {
        var go = CreateUIObject(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        return go;
    }

    static void CreateSpacer(Transform parent, float height)
    {
        var spacer = CreateUIObject("Spacer", parent);
        var le = spacer.AddComponent<LayoutElement>();
        le.preferredHeight = height;
    }

    static GameObject CreateButtonTMP(Transform parent, string name, string label)
    {
        var btnGO = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnGO.name = name;
        btnGO.transform.SetParent(parent, false);

        var img = btnGO.GetComponent<Image>();
        img.color = new Color(0.85f, 0.35f, 0.40f, 1f);

        // Replace legacy Text with TMP
        var legacyText = btnGO.GetComponentInChildren<Text>();
        if (legacyText != null)
        {
            var textObj = legacyText.gameObject;
            Object.DestroyImmediate(legacyText);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 34;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var rt = tmp.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        return btnGO;
    }

    static void CreateProductCard(Transform parent, string title, string price, string desc, string buyButtonName)
    {
        var card = CreateUIObject(title.Replace(" ", "") + "_Card", parent);
        var img = card.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.06f);
        img.raycastTarget = false;

        var le = card.AddComponent<LayoutElement>();
        le.preferredHeight = 170;

        var vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.spacing = 6;
        vlg.padding = new RectOffset(22, 22, 18, 18);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var titleGO = CreateTMP(card.transform, "Title", title, 30, TextAlignmentOptions.Left);
        var priceGO = CreateTMP(card.transform, "Price", price, 26, TextAlignmentOptions.Left);
        var descGO = CreateTMP(card.transform, "Description", desc, 22, TextAlignmentOptions.Left);

        descGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.85f);

        // Button row
        var row = CreateUIObject("ButtonRow", card.transform);
        var rowHLG = row.AddComponent<HorizontalLayoutGroup>();
        rowHLG.childAlignment = TextAnchor.MiddleRight;
        rowHLG.spacing = 10;
        rowHLG.childControlWidth = true;
        rowHLG.childControlHeight = true;
        rowHLG.childForceExpandWidth = true;
        rowHLG.childForceExpandHeight = false;

        var rowLE = row.AddComponent<LayoutElement>();
        rowLE.preferredHeight = 60;

        var buyBtn = CreateButtonTMP(row.transform, buyButtonName, "Purchase");
        var buyLE = buyBtn.AddComponent<LayoutElement>();
        buyLE.preferredWidth = 220;
        buyLE.preferredHeight = 56;
    }
}
