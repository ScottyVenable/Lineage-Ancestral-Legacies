using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Lineage.Ancestral.Legacies.Debug
{
    /// <summary>
    /// In-game debug console. Toggle with F2. Supports resizing, markdown-like formatting, and colored text.
    /// </summary>
    public class DebugConsoleManager : MonoBehaviour
    {
        private Canvas consoleCanvas;
        private RectTransform window;
        private RectTransform contentArea;
        private TextMeshProUGUI outputText;
        private TMP_InputField inputField;
        private ScrollRect scrollRect;
        private Button minimizeButton;
        private RectTransform resizeHandle;
        private bool isOpen = false;
        private bool isMinimized = false;
        private Vector2 dragOffset;
        private Vector2 resizeStart;
        private Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();

        [RuntimeInitializeOnLoadMethod]
        static void InitConsole()
        {
            var go = new GameObject("DebugConsoleManager");
            DontDestroyOnLoad(go);
            go.AddComponent<DebugConsoleManager>();
        }

        void Awake()
        {
            CreateUI();
            RegisterCommands();
            window.gameObject.SetActive(isOpen);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                isOpen = !isOpen;
                window.gameObject.SetActive(isOpen);
                if (isOpen) inputField.ActivateInputField();
            }
            if (isOpen && Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(inputField.text))
            {
                var line = inputField.text;
                inputField.text = string.Empty;
                AppendLine("> " + line);
                ProcessCommand(line);
            }
        }

        void CreateUI()
        {
            // create canvas
            GameObject cgo = new GameObject("ConsoleCanvas");
            consoleCanvas = cgo.AddComponent<Canvas>();
            consoleCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(cgo);

            // window
            GameObject wgo = new GameObject("ConsoleWindow");
            wgo.transform.SetParent(cgo.transform);
            window = wgo.AddComponent<RectTransform>();
            window.sizeDelta = new Vector2(600, 300);
            window.anchorMin = new Vector2(0.05f, 0.05f);
            window.anchorMax = new Vector2(0.95f, 0.5f);
            window.pivot = new Vector2(0, 1);
            var img = wgo.AddComponent<Image>(); img.color = new Color(0,0,0,0.9f);

            // header for drag + minimize
            GameObject header = new GameObject("Header");
            header.transform.SetParent(wgo.transform);
            var hrt = header.AddComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0, 1); hrt.anchorMax = new Vector2(1, 1);
            hrt.sizeDelta = new Vector2(0, 30); hrt.pivot = new Vector2(0,1);
            var himg = header.AddComponent<Image>(); himg.color = new Color(0.1f,0.1f,0.1f,1);
            // drag events
            var trig = header.AddComponent<EventTrigger>();
            AddEvent(trig, EventTriggerType.BeginDrag, e => OnBeginDrag((PointerEventData)e));
            AddEvent(trig, EventTriggerType.Drag, e => OnDrag((PointerEventData)e));
            // minimize button
            GameObject mb = new GameObject("Minimize"); mb.transform.SetParent(header.transform);
            var mbrt = mb.AddComponent<RectTransform>();
            mbrt.anchorMin = new Vector2(1,0); mbrt.anchorMax = new Vector2(1,1);
            mbrt.sizeDelta = new Vector2(30,30); mbrt.pivot = new Vector2(1,0.5f);
            minimizeButton = mb.AddComponent<Button>();
            var mbimg = mb.AddComponent<Image>(); mbimg.color = Color.gray;
            minimizeButton.onClick.AddListener(ToggleMinimize);

            // scroll area for output
            GameObject sg = new GameObject("ScrollArea"); sg.transform.SetParent(wgo.transform);
            contentArea = sg.AddComponent<RectTransform>();
            contentArea.anchorMin = new Vector2(0,0.2f); contentArea.anchorMax = new Vector2(1,1);
            contentArea.offsetMin = new Vector2(10,10); contentArea.offsetMax = new Vector2(-10,-40);
            scrollRect = sg.AddComponent<ScrollRect>(); scrollRect.horizontal = false;
            var sback = sg.AddComponent<Image>(); sback.color = Color.clear;
            // viewport
            GameObject vp = new GameObject("Viewport"); vp.transform.SetParent(sg.transform);
            var vprt = vp.AddComponent<RectTransform>(); vprt.anchorMin=Vector2.zero; vprt.anchorMax=Vector2.one; vprt.offsetMin=vprt.offsetMax=Vector2.zero;
            var mask = vp.AddComponent<Mask>(); mask.showMaskGraphic=false;
            var vpimg = vp.AddComponent<Image>(); vpimg.color = Color.clear;
            scrollRect.viewport = vprt;
            // text content
            GameObject ct = new GameObject("Content"); ct.transform.SetParent(vp.transform);
            var ctrt = ct.AddComponent<RectTransform>(); ctrt.anchorMin=Vector2.zero; ctrt.anchorMax=Vector2.one; ctrt.offsetMin=ctrt.offsetMax=Vector2.zero;
            outputText = ct.AddComponent<TextMeshProUGUI>();
            outputText.color = Color.white;
            outputText.fontSize = 14;
            // Use legacy enableWordWrapping (obsolete) with warning suppression
            #pragma warning disable 0618
            outputText.enableWordWrapping = true;
            #pragma warning restore 0618
            outputText.alignment = TextAlignmentOptions.TopLeft;
            scrollRect.content = ctrt;

            // input field
            GameObject ig = new GameObject("InputField"); ig.transform.SetParent(wgo.transform);
            var irt = ig.AddComponent<RectTransform>(); irt.anchorMin=new Vector2(0,0); irt.anchorMax=new Vector2(1,0); irt.sizeDelta=new Vector2(0,30); irt.pivot=new Vector2(0,0);
            inputField = ig.AddComponent<TMP_InputField>(); var tf = ig.AddComponent<TextMeshProUGUI>(); tf.color=Color.white; tf.fontSize=14;
            inputField.textComponent = tf;
            var ph = ig.AddComponent<TextMeshProUGUI>(); ph.text="Enter command..."; ph.color=Color.gray; inputField.placeholder=ph;

            // resize handle
            GameObject rh = new GameObject("Resize"); rh.transform.SetParent(wgo.transform);
            resizeHandle = rh.AddComponent<RectTransform>();
            resizeHandle.anchorMin=resizeHandle.anchorMax=new Vector2(1,0);
            resizeHandle.sizeDelta=new Vector2(20,20); resizeHandle.anchoredPosition=new Vector2(-10,10);
            var rhimg = rh.AddComponent<Image>(); rhimg.color=Color.gray;
            var rtrig = rh.AddComponent<EventTrigger>();
            AddEvent(rtrig, EventTriggerType.BeginDrag, e => OnBeginResize((PointerEventData)e));
            AddEvent(rtrig, EventTriggerType.Drag, e => OnDragResize((PointerEventData)e));
        }

        void AddEvent(EventTrigger trg, EventTriggerType t, Action<BaseEventData> cb) { var e=new EventTrigger.Entry{eventID=t}; e.callback.AddListener(cb.Invoke); trg.triggers.Add(e); }

        void OnBeginDrag(PointerEventData e) { RectTransformUtility.ScreenPointToLocalPointInRectangle(window,e.position,null, out dragOffset); }
        void OnDrag(PointerEventData e) { Vector2 lp; RectTransformUtility.ScreenPointToLocalPointInRectangle(consoleCanvas.transform as RectTransform,e.position,null, out lp); window.anchoredPosition = lp - dragOffset; }
        void OnBeginResize(PointerEventData e){ resizeStart=window.sizeDelta; }
        void OnDragResize(PointerEventData e){ var s=resizeStart + new Vector2(e.delta.x,-e.delta.y); window.sizeDelta=new Vector2(Mathf.Max(200,s.x),Mathf.Max(100,s.y)); }

        void ToggleMinimize(){ isMinimized=!isMinimized; contentArea.gameObject.SetActive(!isMinimized); inputField.gameObject.SetActive(!isMinimized); }

        void AppendLine(string line){ outputText.text += ParseMarkdown(line)+"\n"; Canvas.ForceUpdateCanvases(); scrollRect.verticalNormalizedPosition=0; }
        string ParseMarkdown(string s)
        {
            // Bold: **text**
            s = Regex.Replace(s, @"\*\*(.*?)\*\*", "<b>$1</b>");
            // Italic: *text*
            s = Regex.Replace(s, @"\*(.*?)\*", "<i>$1</i>");
            return s;
        }

        void RegisterCommands()
        {
            commands.Clear();
            commands["help"] = args => { foreach(var k in commands.Keys) AppendLine(k); };
            commands["echo"] = args => AppendLine(string.Join(" ", args));
            commands["clear"] = args => outputText.text = string.Empty;
            // add more here
        }

        void ProcessCommand(string line)
        {
            var parts=line.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            var cmd=parts[0].ToLower();
            var args=parts.Length>1? parts[1..] : new string[0];
            if(commands.ContainsKey(cmd)) commands[cmd].Invoke(args);
            else AppendLine($"<color=red>Error:</color> Unknown command '{cmd}'");
        }
    }
}
