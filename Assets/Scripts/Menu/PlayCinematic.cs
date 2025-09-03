using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Menu
{
    public class PlayCinematic : MonoBehaviour
    {
        [SerializeField] private GameObject videoCanvas;
        [SerializeField] private GameObject audioSource;
        
        public async void PerformStartGameSequence()
        {
            audioSource.SetActive(false);
            videoCanvas.SetActive(true);

            await UniTask.WaitForSeconds(89);
            audioSource.SetActive(true);
            videoCanvas.SetActive(false);
        }
    }
}