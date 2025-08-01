using Sirenix.OdinInspector;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(menuName = "FX/FXEventAsset")]
    public class FXEventAsset : ScriptableObject
    {
        public FXEventType eventType;

        [Header("Animation")]
        public bool playAnimation;
        [ShowIf("playAnimation")] public AnimationClip animation;
        [ShowIf("playAnimation")] public bool waitForAnimationEnd;
        [ShowIf("playAnimation")] public bool isAnimPlayedForAll;

        [Header("Sound")]
        public bool playSound;
        [ShowIf("playSound")] public AudioClip clip;
        [ShowIf("playSound")] public bool waitForSoundEnd;
        [ShowIf("playSound")] public bool isSoundPlayedForAll;
    }
}