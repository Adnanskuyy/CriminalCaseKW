#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [McpPluginToolType]
    public partial class Tool_VideoPanelSetup
    {
        [McpPluginTool("video-panel-setup", Title = "Video Panel / Setup")]
        [Description("Configures all UGUI elements in the VideoPlayerPanel hierarchy with correct RectTransforms, colors, and properties.")]
        public string Setup()
        {
            return MainThread.Instance.Run(() =>
            {
                var panel = GameObject.Find("VideoPlayerPanel");
                if (panel == null) return "VideoPlayerPanel not found";

                SetupPlayScreen(panel);
                SetupVideoScreen(panel);

                EditorUtility.SetDirty(panel);
                EditorUtils.RepaintAllEditorWindows();

                return "VideoPlayerPanel UGUI setup complete";
            });
        }

        private static Font? GetDefaultFont()
        {
            try
            {
                var fonts = Resources.FindObjectsOfTypeAll<Font>();
                foreach (var f in fonts)
                {
                    if (f != null && f.name == "LegacyRuntime")
                        return f;
                }
                var font = Font.CreateDynamicFontFromOSFont("Arial", 14);
                return font;
            }
            catch
            {
                return null;
            }
        }

        private static RectTransform EnsureRectTransform(Transform t)
        {
            var rt = t as RectTransform;
            if (rt == null)
            {
                // GameObjects under a Canvas must have RectTransform
                // If they only have Transform, we need to add RectTransform
                // which Unity does automatically when parented to a Canvas
                // but for manually created objects it might not have happened
                rt = t.gameObject.AddComponent<RectTransform>();
            }
            return rt;
        }

        private static void SetupPlayScreen(GameObject panel)
        {
            var playScreenT = panel.transform.Find("PlayScreen");
            if (playScreenT == null) return;

            // Temporarily activate to ensure layout works
            playScreenT.gameObject.SetActive(true);

            var psRt = EnsureRectTransform(playScreenT);
            psRt.anchorMin = Vector2.zero;
            psRt.anchorMax = Vector2.one;
            psRt.offsetMin = Vector2.zero;
            psRt.offsetMax = Vector2.zero;

            var psImage = playScreenT.GetComponent<Image>();
            if (psImage != null) psImage.color = new Color(0, 0, 0, 0.85f);

            var vlg = playScreenT.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.childAlignment = TextAnchor.MiddleCenter;
                vlg.childControlWidth = false;
                vlg.childControlHeight = false;
                vlg.spacing = 28;
            }

            var font = GetDefaultFont();
            SetupLabel(playScreenT.Find("TitleLabel"), "Criminal Case 2", 48, new Color(1f, 0.82f, 0.24f), FontStyle.Bold, new Vector2(800, 80), font);
            SetupLabel(playScreenT.Find("SubtitleLabel"), "Klik di bawah untuk memulai investigasi", 28, new Color(0.82f, 0.82f, 0.82f), FontStyle.Normal, new Vector2(800, 60), font);
            SetupButton(playScreenT.Find("PlayButton"), "Putar Intro", 28, FontStyle.Bold, new Color(0.16f, 0.55f, 0.31f), Color.white, new Vector2(260, 72), new Color(0.24f, 0.67f, 0.39f), font);
        }

        private static void SetupVideoScreen(GameObject panel)
        {
            var videoScreenT = panel.transform.Find("VideoScreen");
            if (videoScreenT == null) return;

            // Temporarily activate so we can configure children
            bool wasActive = videoScreenT.gameObject.activeSelf;
            videoScreenT.gameObject.SetActive(true);

            var vsRt = EnsureRectTransform(videoScreenT);
            vsRt.anchorMin = Vector2.zero;
            vsRt.anchorMax = Vector2.one;
            vsRt.offsetMin = Vector2.zero;
            vsRt.offsetMax = Vector2.zero;

            var vsImage = videoScreenT.GetComponent<Image>();
            if (vsImage != null) vsImage.color = Color.black;

            var rawImgT = videoScreenT.Find("VideoRawImage");
            if (rawImgT != null)
            {
                var riRt = EnsureRectTransform(rawImgT);
                riRt.anchorMin = Vector2.zero;
                riRt.anchorMax = Vector2.one;
                riRt.offsetMin = Vector2.zero;
                riRt.offsetMax = Vector2.zero;

                var ri = rawImgT.GetComponent<RawImage>();
                if (ri != null) ri.color = Color.white;
            }

            var containerT = videoScreenT.Find("SkipButtonContainer");
            if (containerT != null)
            {
                var cRt = EnsureRectTransform(containerT);
                cRt.anchorMin = new Vector2(1, 0);
                cRt.anchorMax = new Vector2(1, 0);
                cRt.pivot = new Vector2(1, 0);
                cRt.anchoredPosition = new Vector2(-40, 40);
                cRt.sizeDelta = new Vector2(150, 52);
            }

            var font = GetDefaultFont();
            SetupButton(videoScreenT.Find("SkipButtonContainer/SkipButton"), "Lewati >>", 22, FontStyle.Normal, new Color(0.16f, 0.16f, 0.16f, 0.8f), new Color(0.86f, 0.86f, 0.86f), new Vector2(150, 52), new Color(0.24f, 0.24f, 0.24f, 0.9f), font);

            // Restore inactive state
            if (!wasActive)
                videoScreenT.gameObject.SetActive(false);
        }

        private static void SetupLabel(Transform? t, string text, int fontSize, Color color, FontStyle fontStyle, Vector2 size, Font? font)
        {
            if (t == null) return;
            var rt = EnsureRectTransform(t);
            rt.sizeDelta = size;
            var uiText = t.GetComponent<Text>();
            if (uiText != null)
            {
                uiText.text = text;
                if (font != null) uiText.font = font;
                uiText.fontSize = fontSize;
                uiText.fontStyle = fontStyle;
                uiText.color = color;
                uiText.alignment = TextAnchor.MiddleCenter;
            }
        }

        private static void SetupButton(Transform? t, string text, int fontSize, FontStyle fontStyle, Color bgColor, Color textColor, Vector2 size, Color hoverColor, Font? font)
        {
            if (t == null) return;
            var rt = EnsureRectTransform(t);
            rt.sizeDelta = size;

            var img = t.GetComponent<Image>();
            if (img != null) img.color = bgColor;

            var btn = t.GetComponent<Button>();
            if (btn != null)
            {
                var colors = btn.colors;
                colors.highlightedColor = hoverColor;
                btn.colors = colors;
            }

            var textChild = t.Find("Text");
            if (textChild != null)
            {
                var textRt = EnsureRectTransform(textChild);
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;

                var uiText = textChild.GetComponent<Text>();
                if (uiText != null)
                {
                    uiText.text = text;
                    if (font != null) uiText.font = font;
                    uiText.fontSize = fontSize;
                    uiText.fontStyle = fontStyle;
                    uiText.color = textColor;
                    uiText.alignment = TextAnchor.MiddleCenter;
                }
            }
        }
    }
}