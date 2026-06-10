using TMPro;
using UnityEngine;

namespace StarterAssets
{
    [CreateAssetMenu(menuName = "StarterAssets/UI/Compass Bar Settings", fileName = "CompassBarSettings")]
    public class CompassBarSettings : ScriptableObject
    {
        [Header("General")]
        public bool EnableCompass = true;
        [Range(0.4f, 1.0f)] public float WidthScale = 0.94f;
        [Range(0.3f, 1.6f)] public float Scale = 1.0f;
        [Range(0.2f, 1.0f)] public float Opacity = 0.95f;
        [Tooltip("How many world degrees are visible across the compass bar.")]
        [Range(90f, 360f)] public float VisibleDegrees = 160f;
        [Tooltip("How fast the compass interpolates to player heading.")]
        [Range(0.01f, 0.8f)] public float RotationSmoothTime = 0.16f;
        [Tooltip("Maximum angular velocity used by the smoothing system.")]
        public float MaxRotationSpeed = 1080f;
        [Tooltip("How quickly direction labels fade out when they move away from center.")]
        [Range(30f, 120f)] public float LabelFadeDistance = 90f;
        [Range(0.5f, 3.0f)] public float LabelFadePower = 1.9f;
        [Tooltip("How much labels scale when they are near the center marker.")]
        [Range(0f, 0.5f)] public float LabelZoom = 0.18f;

        [Header("Colors")]
        public Color LineColor = new Color(1f, 1f, 1f, 0.92f);
        public Color TickColor = new Color(1f, 1f, 1f, 0.92f);
        public Color LabelColor = new Color(1f, 1f, 1f, 0.94f);
        public Color BackgroundColor = new Color(0f, 0f, 0f, 0.12f);
        public Color CenterMarkerColor = new Color(1f, 1f, 1f, 0.98f);

        [Header("Line & Bar")]
        [Range(1f, 5f)] public float LineThickness = 2f;
        [Range(0f, 48f)] public float BarPaddingVertical = 14f;
        [Range(0f, 64f)] public float BarPaddingHorizontal = 24f;
        public bool ShowBackground = true;

        [Header("Tick Marks")]
        [Range(1f, 4f)] public float TickThickness = 1.75f;
        [Range(10f, 30f)] public float MajorTickHeight = 18f;
        [Range(5f, 16f)] public float MinorTickHeight = 10f;
        [Tooltip("Distance in pixels from the compass line where tick marks are drawn.")]
        public float TickVerticalOffset = 0f;
        public bool ShowMinorTicks = true;

        [Header("Labels")]
        public TMP_FontAsset LabelFont;
        [Range(10f, 34f)] public float LabelFontSize = 18f;
        [Range(0f, 60f)] public float LabelVerticalOffset = 24f;
        [Tooltip("How far the label text sits from the center axis.")]
        [Range(0f, 40f)] public float LabelDistance = 24f;
        public string[] DirectionLabels = new string[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        [Header("Center Marker")]
        public bool ShowCenterMarker = true;
        [Range(1f, 6f)] public float CenterMarkerWidth = 2f;
        [Range(14f, 36f)] public float CenterMarkerHeight = 24f;
        [Range(0f, 16f)] public float CenterMarkerVerticalOffset = 6f;

        private static CompassBarSettings _defaultSettings;

        public static CompassBarSettings Default
        {
            get
            {
                if (_defaultSettings == null)
                {
                    _defaultSettings = CreateInstance<CompassBarSettings>();
                    _defaultSettings.EnableCompass = true;
                    _defaultSettings.WidthScale = 0.94f;
                    _defaultSettings.Scale = 1f;
                    _defaultSettings.Opacity = 0.95f;
                    _defaultSettings.VisibleDegrees = 160f;
                    _defaultSettings.RotationSmoothTime = 0.16f;
                    _defaultSettings.MaxRotationSpeed = 1080f;
                    _defaultSettings.LabelFadeDistance = 90f;
                    _defaultSettings.LabelFadePower = 1.9f;
                    _defaultSettings.LabelZoom = 0.18f;
                    _defaultSettings.LineColor = new Color(1f, 1f, 1f, 0.92f);
                    _defaultSettings.TickColor = new Color(1f, 1f, 1f, 0.92f);
                    _defaultSettings.LabelColor = new Color(1f, 1f, 1f, 0.94f);
                    _defaultSettings.BackgroundColor = new Color(0f, 0f, 0f, 0.12f);
                    _defaultSettings.CenterMarkerColor = new Color(1f, 1f, 1f, 0.98f);
                    _defaultSettings.LineThickness = 2f;
                    _defaultSettings.BarPaddingVertical = 14f;
                    _defaultSettings.BarPaddingHorizontal = 24f;
                    _defaultSettings.ShowBackground = true;
                    _defaultSettings.TickThickness = 1.75f;
                    _defaultSettings.MajorTickHeight = 18f;
                    _defaultSettings.MinorTickHeight = 10f;
                    _defaultSettings.TickVerticalOffset = 0f;
                    _defaultSettings.ShowMinorTicks = true;
                    _defaultSettings.LabelFontSize = 18f;
                    _defaultSettings.LabelVerticalOffset = 24f;
                    _defaultSettings.LabelDistance = 24f;
                    _defaultSettings.DirectionLabels = new string[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
                    _defaultSettings.ShowCenterMarker = true;
                    _defaultSettings.CenterMarkerWidth = 2f;
                    _defaultSettings.CenterMarkerHeight = 24f;
                    _defaultSettings.CenterMarkerVerticalOffset = 6f;
                }

                return _defaultSettings;
            }
        }
    }
}
