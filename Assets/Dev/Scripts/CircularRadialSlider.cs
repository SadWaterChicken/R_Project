using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class CircularRadialSlider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image radialImage; // should be Image.Type = Filled, Fill Method = Radial360
    [SerializeField] private Image borderImage; // optional decorative border
    [SerializeField] private TextMeshProUGUI centerText; // optional numeric display

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 6f;

    [Header("Coloring")]
    [SerializeField] private Gradient fillGradient;

    private float target = 1f;
    private float current = 1f;

    private void Reset()
    {
        radialImage = GetComponent<Image>();
    }

    private void Awake()
    {
        if (radialImage == null)
            radialImage = GetComponent<Image>();

        radialImage.type = Image.Type.Filled;
        radialImage.fillMethod = Image.FillMethod.Radial360;
    }

    private void Update()
    {
        if (Mathf.Approximately(current, target)) return;

        current = Mathf.Lerp(current, target, Time.deltaTime * smoothSpeed);
        ApplyToVisuals(current);
    }

    private void ApplyToVisuals(float normalized)
    {
        if (radialImage != null)
        {
            radialImage.fillAmount = Mathf.Clamp01(normalized);
            if (fillGradient != null)
            {
                radialImage.color = fillGradient.Evaluate(normalized);
            }
        }

        if (centerText != null)
        {
            centerText.text = Mathf.RoundToInt(normalized * 100).ToString();
        }
    }

    /// <summary>
    /// Set the value in normalized 0..1 range. Visual will animate to it.
    /// </summary>
    public void SetNormalizedValue(float normalized)
    {
        target = Mathf.Clamp01(normalized);
        // if instantaneous desired, set current = target and ApplyToVisuals(target)
    }

    /// <summary>
    /// Immediately set the visual without animation
    /// </summary>
    public void SetNormalizedValueImmediate(float normalized)
    {
        target = current = Mathf.Clamp01(normalized);
        ApplyToVisuals(current);
    }

    /// <summary>
    /// Optional: change fill gradient at runtime
    /// </summary>
    public void SetGradient(Gradient g)
    {
        fillGradient = g;
    }
}
