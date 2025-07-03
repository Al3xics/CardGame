using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "BuildRitual", menuName = "Card Effects/Build Ritual")]
    public class BuildRitual : CardEffect
    {
        public GameObject selectResourcePrefab;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            
        }

        public override void ShowUI()
        {
            selectResourcePrefab.SetActive(true);
        }
        
        public override void HideUI()
        {
            selectResourcePrefab.SetActive(false);
        }
    }
}
