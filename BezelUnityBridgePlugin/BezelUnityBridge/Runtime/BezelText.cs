using TMPro;
using UnityEngine;
using Bezel.Bridge.Editor.Fonts;
using System.Threading.Tasks;
using System;

namespace Bezel.Bridge
{
    public class BezelText : MonoBehaviour
    {
        [SerializeField]
        private Parameters parameters;

        private float magicScale1 = 0.135f; //Bezel to Unity font rescale

        private const float magicScale2 = 7.4f; //Bezel to Unity font rescale

        public async Task<bool> SetTextParameters(Parameters parameters)
        {
            //Debug.Log("Enter SetTextParameters");

            this.parameters = parameters;

            this.gameObject.AddComponent<MeshRenderer>();

            this.gameObject.AddComponent<TextMeshPro>();

            TextMeshPro text = this.gameObject.GetComponent<TextMeshPro>();

            text.text = parameters.text;
            text.fontSize = parameters.fontSize;
            text.color = convertColorCode(parameters.color);

            // Set to default pivot point
            text.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);

            text.GetComponent<RectTransform>().localScale = new Vector3(
                -magicScale1, magicScale1, magicScale1);

            text.GetComponent<RectTransform>().sizeDelta = getMaxHeightWidth(parameters);

            //letterSpacing
            text.characterSpacing = parameters.letterSpacing;

            text.horizontalAlignment = parameters.textAlign switch
            {
                "left" => HorizontalAlignmentOptions.Left,
                "center" => HorizontalAlignmentOptions.Center,
                "justify" => HorizontalAlignmentOptions.Justified,
                "right" => HorizontalAlignmentOptions.Right,
                _ => HorizontalAlignmentOptions.Left
            };

            text.verticalAlignment = parameters.verticalAlign switch
            {
                "top" => VerticalAlignmentOptions.Top,
                "middle" => VerticalAlignmentOptions.Middle,
                "bottom" => VerticalAlignmentOptions.Bottom,
                _ => VerticalAlignmentOptions.Top,
            };

            text.fontStyle |= parameters.letterCase switch
            {
                "as-typed" => 0,
                "uppercase" => FontStyles.UpperCase,
                "lowercase" => FontStyles.LowerCase,
                "title-case" => FontStyles.SmallCaps,
                _ => 0
            };

            // Generate font map
            BezelFontMap fontMap = await GenerateFontMap(parameters.fontFamily, parameters.fontWeight);

            int fontWeightInt = FontManager.FontWeightStringToInt(parameters.fontWeight);

            // Get font map
            BezelFontMapEntry matchingFontMapping = fontMap.GetFontMapping(parameters.fontFamily, fontWeightInt);

            try
            {
                text.font = matchingFontMapping.FontAsset;
            }
            catch (NullReferenceException)
            {

                // Handle exception
                Debug.LogWarning("Ignoring NullRef due to TMP error");
            }

            var effectMaterialPreset = GetEffectMaterialPreset(matchingFontMapping);
            text.fontMaterial = effectMaterialPreset;

            return true;
        }

        private async Task<BezelFontMap> GenerateFontMap(string fontFamily, string fontWeight)
        {
            int fontWeightInt = FontManager.FontWeightStringToInt(fontWeight);

            BezelFontMap fontMap = await FontManager.GenerateFontMapForDocument(fontFamily, fontWeightInt, true);

            return fontMap;
        }


        private Material GetEffectMaterialPreset(BezelFontMapEntry matchingFontMapping)
        {

            var hasShadowEffect = false;
            var shadowColor = UnityEngine.Color.black; // Not in used
            var shadowDistance = Vector2.zero; // Not in used
            var hasOutlineColor = false;
            var outlineColor = UnityEngine.Color.black; // Not in used
            var outlineWidth = 0f; // Not in used
            return FontManager.GetEffectMaterialPreset(matchingFontMapping,
                        hasShadowEffect, shadowColor, shadowDistance, hasOutlineColor, outlineColor, outlineWidth);
        }

        private Vector2 getMaxHeightWidth(Parameters p)
        {
            float maxWidth = 1.0f;
            float maxHeight = 1.0f;
            float parsed;
            if (p.width != null && float.TryParse(p.width.ToString(), out parsed))
            {
                maxWidth = parsed;
            }
            if (p.depth != null && float.TryParse(p.depth.ToString(), out parsed))
            {
                maxHeight = parsed;
            }

            return new Vector2(
                maxWidth * magicScale2, maxHeight * magicScale2);

        }

        private Color convertColorCode(string colorCode)
        {
            Color color = Color.white;
            if (!ColorUtility.TryParseHtmlString(colorCode, out color))
            {
                color = Color.white;
            }

            return color;
        }
    }
}