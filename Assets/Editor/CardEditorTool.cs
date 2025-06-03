using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities.Editor;

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

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true)
        {
            { "Database", this }
        };

        CardDatabase = AssetDatabase.LoadAssetAtPath<CardDatabaseSO>("Assets/ScriptableObjects/CardDatabase.asset");

        if (CardDatabase == null)
        {
            return tree;
        }


        foreach (var card in CardDatabase.CardList)
        {
            if (IsCardVisible(card))
            {
                tree.Add($"Cards/{card.Name} ({card.ID})", card);
            }
        }

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
    }

    private bool IsCardVisible(CardDataSO card)
    {
        if (string.IsNullOrEmpty(searchQuery)) return true;
        return card.Name.ToLower().Contains(searchQuery.ToLower()) ||
               card.CardType.name.ToLower().Contains(searchQuery.ToLower());
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
            int startID = 10100;
            int maxID = 10199;
            HashSet<int> usedIDs = new HashSet<int>(_database.CardList.Select(c => c.ID));

            for (int id = startID; id <= maxID; id++)
            {
                if (!usedIDs.Contains(id))
                {
                    NewCard.ID = id;
                    break;
                }
            }
        }

        [Button("Add to Database", ButtonSizes.Large)]
        private void AddCard()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save New Card", "NewCard", "asset", "Save new card asset");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(NewCard, path);
                AssetDatabase.SaveAssets();

                _database.CardList.Add(NewCard);
                EditorUtility.SetDirty(_database);
                AssetDatabase.SaveAssets();

                EditorWindow.GetWindow<CardEditorTool>().ForceMenuTreeRebuild();
            }
        }
    }
}
