using System;
using TMPro;
using UnityEngine;

namespace Wendogo
{
    public class ResourcesSelectionUI : MonoBehaviour
    {
        #region Variables

        [Header("Possessed Resources Text")]
        [SerializeField] private TMP_Text foodText;
        [SerializeField] private TMP_Text woodText;
        
        [Header("Incremented/Decremented Text")]
        [SerializeField] private TMP_Text foodCptText;
        [SerializeField] private TMP_Text woodCptText;
        
        private PlayerController _playerController;
        private int _food;
        private int _wood;
        private int _maxFood;
        private int _maxWood;

        #endregion

        #region Actions

        public static event Action<int, int> OnResourcesValidated;

        #endregion

        #region Methods

        private void OnEnable()
        {
            _playerController = PlayerController.LocalPlayer;
            GetCurrentResources();
        }

        /// <summary>
        /// Retrieves and updates the current resource count (food or wood) based on the simulation state
        /// (day or night) and sets the values for internal tracking. The method determines whether to
        /// update food or wood based on the provided parameter.
        /// </summary>
        private void GetCurrentResources()
        {
            if (_playerController.IsSimulatingNight)
            {
                _maxFood = _playerController.hiddenFood;
                _maxWood = _playerController.hiddenWood;
            }
            else
            {
                _maxFood = _playerController.food.Value;
                _maxWood = _playerController.wood.Value;
            }
            
            UpdateUI();
        }

        /// <summary>
        /// Validates the current resources (food and wood) and triggers an event to notify listeners
        /// with the updated resource values. This method is typically used to confirm resource states
        /// after modifications or actions are taken.
        /// </summary>
        public void ValidateResources()
        {
            OnResourcesValidated?.Invoke(_food, _wood);
        }

        /// <summary>
        /// Resets the resources displayed in the UI to their default values (zero) and updates
        /// the internal tracking variables for food and wood accordingly.
        /// </summary>
        public void ResetResources()
        {
            foodCptText.text = "0";
            _food = 0;
            
            woodCptText.text = "0";
            _wood = 0;
        }

        /// <summary>
        /// Increments the current resource count (food or wood) by one based on the provided parameter.
        /// This method determines whether to increment food or wood.
        /// </summary>
        /// <param name="isFood">If true, increments the food resource count.
        /// If false, increments the wood resource count.</param>
        public void Increment(bool isFood)
        {
            if (isFood)
                _food = Mathf.Clamp(_food + 1, 0, _maxFood);
            else
                _wood = Mathf.Clamp(_wood + 1, 0, _maxWood);

            UpdateUICpt(isFood);
        }

        /// <summary>
        /// Decrements the count of a specific resource (food or wood) based on
        /// the provided parameter and updates the associated UI component.
        /// </summary>
        /// <param name="isFood">If true, decrements the food count.
        /// If false, decrements the wood count.</param>
        public void Decrement(bool isFood)
        {
            if (isFood)
                _food = Mathf.Clamp(_food - 1, 0, _maxFood);
            else
                _wood = Mathf.Clamp(_wood - 1, 0, _maxWood);

            UpdateUICpt(isFood);
        }

        /// <summary>
        /// Updates the count text for a specific resource (food or wood) based on
        /// the current value of the resource. Determines whether to update the
        /// food or wood count based on the provided parameter.
        /// </summary>
        /// <param name="isFood">If true, updates the food count text.
        /// If false, updates the wood count text.</param>
        private void UpdateUICpt(bool isFood)
        {
            if (isFood)
                foodCptText.text = _food.ToString();
            else
                woodCptText.text = _wood.ToString();
        }

        /// <summary>
        /// Updates the UI to reflect the current resources a player possesses.
        /// This method sets the respective text elements for food and wood
        /// to their updated values stored in the corresponding fields.
        /// </summary>
        private void UpdateUI()
        {
            foodText.text = _maxFood.ToString();
            woodText.text = _maxWood.ToString();
        }

        #endregion
    }
}