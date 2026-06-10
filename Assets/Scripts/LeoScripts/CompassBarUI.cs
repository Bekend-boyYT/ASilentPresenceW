using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    [RequireComponent(typeof(RectTransform))]
    public class CompassBarUI : MonoBehaviour
    {
        [Header("Player / Runtime")]
        [Tooltip("Optional transform that drives the compass heading. If empty, the player controller transform is used.")]
        public Transform PlayerTransform;

        [Tooltip("Optional FirstPersonController to auto-find the player transform.")]
        public FirstPersonController PlayerController;

        [Tooltip("Compass bar settings asset used for styling and smoothing.")]
        public CompassBarSettings Settings;

        [Header("UI References")]
        [Tooltip("The root rect transform for the compass bar. If not assigned, the component's rect transform is used.")]
        public RectTransform CompassRoot;

        [Header("Layout")]
        [Tooltip("Anchor position for the compass bar inside the canvas.")]
        public CompassAnchor Anchor = CompassAnchor.TopCenter;

        [Tooltip("Vertical offset from the anchor position.")]
        public float AnchorVerticalOffset = 32f;

        [Tooltip("Horizontal offset from the anchor position.")]
        public float AnchorHorizontalOffset = 0f;

        [Tooltip("Additional offset from the selected anchor position.")]
        public Vector2 AnchorOffset = Vector2.zero;

        [Tooltip("Manual position offset from the computed anchor position.")]
        public Vector2 ManualPositionOffset = Vector2.zero;

        private const int MajorTickCount = 8;
        private const int MinorTicksPerMajor = 3;
        private const int TotalTickCount = MajorTickCount * (MinorTicksPerMajor + 1);
        private const float MajorTickInterval = 45f;
        private const float MinorTickInterval = 11.25f;

        private RectTransform _rootTransform;
        private RectTransform _lineRect;
        private Image _lineImage;
        private Image _backgroundImage;
        private Image _centerMarkerImage;
        private RectTransform _tickContainer;
        private RectTransform _labelContainer;

        private Image[] _tickImages = Array.Empty<Image>();
        private TextMeshProUGUI[] _directionLabels = Array.Empty<TextMeshProUGUI>();
        private float[] _tickAngles = Array.Empty<float>();

        private float _currentHeading;
        private float _headingVelocity;
        private float _currentRootWidth;
        private TMP_FontAsset _labelFont;

        private static readonly string[] DefaultDirectionLabels = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        public enum CompassAnchor
        {
            TopCenter,
            Center,
            BottomCenter
        }

        private void Reset()
        {
            CompassRoot = GetComponent<RectTransform>();
        }

        private void Awake()
        {
            if (CompassRoot == null)
                CompassRoot = GetComponent<RectTransform>();

            CompassRoot = EnsureCompassRoot(CompassRoot);
            _rootTransform = CompassRoot;
            Settings ??= CompassBarSettings.Default;
            _labelFont = Settings.LabelFont ?? TMP_Settings.defaultFontAsset;
            InitializeCompassUI();
            InitializeTicksAndLabels();
        }

        private RectTransform EnsureCompassRoot(RectTransform assignedRoot)
        {
            if (assignedRoot == null)
                return null;

            if (assignedRoot.parent == null)
                return CreateCompassRootContainer();

            return assignedRoot;
        }

        private RectTransform CreateCompassRootContainer()
        {
            GameObject rootObject = new GameObject("Compass Root", typeof(RectTransform));
            rootObject.transform.SetParent(transform, false);
            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = new Vector2(0f, -AnchorVerticalOffset);
            rootRect.sizeDelta = Vector2.zero;
            return rootRect;
        }

        private void Start()
        {
            if (PlayerTransform == null && PlayerController != null)
                PlayerTransform = PlayerController.transform;

            if (PlayerTransform == null)
            {
                Debug.LogWarning("CompassBarUI: No player transform assigned.");
                enabled = false;
                return;
            }

            _currentHeading = PlayerTransform.eulerAngles.y;
            UpdateBarLayout(true);
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

            if (PlayerTransform == null)
            {
                if (PlayerController != null)
                    PlayerTransform = PlayerController.transform;

                if (PlayerTransform == null)
                    return;
            }

            if (Settings == null)
                Settings = CompassBarSettings.Default;

            float targetHeading = PlayerTransform.eulerAngles.y;
            _currentHeading = Mathf.SmoothDampAngle(_currentHeading, targetHeading, ref _headingVelocity, Settings.RotationSmoothTime, Settings.MaxRotationSpeed, Time.deltaTime);

            UpdateBarLayout(false);
            RefreshTickPositions();
            RefreshLabelPositions();
            RefreshCenterMarker();
        }

        private void InitializeCompassUI()
        {
            if (_rootTransform == null)
                return;

            _rootTransform.localScale = Vector3.one;

            _backgroundImage = CreateChildImage("Compass Background", _rootTransform);
            _backgroundImage.raycastTarget = false;
            _backgroundImage.color = Settings.BackgroundColor;

            _lineImage = CreateChildImage("Compass Line", _rootTransform);
            _lineImage.raycastTarget = false;
            _lineImage.color = Settings.LineColor;

            _centerMarkerImage = CreateChildImage("Compass Center Marker", _rootTransform);
            _centerMarkerImage.raycastTarget = false;
            _centerMarkerImage.color = Settings.CenterMarkerColor;

            _tickContainer = CreateChildContainer("Compass Tick Container", _rootTransform);
            _labelContainer = CreateChildContainer("Compass Label Container", _rootTransform);
        }

        private void InitializeTicksAndLabels()
        {
            if (_tickContainer == null || _labelContainer == null)
                return;

            _tickImages = new Image[TotalTickCount];
            _tickAngles = new float[TotalTickCount];

            for (int i = 0; i < TotalTickCount; i++)
            {
                _tickAngles[i] = i * MinorTickInterval;
                Image tick = CreateChildImage($"Tick {i}", _tickContainer);
                tick.raycastTarget = false;
                _tickImages[i] = tick;
            }

            _directionLabels = new TextMeshProUGUI[MajorTickCount];
            string[] labels = Settings.DirectionLabels != null && Settings.DirectionLabels.Length >= MajorTickCount
                ? Settings.DirectionLabels
                : DefaultDirectionLabels;

            for (int i = 0; i < MajorTickCount; i++)
            {
                TextMeshProUGUI label = CreateChildText($"Label {labels[i]}", _labelContainer);
                label.raycastTarget = false;
                label.text = labels[i];
                label.fontSize = Settings.LabelFontSize;
                label.font = _labelFont;
                label.alignment = TextAlignmentOptions.Center;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                _directionLabels[i] = label;
            }
        }

        private void UpdateBarLayout(bool force)
        {
            if (_rootTransform == null)
                return;

            float rootWidth = Mathf.Max(_rootTransform.rect.width, Screen.width * 0.35f);
            bool widthChanged = !Mathf.Approximately(rootWidth, _currentRootWidth);
            
            if (widthChanged)
                _currentRootWidth = rootWidth;

            float targetWidth = rootWidth * Settings.WidthScale * Settings.Scale;
            float targetHeight = Settings.LineThickness + Settings.BarPaddingVertical * 2f;
            float targetHeightWithLabels = targetHeight + Settings.LabelVerticalOffset * 2f + Settings.CenterMarkerHeight + Settings.BarPaddingVertical;

            if (widthChanged || force)
            {
                if (_rootTransform != null)
                    _rootTransform.sizeDelta = new Vector2(targetWidth + Settings.BarPaddingHorizontal, targetHeightWithLabels);

                if (_backgroundImage != null)
                {
                    RectTransform backgroundRect = _backgroundImage.rectTransform;
                    backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
                    backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
                    backgroundRect.pivot = new Vector2(0.5f, 0.5f);
                    backgroundRect.anchoredPosition = Vector2.zero;
                    backgroundRect.sizeDelta = new Vector2(targetWidth + Settings.BarPaddingHorizontal, targetHeight);
                    _backgroundImage.color = Settings.BackgroundColor * Settings.Opacity;
                    _backgroundImage.gameObject.SetActive(Settings.ShowBackground);
                }
            }

            SetRootAnchor();

            if (widthChanged || force)
            {
                if (_lineImage != null)
                {
                    RectTransform lineRect = _lineImage.rectTransform;
                    lineRect.anchorMin = new Vector2(0.5f, 0.5f);
                    lineRect.anchorMax = new Vector2(0.5f, 0.5f);
                    lineRect.pivot = new Vector2(0.5f, 0.5f);
                    lineRect.anchoredPosition = Vector2.zero;
                    lineRect.sizeDelta = new Vector2(targetWidth, Settings.LineThickness);
                    _lineImage.color = Settings.LineColor * Settings.Opacity;
                    _lineImage.type = Image.Type.Sliced;
                    _lineRect = lineRect;
                }

                if (_centerMarkerImage != null)
                {
                    RectTransform centerRect = _centerMarkerImage.rectTransform;
                    centerRect.anchorMin = new Vector2(0.5f, 0.5f);
                    centerRect.anchorMax = new Vector2(0.5f, 0.5f);
                    centerRect.pivot = new Vector2(0.5f, 0f);
                    centerRect.sizeDelta = new Vector2(Settings.CenterMarkerWidth, Settings.CenterMarkerHeight);
                    _centerMarkerImage.color = Settings.CenterMarkerColor * Settings.Opacity;
                }

                if (_tickContainer != null)
                {
                    RectTransform tickRect = _tickContainer;
                    tickRect.anchorMin = new Vector2(0.5f, 0.5f);
                    tickRect.anchorMax = new Vector2(0.5f, 0.5f);
                    tickRect.pivot = new Vector2(0.5f, 0.5f);
                    tickRect.anchoredPosition = Vector2.zero;
                    tickRect.sizeDelta = new Vector2(targetWidth, targetHeight);
                }

                if (_labelContainer != null)
                {
                    RectTransform labelRect = _labelContainer;
                    labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                    labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                    labelRect.pivot = new Vector2(0.5f, 0.5f);
                    labelRect.anchoredPosition = Vector2.zero;
                    labelRect.sizeDelta = new Vector2(targetWidth, targetHeight + Settings.LabelVerticalOffset * 2f);
                }

                for (int i = 0; i < _directionLabels.Length; i++)
                {
                    _directionLabels[i].fontSize = Settings.LabelFontSize;
                    _directionLabels[i].font = _labelFont;
                }
            }
        }

        private void SetRootAnchor()
        {
            if (_rootTransform == null)
                return;

            switch (Anchor)
            {
                case CompassAnchor.TopCenter:
                    _rootTransform.anchorMin = new Vector2(0.5f, 1f);
                    _rootTransform.anchorMax = new Vector2(0.5f, 1f);
                    _rootTransform.pivot = new Vector2(0.5f, 1f);
                    break;
                case CompassAnchor.BottomCenter:
                    _rootTransform.anchorMin = new Vector2(0.5f, 0f);
                    _rootTransform.anchorMax = new Vector2(0.5f, 0f);
                    _rootTransform.pivot = new Vector2(0.5f, 0f);
                    break;
                default:
                    _rootTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    _rootTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    _rootTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }

            float xOffset = AnchorHorizontalOffset + AnchorOffset.x + ManualPositionOffset.x;
            float yOffset = AnchorVerticalOffset + AnchorOffset.y + ManualPositionOffset.y;

            if (Anchor == CompassAnchor.TopCenter)
                yOffset = -yOffset;

            _rootTransform.anchoredPosition = new Vector2(xOffset, yOffset);
        }

        private void RefreshTickPositions()
        {
            if (_tickImages == null || _tickImages.Length == 0)
                return;

            float visibleWidth = _currentRootWidth * Settings.WidthScale * Settings.Scale;
            float halfWidth = visibleWidth * 0.5f;
            float pixelsPerDegree = visibleWidth / Mathf.Max(Settings.VisibleDegrees, 1f);

            for (int i = 0; i < _tickImages.Length; i++)
            {
                bool isMajor = i % (MinorTicksPerMajor + 1) == 0;
                Image tick = _tickImages[i];
                float tickAngle = _tickAngles[i];
                float delta = Mathf.DeltaAngle(_currentHeading, tickAngle);
                float x = delta * pixelsPerDegree;
                bool visible = Mathf.Abs(x) <= halfWidth + 32f;

                tick.enabled = visible;
                if (!visible)
                    continue;

                RectTransform tickRect = tick.rectTransform;
                tickRect.anchorMin = new Vector2(0.5f, 0.5f);
                tickRect.anchorMax = new Vector2(0.5f, 0.5f);
                tickRect.pivot = new Vector2(0.5f, 0f);
                tickRect.anchoredPosition = new Vector2(x, Settings.TickVerticalOffset);
                tickRect.sizeDelta = new Vector2(Settings.TickThickness, isMajor ? Settings.MajorTickHeight : Settings.MinorTickHeight);

                float distanceRatio = 1f - Mathf.Clamp01(Mathf.Abs(delta) / Settings.LabelFadeDistance);
                float alpha = Mathf.Pow(distanceRatio, Settings.LabelFadePower) * Settings.Opacity;
                tick.color = new Color(Settings.TickColor.r, Settings.TickColor.g, Settings.TickColor.b, Settings.TickColor.a * Mathf.Lerp(0.45f, 1f, alpha) * Settings.Opacity);
            }
        }

        private void RefreshLabelPositions()
        {
            if (_directionLabels == null || _directionLabels.Length == 0)
                return;

            float visibleWidth = _currentRootWidth * Settings.WidthScale * Settings.Scale;
            float halfWidth = visibleWidth * 0.5f;
            float pixelsPerDegree = visibleWidth / Mathf.Max(Settings.VisibleDegrees, 1f);

            for (int i = 0; i < _directionLabels.Length; i++)
            {
                TextMeshProUGUI label = _directionLabels[i];
                float labelAngle = i * MajorTickInterval;
                float delta = Mathf.DeltaAngle(_currentHeading, labelAngle);
                float x = delta * pixelsPerDegree;
                bool visible = Mathf.Abs(x) <= halfWidth + 64f;

                label.gameObject.SetActive(visible);
                if (!visible)
                    continue;

                float distanceRatio = 1f - Mathf.Clamp01(Mathf.Abs(delta) / Settings.LabelFadeDistance);
                float fade = Mathf.Pow(distanceRatio, Settings.LabelFadePower);
                float alpha = fade * Settings.Opacity;
                float zoom = 1f + (Settings.LabelZoom * fade);

                RectTransform labelRect = label.rectTransform;
                labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = new Vector2(x, Settings.LabelVerticalOffset);
                labelRect.localScale = new Vector3(zoom, zoom, 1f);

                label.color = new Color(Settings.LabelColor.r, Settings.LabelColor.g, Settings.LabelColor.b, Mathf.Clamp01(Settings.LabelColor.a * alpha));
            }
        }

        private void RefreshCenterMarker()
        {
            if (_centerMarkerImage == null || !Settings.ShowCenterMarker)
            {
                if (_centerMarkerImage != null)
                    _centerMarkerImage.gameObject.SetActive(false);
                return;
            }

            _centerMarkerImage.gameObject.SetActive(true);
            RectTransform centerRect = _centerMarkerImage.rectTransform;
            centerRect.anchoredPosition = new Vector2(0f, Settings.CenterMarkerVerticalOffset);
            centerRect.sizeDelta = new Vector2(Settings.CenterMarkerWidth, Settings.CenterMarkerHeight);
            _centerMarkerImage.color = Settings.CenterMarkerColor * Settings.Opacity;
        }

        private Image CreateChildImage(string name, RectTransform parent)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(parent, false);
            Image image = child.GetComponent<Image>();
            image.material = null;
            return image;
        }

        private RectTransform CreateChildContainer(string name, RectTransform parent)
        {
            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            return rect;
        }

        private TextMeshProUGUI CreateChildText(string name, RectTransform parent)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            child.transform.SetParent(parent, false);
            TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
            text.font = _labelFont;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            text.color = Settings.LabelColor;
            return text;
        }
    }
}
