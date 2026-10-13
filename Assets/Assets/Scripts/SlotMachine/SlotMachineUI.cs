using System;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachineGame
{
    public sealed class SlotMachineUI : MonoBehaviour
    {
        private const float ReferenceWidth = 1600f;
        private const float ReferenceHeight = 900f;

        private RectTransform canvasRect;
        private RectTransform machineRect;
        private RectTransform controlPanelRect;
        private RectTransform[] reelRects;
        private Image machineImage;
        private Image controlPanelImage;
        private Text headingText;
        private Text subtitleText;
        private Text balanceText;
        private Text betText;
        private Text messageText;
        private Text payoutText;
        private Text spinButtonLabel;
        private Button spinButton;
        private Button decreaseBetButton;
        private Button increaseBetButton;
        private float lastCanvasWidth = -1f;
        private float lastCanvasHeight = -1f;
        private readonly float[] reelXOffsets = { -131f, -1f, 129f };

        public ReelView[] Reels { get; private set; }

        public static SlotMachineUI Create(
            Transform parent,
            Sprite[] symbols,
            Sprite backgroundSprite,
            Sprite cabinetSprite,
            Sprite panelSprite,
            Sprite[] decreaseBetButtonSprites,
            Sprite[] increaseBetButtonSprites,
            Sprite[] spinButtonSprites,
            Action onSpin,
            Action increaseBet,
            Action decreaseBet)
        {
            GameObject canvasObject = new GameObject("Slot Machine Canvas");
            canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            SlotMachineUI userInterface = canvasObject.AddComponent<SlotMachineUI>();
            userInterface.Build(
                symbols,
                backgroundSprite,
                cabinetSprite,
                panelSprite,
                decreaseBetButtonSprites,
                increaseBetButtonSprites,
                spinButtonSprites,
                onSpin,
                increaseBet,
                decreaseBet);
            return userInterface;
        }

        private void Build(
            Sprite[] symbols,
            Sprite backgroundSprite,
            Sprite cabinetSprite,
            Sprite panelSprite,
            Sprite[] decreaseBetButtonSprites,
            Sprite[] increaseBetButtonSprites,
            Sprite[] spinButtonSprites,
            Action onSpin,
            Action increaseBet,
            Action decreaseBet)
        {
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;

            canvasRect = (RectTransform)transform;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Image backdrop = CreateImage(
                "Gradient backdrop",
                transform,
                backgroundSprite,
                Vector2.zero,
                Vector2.zero);
            RectTransform backdropRect = (RectTransform)backdrop.transform;
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;

            machineImage = CreateImage(
                "JACKPOT cabinet",
                transform,
                cabinetSprite,
                new Vector2(30f, 54f),
                new Vector2(617f, 624f));
            machineImage.preserveAspect = true;

            headingText = CreateText(
                "Game title",
                transform,
                font,
                new Vector2(0f, -32f),
                new Vector2(380f, 42f),
                25,
                new Color(1f, 0.84f, 0.43f, 1f),
                "MIDNIGHT SPIN");

            subtitleText = CreateText(
                "Game subtitle",
                transform,
                font,
                new Vector2(0f, -69f),
                new Vector2(400f, 25f),
                13,
                new Color(0.79f, 0.77f, 0.99f, 1f),
                "PLACE YOUR BET  -  MATCH ALL THREE REELS");

            reelRects = new RectTransform[3];
            Reels = new ReelView[reelRects.Length];

            for (int i = 0; i < reelRects.Length; i++)
            {
                GameObject reelObject = new GameObject("Reel " + (i + 1), typeof(RectTransform));
                reelObject.transform.SetParent(transform, false);
                reelRects[i] = reelObject.GetComponent<RectTransform>();
                SetCenteredRect(
                    reelRects[i],
                    new Vector2(reelXOffsets[i], 15f),
                    new Vector2(110f, 194f));

                Reels[i] = reelObject.AddComponent<ReelView>();
                Reels[i].Initialize(symbols);
            }

            controlPanelImage = CreateImage(
                "Gold-trimmed control panel",
                transform,
                panelSprite,
                Vector2.zero,
                new Vector2(700f, 202f));
            controlPanelImage.color = new Color(1f, 1f, 1f, 0.92f);
            controlPanelRect = (RectTransform)controlPanelImage.transform;

            messageText = CreateText(
                "Spin result",
                transform,
                font,
                Vector2.zero,
                new Vector2(650f, 34f),
                20,
                Color.white,
                string.Empty);

            balanceText = CreateText(
                "Credit balance",
                transform,
                font,
                Vector2.zero,
                new Vector2(150f, 48f),
                18,
                new Color(1f, 0.89f, 0.56f, 1f),
                string.Empty);

            betText = CreateText(
                "Current bet",
                transform,
                font,
                Vector2.zero,
                new Vector2(130f, 38f),
                21,
                Color.white,
                string.Empty);

            payoutText = CreateText(
                "Payout guide",
                transform,
                font,
                Vector2.zero,
                new Vector2(660f, 25f),
                14,
                new Color(0.88f, 0.83f, 1f, 1f),
                string.Empty);

            decreaseBetButton = CreateSpriteButton(
                "Decrease bet",
                transform,
                decreaseBetButtonSprites,
                decreaseBet);
            increaseBetButton = CreateSpriteButton(
                "Increase bet",
                transform,
                increaseBetButtonSprites,
                increaseBet);
            spinButton = CreateSpriteButton(
                "Spin",
                transform,
                spinButtonSprites,
                onSpin);
            Image spinGraphic = (Image)spinButton.targetGraphic;
            spinGraphic.rectTransform.sizeDelta = new Vector2(86f, 86f);

            CreateText(
                "Spin button label",
                transform,
                font,
                Vector2.zero,
                new Vector2(112f, 25f),
                16,
                new Color(1f, 0.87f, 0.51f, 1f),
                "SPIN");
            spinButtonLabel = transform.Find("Spin button label").GetComponent<Text>();

            UpdateLayout();
        }

        public void UpdateDisplay(
            long balance,
            long currentBet,
            string message,
            bool isSpinning,
            bool canSpin,
            string payoutGuide)
        {
            balanceText.text = "CREDITS  " + balance.ToString("N0");
            betText.text = "BET  " + currentBet.ToString("N0");
            messageText.text = message;
            messageText.color = message.StartsWith("WIN!", StringComparison.Ordinal)
                ? new Color(1f, 0.83f, 0.34f, 1f)
                : new Color(0.98f, 0.95f, 1f, 1f);
            payoutText.text = payoutGuide;
            SetControlsEnabled(!isSpinning, canSpin);
        }

        public void SetControlsEnabled(bool allowBetChanges, bool canSpin)
        {
            if (spinButton != null)
            {
                spinButton.interactable = allowBetChanges && canSpin;
            }

            if (decreaseBetButton != null)
            {
                decreaseBetButton.interactable = allowBetChanges;
            }

            if (increaseBetButton != null)
            {
                increaseBetButton.interactable = allowBetChanges;
            }
        }

        private Image CreateImage(
            string name,
            Transform parent,
            Sprite sprite,
            Vector2 position,
            Vector2 size)
        {
            Image image = CreateRectObject(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            SetCenteredRect((RectTransform)image.transform, position, size);
            return image;
        }

        private Text CreateText(
            string name,
            Transform parent,
            Font font,
            Vector2 position,
            Vector2 size,
            int fontSize,
            Color color,
            string initialText)
        {
            Text text = CreateRectObject(name, parent).gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = initialText;

            RectTransform rect = (RectTransform)text.transform;
            SetCenteredRect(rect, position, size);
            return text;
        }

        private Button CreateSpriteButton(
            string name,
            Transform parent,
            Sprite[] stateSprites,
            Action callback)
        {
            Image image = CreateImage(
                name,
                parent,
                stateSprites[0],
                Vector2.zero,
                new Vector2(66f, 66f));
            image.raycastTarget = true;

            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.spriteState = new SpriteState
            {
                highlightedSprite = stateSprites[1],
                pressedSprite = stateSprites[2],
                disabledSprite = stateSprites[3]
            };
            button.transition = Selectable.Transition.SpriteSwap;
            button.onClick.AddListener(() => callback());
            return button;
        }

        private RectTransform CreateRectObject(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }

        private static void SetCenteredRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private void Update()
        {
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;
            if (canvasWidth < 100f || canvasHeight < 100f ||
                (Mathf.Approximately(canvasWidth, lastCanvasWidth) &&
                 Mathf.Approximately(canvasHeight, lastCanvasHeight)))
            {
                return;
            }

            lastCanvasWidth = canvasWidth;
            lastCanvasHeight = canvasHeight;

            float machineScale = Mathf.Min(
                1f,
                (canvasWidth - 48f) / 617f,
                (canvasHeight - 280f) / 624f);
            machineScale = Mathf.Max(0.35f, machineScale);
            machineRect = (RectTransform)machineImage.transform;
            machineRect.localScale = Vector3.one * machineScale;
            machineRect.anchoredPosition = new Vector2(30f * machineScale, canvasHeight * 0.06f);
            ((RectTransform)headingText.transform).anchoredPosition =
                new Vector2(0f, canvasHeight * 0.5f - 45f);
            ((RectTransform)subtitleText.transform).anchoredPosition =
                new Vector2(0f, canvasHeight * 0.5f - 78f);

            for (int i = 0; i < reelRects.Length; i++)
            {
                SetCenteredRect(
                    reelRects[i],
                    new Vector2(reelXOffsets[i] * machineScale, 15f * machineScale),
                    new Vector2(110f * machineScale, 194f * machineScale));
            }

            float panelWidth = Mathf.Min(700f, canvasWidth - 24f);
            float panelScale = panelWidth / 700f;
            Vector2 panelCenter = new Vector2(0f, -canvasHeight * 0.5f + 116f * panelScale);

            SetCenteredRect(controlPanelRect, panelCenter, new Vector2(panelWidth, 202f * panelScale));
            ((RectTransform)messageText.transform).anchoredPosition =
                panelCenter + new Vector2(0f, 66f * panelScale);
            ((RectTransform)messageText.transform).sizeDelta =
                new Vector2(panelWidth - 40f, 34f * panelScale);
            ((RectTransform)balanceText.transform).anchoredPosition =
                panelCenter + new Vector2(-235f * panelScale, 5f * panelScale);
            ((RectTransform)balanceText.transform).sizeDelta =
                new Vector2(170f * panelScale, 44f * panelScale);
            ((RectTransform)betText.transform).anchoredPosition =
                panelCenter + new Vector2(-5f * panelScale, 18f * panelScale);
            ((RectTransform)betText.transform).sizeDelta =
                new Vector2(140f * panelScale, 35f * panelScale);
            ((RectTransform)decreaseBetButton.transform).GetComponent<RectTransform>().anchoredPosition =
                panelCenter + new Vector2(-86f * panelScale, -29f * panelScale);
            ((RectTransform)increaseBetButton.transform).GetComponent<RectTransform>().anchoredPosition =
                panelCenter + new Vector2(76f * panelScale, -29f * panelScale);
            ((RectTransform)spinButton.transform).anchoredPosition =
                panelCenter + new Vector2(225f * panelScale, -4f * panelScale);
            ((RectTransform)spinButton.targetGraphic.transform).GetComponent<RectTransform>().sizeDelta =
                new Vector2(82f * panelScale, 82f * panelScale);
            ((RectTransform)spinButtonLabel.transform).anchoredPosition =
                panelCenter + new Vector2(225f * panelScale, -53f * panelScale);
            ((RectTransform)payoutText.transform).anchoredPosition =
                panelCenter + new Vector2(0f, -66f * panelScale);
            ((RectTransform)payoutText.transform).sizeDelta =
                new Vector2(panelWidth - 30f, 22f * panelScale);
        }

    }
}
