using Sirenix.OdinInspector;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(menuName = "FX/FX Event Asset")]
    public class FXEventAsset : ScriptableObject
    {
        #region FX

        [Header("FX General")]
        [Tooltip("The event type that triggers the visual effect.")]
        public FXEventType eventType = FXEventType.None;
        
        [Tooltip("The logic to be applied to the visual effect (Pre and Post FX).")]
        public FXEventLogicBase logic;

        #endregion

        #region Animation

        [Header("Animation")]
        [Tooltip("Should an animation be played ?")]
        public bool playAnimation;

        [ShowIf("playAnimation")]
        [Tooltip("The animation clip to be played.")]
        public AnimationClip animation;

        [ShowIf("playAnimation")]
        [Tooltip("Should the system wait for the animation to finish before proceeding ?")]
        public bool waitForAnimationEnd;

        [ShowIf("playAnimation")]
        [Tooltip("Should the animation be played for all players ?")]
        public bool isAnimPlayedForAll;

        #endregion

        #region Sound

        [Header("Sound")]
        [Tooltip("Should a sound effect be played ?")]
        public bool playSound;

        [ShowIf("playSound")]
        [Tooltip("The audio clip to be played.")]
        public AudioClip clip;

        [ShowIf("playSound")]
        [Tooltip("Should the system wait for the sound to finish before proceeding ?")]
        public bool waitForSoundEnd;

        [ShowIf("playSound")]
        [Tooltip("Should the sound be played for all players ?")]
        public bool isSoundPlayedForAll;

        #endregion
    }
}