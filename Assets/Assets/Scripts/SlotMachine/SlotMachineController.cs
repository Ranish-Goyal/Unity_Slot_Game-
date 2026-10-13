using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SlotMachineGame
{
    public sealed class SlotMachineController : MonoBehaviour
    {
        [Serializable]
        public sealed class SymbolPayout
        {
            [SerializeField] private Sprite sprite;
            [SerializeField] private string displayName;
            [SerializeField, Min(1)] private int tripleMatchMultiplier = 5;

            public Sprite Sprite => sprite;
            public string DisplayName => displayName;
            public int TripleMatchMultiplier => tripleMatchMultiplier;
        }

        [SerializeField, Min(1)] private int startingBalance = 1000;
        [SerializeField, Min(1)] private int startingBet = 10;
        [SerializeField, Min(1)] private int betIncrement = 10;
        [SerializeField] private List<SymbolPayout> symbolPayouts = new List<SymbolPayout>();
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite cabinetSprite;
        [SerializeField] private Sprite controlPanelSprite;
        [SerializeField] private Sprite[] decreaseBetButtonSprites;
        [SerializeField] private Sprite[] increaseBetButtonSprites;
        [SerializeField] private Sprite[] spinButtonSprites;

        private SlotMachineUI userInterface;
        private ReelView[] reels;
        private long balance;
        private long currentBet;
        private bool isSpinning;

        private void Awake()
        {
            if (!ValidateConfiguration())
            {
                enabled = false;
                return;
            }

            CreateEventSystemIfNeeded();

            balance = startingBalance;
            currentBet = startingBet;
            userInterface = SlotMachineUI.Create(
                transform,
                symbolPayouts.Select(payout => payout.Sprite).ToArray(),
                backgroundSprite,
                cabinetSprite,
                controlPanelSprite,
                decreaseBetButtonSprites,
                increaseBetButtonSprites,
                spinButtonSprites,
                StartSpin,
                IncreaseBet,
                DecreaseBet);

            reels = userInterface.Reels;
            UpdateInterface("READY - MATCH 3 TO WIN");
        }

        private bool ValidateConfiguration()
        {
            if (startingBalance <= 0 || startingBet <= 0 || betIncrement <= 0)
            {
                Debug.LogError("Slot machine balance, starting bet, and bet increment must all be greater than zero.", this);
                return false;
            }

            if (startingBet > startingBalance)
            {
                Debug.LogError("The starting bet cannot be greater than the starting balance.", this);
                return false;
            }

            if (symbolPayouts == null || symbolPayouts.Count < 2 ||
                symbolPayouts.Any(payout => payout == null || payout.Sprite == null ||
                                            string.IsNullOrWhiteSpace(payout.DisplayName) ||
                                            payout.TripleMatchMultiplier < 1))
            {
                Debug.LogError("Assign at least two symbols, display names, and positive payout multipliers to the slot machine.", this);
                return false;
            }

            if (backgroundSprite == null || cabinetSprite == null || controlPanelSprite == null ||
                !HasButtonSprites(decreaseBetButtonSprites) ||
                !HasButtonSprites(increaseBetButtonSprites) ||
                !HasButtonSprites(spinButtonSprites))
            {
                Debug.LogError("Assign the slot-machine background, cabinet, control panel, and all button-state sprites.", this);
                return false;
            }

            return true;
        }

        private static bool HasButtonSprites(Sprite[] sprites)
        {
            return sprites != null && sprites.Length == 4 && sprites.All(sprite => sprite != null);
        }

        private void CreateEventSystemIfNeeded()
        {
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                StandaloneInputModule oldInputModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (oldInputModule != null)
                {
                    oldInputModule.enabled = false;
                }

                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        private void IncreaseBet()
        {
            if (isSpinning)
            {
                return;
            }

            if (currentBet >= balance)
            {
                UpdateInterface("BET CANNOT EXCEED YOUR CREDITS");
                return;
            }

            currentBet = balance - currentBet < betIncrement
                ? balance
                : currentBet + betIncrement;
            UpdateInterface("BET UPDATED");
        }

        private void DecreaseBet()
        {
            if (isSpinning)
            {
                return;
            }

            if (currentBet <= startingBet)
            {
                UpdateInterface("MINIMUM BET IS " + startingBet);
                return;
            }

            currentBet = Math.Max(startingBet, currentBet - betIncrement);
            UpdateInterface("BET UPDATED");
        }

        private void StartSpin()
        {
            if (isSpinning)
            {
                return;
            }

            if (currentBet < 1 || currentBet > balance)
            {
                Debug.LogWarning("A spin was prevented because the current bet is outside the available balance.", this);
                UpdateInterface("BET MUST BE WITHIN YOUR CREDITS");
                return;
            }

            isSpinning = true;
            balance -= currentBet;

            int[] results = new int[reels.Length];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = UnityEngine.Random.Range(0, symbolPayouts.Count);
            }

            UpdateInterface("GOOD LUCK!");
            StartCoroutine(PlaySpin(results));
        }

        private IEnumerator PlaySpin(IReadOnlyList<int> results)
        {
            const float spinDuration = 1.65f;
            const float stopInterval = 0.55f;

            for (int i = 0; i < reels.Length; i++)
            {
                StartCoroutine(reels[i].SpinTo(symbolPayouts[results[i]].Sprite, spinDuration));
                if (i < reels.Length - 1)
                {
                    yield return new WaitForSeconds(stopInterval);
                }
            }

            yield return new WaitForSeconds(spinDuration);

            if (results[0] == results[1] && results[1] == results[2])
            {
                int multiplier = symbolPayouts[results[0]].TripleMatchMultiplier;
                if (currentBet > long.MaxValue / multiplier ||
                    balance > long.MaxValue - currentBet * multiplier)
                {
                    Debug.LogError("The slot-machine payout would exceed the maximum supported credit balance.", this);
                    UpdateInterface("CREDIT LIMIT REACHED");
                }
                else
                {
                    long winnings = currentBet * multiplier;
                    balance += winnings;
                    UpdateInterface("WIN! +" + winnings.ToString("N0") + " CREDITS");
                }
            }
            else
            {
                UpdateInterface("NO MATCH - SPIN AGAIN");
            }

            isSpinning = false;
            userInterface.SetControlsEnabled(true, balance >= currentBet);
        }

        private void UpdateInterface(string message)
        {
            if (userInterface == null)
            {
                return;
            }

            userInterface.UpdateDisplay(
                balance,
                currentBet,
                message,
                isSpinning,
                balance >= currentBet,
                CreatePayoutGuide());
        }

        private string CreatePayoutGuide()
        {
            return "TRIPLE MATCH PAYS   " + string.Join(
                "     ",
                symbolPayouts.Select(
                    payout => payout.DisplayName + " x" + payout.TripleMatchMultiplier));
        }
    }
}
