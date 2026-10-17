using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace URAH_RoyalWedding
{
    /// <summary>
    /// UIRoyalFamily.UpdateSpouse hides the spouse portrait while the king has no wife. This puts the
    /// wedding feast's action icon in that empty slot instead (the game's standard action icon, so
    /// tooltip, cost, success chance and click handling, also over the network, are the game's own).
    /// The icon stays while the feast is under way and goes away once there is a queen.
    /// </summary>
    [HarmonyPatch(typeof(UIRoyalFamily), "UpdateSpouse")]
    internal static class SpouseSlotPatch
    {
        private class Slot
        {
            public UIActionIcon icon;
        }

        private static readonly ConditionalWeakTable<UIRoyalFamily, Slot> slots = new ConditionalWeakTable<UIRoyalFamily, Slot>();

        private static void Postfix(UIRoyalFamily __instance, UICharacterIcon ___m_SpouseIcon)
        {
            Slot slot = slots.GetOrCreateValue(__instance);
            Logic.Action action = ShownAction(__instance);
            if (action == null || ___m_SpouseIcon == null)
            {
                if (slot.icon != null)
                {
                    slot.icon.SetObject(null, null);
                    slot.icon.gameObject.SetActive(false);
                }
                return;
            }
            if (slot.icon == null)
            {
                slot.icon = CreateIcon(action, ___m_SpouseIcon);
                if (slot.icon == null)
                {
                    return;
                }
            }
            slot.icon.gameObject.SetActive(true);
            slot.icon.SetObject(action, null);
        }

        /// <summary>The wedding action if the window shows our own unmarried king and the action may be shown.</summary>
        private static Logic.Action ShownAction(UIRoyalFamily window)
        {
            Logic.Kingdom kingdom = window.Data?.GetKingdom();
            if (kingdom == null || kingdom != BaseUI.LogicKingdom())
            {
                return null;
            }
            Logic.Action action = kingdom.actions?.Find(RoyalWeddingAction.DefKey);
            if (!(action is RoyalWeddingAction))
            {
                return null;
            }
            // the game's own rule: "ok" and "_reason" keep an action visible, anything else hides it
            string result = action.Validate();
            if (result != "ok" && (string.IsNullOrEmpty(result) || result[0] != '_'))
            {
                return null;
            }
            return action;
        }

        private static UIActionIcon CreateIcon(Logic.Action action, UICharacterIcon spouseIcon)
        {
            RectTransform spouse = spouseIcon.transform as RectTransform;
            RectTransform parent = spouse?.parent as RectTransform;
            GameObject go = ObjectIcon.GetIcon(action, null, parent);
            if (go == null)
            {
                Plugin.Log.LogWarning("No action icon prefab for the royal wedding feast");
                return null;
            }
            go.name = "id_URAH_RoyalWedding";
            if (go.transform is RectTransform rect)
            {
                rect.anchorMin = spouse.anchorMin;
                rect.anchorMax = spouse.anchorMax;
                rect.pivot = spouse.pivot;
                rect.anchoredPosition = spouse.anchoredPosition;
                rect.SetSiblingIndex(spouse.GetSiblingIndex() + 1);
            }
            return go.GetComponent<UIActionIcon>();
        }
    }
}
