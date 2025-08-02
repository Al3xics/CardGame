using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;

namespace Wendogo
{
    public class FXEventManager : MonoBehaviour
    {
        #region Variables
        
        public static FXEventManager Instance { get; private set; }

        [Header("FX Events Data")]
        public List<FXEventAsset> fxEvents;

        [Header("References")]
        public Animator animator;
        public AudioSource audioSource;
        
        [Header("References Used By FX Event Logic")]
        public TMP_Text popupText;

        #endregion

        #region Actions References

        private Action<FXEventContext> _onPlayerTurn;
        private Action<FXEventContext> _onPlayerWin;
        private Action<FXEventContext> _onPlayerLose;
        private Action<FXEventContext> _onWendogoWin;
        private Action<FXEventContext> _onWendogoLose;

        #endregion

        #region Unity Life Cycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            
            if (fxEvents.Count == 0) throw new Exception("fxEvents is empty !");
            if (popupText == null) throw new Exception("Pop-up Text not found");
        }

        private void OnEnable()
        {
            _onPlayerTurn = fxEventContext => HandleFXEvents(fxEventContext).Forget();
            _onPlayerWin = fxEventContext => HandleFXEvents(fxEventContext).Forget();
            _onPlayerLose = fxEventContext => HandleFXEvents(fxEventContext).Forget();
            _onWendogoWin = fxEventContext => HandleFXEvents(fxEventContext).Forget();
            _onWendogoLose = fxEventContext => HandleFXEvents(fxEventContext).Forget();

            GameEvents.OnPlayerTurn += _onPlayerTurn;
            GameEvents.OnPlayerWin += _onPlayerWin;
            GameEvents.OnPlayerLose += _onPlayerLose;
            GameEvents.OnWendogoWin += _onWendogoWin;
            GameEvents.OnWendogoLose += _onWendogoLose;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerTurn -= _onPlayerTurn;
            GameEvents.OnPlayerWin -= _onPlayerWin;
            GameEvents.OnPlayerLose -= _onPlayerLose;
            GameEvents.OnWendogoWin -= _onWendogoWin;
            GameEvents.OnWendogoLose -= _onWendogoLose;
        }

        #endregion

        #region Trigger Global

        private async UniTask HandleFXEvents(FXEventContext fxEventContext)
        {
            var fxList = fxEvents.Where(fx => fx.eventType == fxEventContext.fxType).ToList();
            var sharedFXList = fxList.Where(fx => fx.isAnimPlayedForAll || fx.isSoundPlayedForAll).ToList();
            var localPlayerFX = fxList.Where(fx => !fx.isAnimPlayedForAll && !fx.isSoundPlayedForAll && fxEventContext.Player == PlayerController.LocalPlayer).ToList();
            
            // Do we need to block input for the local player?
            bool shouldBlockInput = (sharedFXList.Concat(localPlayerFX)).Any(fx => (fx.waitForAnimationEnd && fx.playAnimation) || (fx.waitForSoundEnd && fx.playSound));
            if (shouldBlockInput && fxEventContext.Player == PlayerController.LocalPlayer)
                fxEventContext.Player.DisableInput();

            var sharedTask = sharedFXList.Count > 0 ? PlayFXSequence(sharedFXList, fxEventContext) : UniTask.CompletedTask;
            var localTask = localPlayerFX.Count > 0 ? PlayFXSequence(localPlayerFX, fxEventContext) : UniTask.CompletedTask;

            await UniTask.WhenAll(sharedTask, localTask);
            
            if (shouldBlockInput && fxEventContext.Player == PlayerController.LocalPlayer)
                fxEventContext.Player.EnableInput();
        }

        private async UniTask PlayFXSequence(List<FXEventAsset> fxList, FXEventContext context)
        {
            var fxTasks = new List<UniTask>();

            foreach (var fx in fxList)
            {
                fx.logic?.PreFX(context);

                UniTask animTask = fx.playAnimation ? PlayAnimation(fx) : UniTask.CompletedTask;
                UniTask soundTask = fx.playSound ? PlaySound(fx) : UniTask.CompletedTask;
                UniTask fxTask;

                if (fx.waitForAnimationEnd && fx.waitForSoundEnd)
                    fxTask = UniTask.WhenAll(animTask, soundTask);
                else if (fx.waitForAnimationEnd)
                    fxTask = animTask;
                else if (fx.waitForSoundEnd)
                    fxTask = soundTask;
                else
                {
                    _ = animTask;
                    _ = soundTask;
                    fxTask = UniTask.CompletedTask;
                }

                fxTasks.Add(fxTask.ContinueWith(() => fx.logic?.PostFX(context)));
            }

            await UniTask.WhenAll(fxTasks);
        }

        #endregion

        #region Animation and Sound Playable

        private async UniTask PlayAnimation(FXEventAsset fx)
        {
            if (!fx.animation || !animator) return;

            // Create a temporary graph to avoid conflit when multiple animations are played.
            var graph = PlayableGraph.Create("TempAnimGraph");
            var output = AnimationPlayableOutput.Create(graph, "TempAnimOutput", animator);
            var clipPlayable = AnimationClipPlayable.Create(graph, fx.animation);
            output.SetSourcePlayable(clipPlayable);
            clipPlayable.SetDuration(fx.animation.length);

            graph.Play();

            if (fx.waitForAnimationEnd)
            {
                float duration = fx.animation.length;
                await UniTask.WaitUntil(() => clipPlayable.IsValid() && clipPlayable.GetTime() >= duration);
                graph.Destroy();
            }
            else
            {
                // Do not wait, but do not destroy immediately either
                _ = UniTask.Delay(TimeSpan.FromSeconds(fx.animation.length)).ContinueWith(() =>
                {
                    if (graph.IsValid()) graph.Destroy();
                });
            }
        }

        private async UniTask PlaySound(FXEventAsset fx)
        {
            if (!fx.clip || !audioSource) return;

            // Create a temporary graph to avoid conflit when multiple sounds are played.
            var graph = PlayableGraph.Create("TempAudioGraph");
            var output = AudioPlayableOutput.Create(graph, "TempAudioOutput", audioSource);
            var clipPlayable = AudioClipPlayable.Create(graph, fx.clip, false);
            output.SetSourcePlayable(clipPlayable);
            clipPlayable.SetDuration(fx.clip.length);

            graph.Play();

            if (fx.waitForSoundEnd)
            {
                float duration = fx.clip.length;
                await UniTask.WaitUntil(() => clipPlayable.IsValid() && clipPlayable.GetTime() >= duration);
                graph.Destroy();
            }
            else
            {
                // Do not wait, but do not destroy immediately either
                _ = UniTask.Delay(TimeSpan.FromSeconds(fx.clip.length)).ContinueWith(() =>
                {
                    if (graph.IsValid()) graph.Destroy();
                });
            }
        }

        #endregion
    }
}
