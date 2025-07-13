using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class FilteredCardListDrawer : OdinAttributeDrawer<FilteredCardListAttribute, List<CardDataSO>>
{
    private Vector2 scrollPos;
    private readonly Dictionary<CardDataSO, PropertyTree> propertyTrees = new();
    private readonly Dictionary<CardDataSO, bool> foldouts = new();
    private bool globalFoldout = true;

    protected override void DrawPropertyLayout(GUIContent label)
    {
        var filterProperty = this.Property.Parent.FindChild(x => x.Name == this.Attribute.FilterPropertyName, true);

        CardFilterType filter = CardFilterType.All;
        if (filterProperty != null && filterProperty.ValueEntry.WeakSmartValue is CardFilterType f)
        {
            filter = f;

            if (filter == CardFilterType.All)
            {
                filterProperty.ValueEntry.WeakSmartValue = CardFilterType.All;
            }
            else if ((filter & CardFilterType.All) == CardFilterType.All)
            {
                filterProperty.ValueEntry.WeakSmartValue = filter & ~CardFilterType.All;
            }
        }

        var list = this.ValueEntry.SmartValue;

        SirenixEditorGUI.Title("Cards List", null, TextAlignment.Left, true);

        if (list == null || list.Count == 0)
        {
            EditorGUILayout.HelpBox("Card list is empty.", MessageType.Info);
            return;
        }

        if (GUILayout.Button(globalFoldout ? "Collapse all" : "Expand all"))
        {
            globalFoldout = !globalFoldout;
            foreach (var card in list)
            {
                if (card != null)
                    foldouts[card] = globalFoldout;
            }
        }

        EditorGUILayout.BeginVertical(GUI.skin.box);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        foreach (var card in list)
        {
            if (card == null || !MatchesFilter(card, filter)) continue;

            if (!propertyTrees.TryGetValue(card, out var tree) || tree == null)
            {
                tree = PropertyTree.Create(card);
                propertyTrees[card] = tree;
            }

            if (!foldouts.ContainsKey(card))
                foldouts[card] = globalFoldout;

            foldouts[card] = SirenixEditorGUI.Foldout(foldouts[card], card.name);

            if (foldouts[card])
            {
                SirenixEditorGUI.BeginBox();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🔍 Open", GUILayout.Width(80)))
                {
                    Selection.activeObject = card;
                    EditorGUIUtility.PingObject(card);
                }

                if (GUILayout.Button("📍 Ping", GUILayout.Width(80)))
                {
                    EditorGUIUtility.PingObject(card);
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5);
                tree.Draw(false);

                SirenixEditorGUI.EndBox();
            }

            GUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    
    private bool MatchesFilter(CardDataSO card, CardFilterType filter)
    {
        if (filter == CardFilterType.All)
            return true;

        return
            ((filter & CardFilterType.Passive) != 0 && card.isPassive) ||
            ((filter & CardFilterType.Active) != 0 && !card.isPassive) ||
            ((filter & CardFilterType.Group) != 0 && card.isGroup) ||
            ((filter & CardFilterType.HasTarget) != 0 && card.HasTarget);
    }

}
