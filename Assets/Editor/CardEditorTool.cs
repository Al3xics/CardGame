using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities.Editor;

namespace Wendogo
{
    public class CardEditorTool : OdinMenuEditorWindow
    {
        [MenuItem("Tools/Card Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<CardEditorTool>();
            window.titleContent = new GUIContent("Card Editor");
            window.Show();
        }

        [PropertySpace(10)]
        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public CardDatabaseSO CardDatabase;

        private string searchQuery = "";

        // Simple in‑editor history storage per card GUID
        private struct HistoryEntry { public string Timestamp, Json; }
        private Dictionary<string, List<HistoryEntry>> historyCache = new();

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true)
            {
                // Root: shows the main database inspector
                { "Database", this },
                // Historic category retains its own helper (with the Show Latest button)
                { "Database/Historic", new HistoricHelper(this) }
            };

            CardDatabase = AssetDatabase.LoadAssetAtPath<CardDatabaseSO>("Assets/ScriptableObjects/CardDatabase.asset");
            if (CardDatabase == null)
                return tree;

            // ——— Normal card list ———
            foreach (var card in CardDatabase.cardList)
            {
                if (IsCardVisible(card))
                    tree.Add($"Cards/{card.Name} ({card.ID})", card);
            }

            // ——— Historic list of the same cards ———
            foreach (var card in CardDatabase.cardList)
            {
                if (IsCardVisible(card))
                    tree.Add($"Database/Historic/{card.Name} ({card.ID})", card);
            }

            // visuals folder, etc.
            tree.AddAllAssetsAtPath("visuals", "Assets/Textures/CardVisuals", typeof(Texture2D), true);

            tree.SortMenuItemsByName();
            tree.Add("✚ Create New Card", new CreateCardHelper(CardDatabase));
            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            SirenixEditorGUI.Title("Card Editor", null, TextAlignment.Center, true);
            GUILayout.Space(10);
            searchQuery = SirenixEditorFields.TextField("Search", searchQuery);
            GUILayout.Space(10);

            // If a CardDataSO is selected (either in Cards/… or Historic/…), show its history UI
            var selection = this.MenuTree.Selection.FirstOrDefault()?.Value as CardDataSO;
            if (selection != null)
            {
                var path = AssetDatabase.GetAssetPath(selection);
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (!historyCache.ContainsKey(guid))
                    historyCache[guid] = new List<HistoryEntry>();

                SirenixEditorGUI.BeginBox();
                GUILayout.Label("Change History", EditorStyles.boldLabel);

                // Snapshot button
                var saveIcon = EditorIcons.Info;

                if (SirenixEditorGUI.IconButton(saveIcon))
                {
                    var json = JsonUtility.ToJson(selection, true);
                    historyCache[guid].Add(new HistoryEntry
                    {
                        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Json = json
                    });
                }

                foreach (var entry in historyCache[guid].ToList())
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(entry.Timestamp, GUILayout.MinWidth(150));
                    if (GUILayout.Button("Rollback", GUILayout.ExpandWidth(false)))
                    {
                        JsonUtility.FromJsonOverwrite(entry.Json, selection);
                        EditorUtility.SetDirty(selection);
                        AssetDatabase.SaveAssets();
                        ForceMenuTreeRebuild();
                    }
                    GUILayout.EndHorizontal();
                }

                SirenixEditorGUI.EndBox();
                GUILayout.Space(10);
            }
        }

        private bool IsCardVisible(CardDataSO card)
        {
            if (string.IsNullOrEmpty(searchQuery)) return true;
            return card.Name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0
                || card.CardEffect.name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // The Historic category helper (kept as you had it)
        private class HistoricHelper
        {
            private readonly CardEditorTool window;
            public HistoricHelper(CardEditorTool window) => this.window = window;

            [Button("Show Latest Version", ButtonSizes.Large), GUIColor(0.8f, 1f, 0.2f)]
            private void ShowLatest()
            {
                var card = window.MenuTree.Selection.FirstOrDefault()?.Value as CardDataSO;
                if (card == null)
                {
                    EditorUtility.DisplayDialog("Historic", "No card selected.", "OK");
                    return;
                }

                var path = AssetDatabase.GetAssetPath(card);
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (!window.historyCache.TryGetValue(guid, out var list) || list.Count == 0)
                {
                    EditorUtility.DisplayDialog("Historic", "No history for this card.", "OK");
                }
                else
                {
                    var latest = list.Last();
                    EditorUtility.DisplayDialog($"Historic – {card.Name}", latest.Json, "OK");
                }
            }
        }

        public class CreateCardHelper
        {
            [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
            [HideLabel]
            public CardDataSO NewCard;

            private CardDatabaseSO _database;
            public CreateCardHelper(CardDatabaseSO database)
            {
                _database = database;
                NewCard = ScriptableObject.CreateInstance<CardDataSO>();
                AssignUniqueID();
            }

            private void AssignUniqueID()
            {
                int startID = 10100, maxID = 10199;
                var used = new HashSet<int>(_database.cardList.Select(c => c.ID));
                for (int id = startID; id <= maxID; id++)
                    if (!used.Contains(id))
                    {
                        NewCard.ID = id;
                        break;
                    }
            }

            [Button("Add to Database", ButtonSizes.Large)]
            private void AddCard()
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save New Card",
                    "NewCard",
                    "asset",
                    "Save new card asset"
                );
                if (string.IsNullOrEmpty(path)) return;

                AssetDatabase.CreateAsset(NewCard, path);
                AssetDatabase.SaveAssets();
                _database.cardList.Add(NewCard);
                EditorUtility.SetDirty(_database);
                AssetDatabase.SaveAssets();
                EditorWindow.GetWindow<CardEditorTool>().ForceMenuTreeRebuild();
            }
        }
    }
}
