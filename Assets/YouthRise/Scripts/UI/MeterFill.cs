using UnityEngine;
using UnityEngine.UI;

namespace YouthRise
{
    public static class MeterFill
    {
        public static void SetValue(Image fill, float normalizedValue)
        {
            // Sprite-less Images ignore Image.fillAmount. Size the solid bar instead.
            fill.type = Image.Type.Simple;
            Vector2 anchor = fill.rectTransform.anchorMax;
            anchor.x = Mathf.Clamp01(normalizedValue);
            fill.rectTransform.anchorMax = anchor;
        }
    }
}
