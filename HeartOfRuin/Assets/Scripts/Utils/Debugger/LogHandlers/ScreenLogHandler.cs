using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Utils.DebuggerConfig; // Allows properties to be called as if they belong to this object

public class ScreenLogHandler : BaseLogHandler, IUpdatableHandler, ICanvasHandler 
{
    readonly Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    #region Cached references
    Transform parent;
    Canvas canvas;
    #endregion

    #region Runtime Variables

    List<(Text text, float clearTime)> logEntries = new();
    private RectTransform scrollContent;
    #endregion

    public void InitCanvas(Transform parent)
    {
        this.parent = parent;

        // Create a canvas and set it to be the child of the Debugger GameObject
        // Add canvas with screen space overlay, sort order 1000
        // Add a CanvasScaler component to the canvas and set the UI Scale Mode to Scale With Screen Size to 1080p
        // Add a GraphicRaycaster component to the canvas
        canvas = new GameObject("DebuggerCanvas").AddComponent<Canvas>();
        canvas.transform.SetParent(parent);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler canvasScaler = canvas.gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = ScreenResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;

        canvas.gameObject.AddComponent<GraphicRaycaster>();
    }

    public void OnUpdate()
    {
        ClearEntries();
    }

    private void ClearEntries()
    {
        if (logEntries.Count == 0) return;

        float currentTime = Time.time;

        foreach (var entry in logEntries)
        {
            if (entry.clearTime <= currentTime)
            {
                // clear text first to avoid flicker
                entry.text.text = "";
                // Destroy the text object, then remove from list
                UnityEngine.GameObject.Destroy(entry.text.gameObject);
                
                logEntries.Remove(entry);
                break;
            }
        }
    }

    public override void Log(LogData data)
    {
        if (!EnableScreenLogging) return;
        if (canvas == null) return;

        SetupScreenLogger();

        GameObject textGO = new GameObject("LogEntry", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(scrollContent, false);

        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Text textComp = textGO.GetComponent<Text>();
        textComp.text = data.message?.ToString();
        textComp.font = font;
        textComp.fontSize = FontSize;
        textComp.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComp.verticalOverflow = VerticalWrapMode.Overflow;

        switch (data.logType)
        {
            case LogType.Warning: textComp.color = Color.yellow; break;
            case LogType.Error: textComp.color = Color.red; break;
            case LogType.Exception: textComp.color = new Color(0.8f, 0.2f, 0, 1); break;
            case LogType.Assert: textComp.color = Color.magenta; break;
            default: textComp.color = Color.white; break;
        }

        logEntries.Add((textComp, Time.time + ScreenShowTime));
    }

    private void SetupScreenLogger()
    {
        // Skip if already setup
        if (scrollContent != null) return;

        // Create ScrollRect object
        GameObject scrollGO = new GameObject("LogScrollRect", typeof(RectTransform), typeof(CanvasRenderer));
        scrollGO.transform.SetParent(canvas.transform, false);
        RectTransform scrollRectTransform = scrollGO.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0, 0);
        scrollRectTransform.anchorMax = new Vector2(1, 1);
        scrollRectTransform.offsetMin = new Vector2(10, 10);
        scrollRectTransform.offsetMax = new Vector2(-10, -10);

        ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        // Add viewport
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollGO.transform, false);
        RectTransform viewportRT = viewport.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        scrollRect.viewport = viewportRT;
        viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0.25f); // semi-transparent background
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        // Add content container
        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        scrollContent = content.GetComponent<RectTransform>();
        scrollContent.anchorMin = new Vector2(0, 1);
        scrollContent.anchorMax = new Vector2(1, 1);
        scrollContent.pivot = new Vector2(0.5f, 1);
        scrollContent.offsetMin = Vector2.zero;
        scrollContent.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.spacing = 4f;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = scrollContent;
    }
}
