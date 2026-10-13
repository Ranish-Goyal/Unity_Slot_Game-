using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachineGame
{
    public sealed class ReelView : MonoBehaviour
    {
        private const int VisibleSymbolCount = 5;

        private readonly Image[] symbolImages = new Image[VisibleSymbolCount];
        private Sprite[] symbols;
        private RectTransform strip;
        private float symbolHeight;
        private float stripOffset;
        private Sprite[] spinningSprites;

        public void Initialize(Sprite[] availableSymbols)
        {
            symbols = availableSymbols;

            RectTransform viewport = (RectTransform)transform;
            Image window = gameObject.AddComponent<Image>();
            window.color = new Color(0.16f, 0.12f, 0.34f, 0.12f);
            window.raycastTarget = false;
            gameObject.AddComponent<RectMask2D>();

            GameObject stripObject = new GameObject("Spinning symbols", typeof(RectTransform));
            stripObject.transform.SetParent(transform, false);
            strip = stripObject.GetComponent<RectTransform>();
            strip.anchorMin = new Vector2(0.5f, 0.5f);
            strip.anchorMax = new Vector2(0.5f, 0.5f);
            strip.pivot = new Vector2(0.5f, 0.5f);
            strip.sizeDelta = viewport.sizeDelta;

            symbolHeight = viewport.rect.height * 0.54f;
            spinningSprites = new Sprite[VisibleSymbolCount];

            for (int i = 0; i < symbolImages.Length; i++)
            {
                GameObject symbolObject = new GameObject("Symbol " + (i + 1), typeof(RectTransform), typeof(Image));
                symbolObject.transform.SetParent(strip, false);

                RectTransform symbolRect = symbolObject.GetComponent<RectTransform>();
                symbolRect.anchorMin = new Vector2(0.5f, 0.5f);
                symbolRect.anchorMax = new Vector2(0.5f, 0.5f);
                symbolRect.pivot = new Vector2(0.5f, 0.5f);
                symbolRect.sizeDelta = new Vector2(viewport.rect.width * 0.96f, symbolHeight);
                symbolRect.anchoredPosition = new Vector2(0f, (2 - i) * symbolHeight);

                symbolImages[i] = symbolObject.GetComponent<Image>();
                symbolImages[i].preserveAspect = true;
                symbolImages[i].raycastTarget = false;
                symbolImages[i].color = i == 2 ? Color.white : new Color(1f, 1f, 1f, 0.52f);
                SetRandomSprite(i);
            }
        }

        public IEnumerator SpinTo(Sprite result, float duration)
        {
            float cycles = Mathf.Max(8f, Mathf.Ceil(1200f * duration / symbolHeight));
            float startingOffset = stripOffset;
            float totalDistance = cycles * symbolHeight - startingOffset;
            float elapsed = 0f;
            float previousEase = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float easedProgress = progress * progress * (3f - 2f * progress);
                Scroll(totalDistance * (easedProgress - previousEase));
                previousEase = easedProgress;
                yield return null;
            }

            if (previousEase < 1f)
            {
                Scroll(totalDistance * (1f - previousEase));
            }

            stripOffset = 0f;
            strip.anchoredPosition = Vector2.zero;
            symbolImages[2].sprite = result;
        }

        private void Scroll(float distance)
        {
            stripOffset += distance;

            while (stripOffset >= symbolHeight)
            {
                stripOffset -= symbolHeight;

                for (int i = 0; i < symbolImages.Length - 1; i++)
                {
                    spinningSprites[i] = symbolImages[i + 1].sprite;
                }

                spinningSprites[VisibleSymbolCount - 1] =
                    symbols[UnityEngine.Random.Range(0, symbols.Length)];

                for (int i = 0; i < symbolImages.Length; i++)
                {
                    symbolImages[i].sprite = spinningSprites[i];
                }
            }

            strip.anchoredPosition = new Vector2(0f, stripOffset);
        }

        private void SetRandomSprite(int index)
        {
            symbolImages[index].sprite = symbols[UnityEngine.Random.Range(0, symbols.Length)];
        }
    }
}
