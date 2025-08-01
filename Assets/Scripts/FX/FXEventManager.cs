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

        [Header("FX Events Data")]
        public List<FXEventAsset> fxEvents;

        [Header("References")]
        public Animator animator;
        public AudioSource audioSource;
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
            if (fxEvents.Count == 0) throw new Exception("fxEvents is empty !");
            if (popupText == null) throw new Exception("Pop-up Text not found");
        }

        private void OnEnable()
        {
            _onPlayerWin = fxEventContext => TriggerOnPlayerWinAsync(fxEventContext).Forget();
            _onPlayerLose = fxEventContext => TriggerOnPlayerLoseAsync(fxEventContext).Forget();
            _onWendogoWin = fxEventContext => TriggerOnWendogoWinAsync(fxEventContext).Forget();
            _onWendogoLose = fxEventContext => TriggerOnWendogoLoseAsync(fxEventContext).Forget();
            _onPlayerTurn = fxEventContext => TriggerOnPlayerTurnAsync(fxEventContext).Forget();

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

        #region Trigger Specific

        private async UniTaskVoid TriggerOnPlayerTurnAsync(FXEventContext fxEventContext)
        {
            #region Pre FX

            bool isLocal = fxEventContext.Player == PlayerController.LocalPlayer;

            if (isLocal)
            {
                fxEventContext.Player.DisableInput();
                popupText.text = PopupSentences.Instance.thisPlayerTurnText;
            }
            else
            {
                string playerName = AutoSessionBootstrapper.AutoConnect
                    ? fxEventContext.Player.name
                    : ServerManager.Instance.GetPlayerName(fxEventContext.Player.OwnerClientId);
                popupText.text = PopupSentences.Instance.ReplaceX(PopupSentences.Instance.otherPlayerTurnText, playerName);
            }

            #endregion

            await HandleFXEvents(fxEventContext);

            #region Post FX

            if (isLocal) fxEventContext.Player.EnableInput();
            popupText.text = "";

            #endregion
        }
        
        private async UniTaskVoid TriggerOnPlayerWinAsync(FXEventContext fxEventContext)
        {
            #region Pre FX

            // todo

            #endregion

            await HandleFXEvents(fxEventContext);

            #region Post FX

            // todo

            #endregion
        }
        
        private async UniTaskVoid TriggerOnPlayerLoseAsync(FXEventContext fxEventContext)
        {
            #region Pre FX

            // todo

            #endregion

            await HandleFXEvents(fxEventContext);

            #region Post FX

            // todo

            #endregion
        }
        
        private async UniTaskVoid TriggerOnWendogoWinAsync(FXEventContext fxEventContext)
        {
            #region Pre FX

            // todo

            #endregion

            await HandleFXEvents(fxEventContext);

            #region Post FX

            // todo

            #endregion
        }
        
        private async UniTaskVoid TriggerOnWendogoLoseAsync(FXEventContext fxEventContext)
        {
            #region Pre FX

            // todo

            #endregion

            await HandleFXEvents(fxEventContext);

            #region Post FX

            // todo

            #endregion
        }

        #endregion

        #region Trigger Global

        private async UniTask HandleFXEvents(FXEventContext fxEventContext)
        {
            var fxList = fxEvents.Where(fx => fx.eventType == fxEventContext.fxType).ToList();
            var sharedFXList = fxList.Where(fx => fx.isAnimPlayedForAll || fx.isSoundPlayedForAll).ToList();
            var localPlayerFX = fxList.Where(fx => !fx.isAnimPlayedForAll && !fx.isSoundPlayedForAll && fxEventContext.Player == PlayerController.LocalPlayer).ToList();

            var sharedTask = sharedFXList.Count > 0 ? PlayFXSequence(sharedFXList) : UniTask.CompletedTask;
            var localTask = localPlayerFX.Count > 0 ? PlayFXSequence(localPlayerFX) : UniTask.CompletedTask;

            await UniTask.WhenAll(sharedTask, localTask);
        }

        private async UniTask PlayFXSequence(List<FXEventAsset> fxList)
        {
            foreach (var fx in fxList)
            {
                UniTask animTask = fx.playAnimation ? PlayAnimation(fx) : UniTask.CompletedTask;
                UniTask soundTask = fx.playSound ? PlaySound(fx) : UniTask.CompletedTask;

                // Wait selectively based on the config
                if (fx.waitForAnimationEnd && fx.waitForSoundEnd)
                {
                    await UniTask.WhenAll(animTask, soundTask);
                }
                else if (fx.waitForAnimationEnd)
                {
                    await animTask;
                }
                else if (fx.waitForSoundEnd)
                {
                    await soundTask;
                }
                // else: fire and forget both
            }
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
            }

            graph.Destroy();
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
            }

            graph.Destroy();
        }

        #endregion
    }
}
