using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Wendogo
{
    public class PopupSentences : MonoBehaviour
    {
        #region Instance
        
        /// <summary>
        /// Gets the singleton instance of the <see cref="PopupSentences"/>.
        /// This property provides access to the single, globally accessible instance
        /// of the <see cref="PopupSentences"/> class, ensuring it follows a singleton pattern.
        /// </summary>
        public static PopupSentences Instance { get; private set; }

        #endregion
        
        #region Variables
        
        [InfoBox("Use {x} (and only this !!!) when you want it to be replaced by something else.", InfoMessageType.Warning)]
        
        [Space(10), TextArea(3, 10)]
        public string otherPlayerTurnText = "C'est au tour de {x} !";
        
        [Space(10), TextArea(3, 10)]
        public string thisPlayerTurnText = "A toi de jouer !";

        #endregion

        #region Basic Methods

        public void Awake()
        {
            if (!Instance)
                Instance = this;
            
            if (otherPlayerTurnText == null) throw new Exception("'otherPlayerTurnText' is null !");
            if (thisPlayerTurnText == null) throw new Exception("'thisPlayerTurnText' is null !");
        }

        public string ReplaceX(string messageToComplete, string valueUsedToReplaceX)
        {
            return messageToComplete.Replace("{x}", valueUsedToReplaceX);
        }

        #endregion

        #region Exemple

        // [TextArea(3, 10)]
        // public string messageTemplate = "Il y a actuellement {count} joueurs connectés.";
        //
        // public List<string> players = new();
        //
        // [Button("Afficher message")]
        // private void ShowFormattedMessage()
        // {
        //     string result = messageTemplate.Replace("{count}", players.Count.ToString());
        //     Debug.Log(result);
        // }

        #endregion
    }
}