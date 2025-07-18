using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "GroupHeal", menuName = "Card Effects/Group Heal")]
    public class GroupHeal : CardEffect
    {
        public int healValue = 2;
        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            var votes = ServerManager.Instance.Votes;
            ulong votedPlayer = VoteResult(votes);

            if (votedPlayer != 1000)
            {
                var targetPlayer = PlayerController.GetPlayer(votedPlayer);
                
                if (targetPlayer != null)
                {
                    targetPlayer.health.Value += healValue;
                }
            }
            
            AnalyticsManager.Instance.RecordEvent(new CustomEvent("groupHealActiveCardWasApplied"));
            ServerManager.Instance.Votes.Clear();
        }
        
        ulong VoteResult(NetworkList<int> votes)
        {
            if (votes == null || votes.Count == 0)
                return 1000;

            Dictionary<int, int> voteCounts = new Dictionary<int, int>();
            
            foreach (int vote in votes)
            {
                if (!voteCounts.TryAdd(vote, 1))
                    voteCounts[vote]++;
            }

            int maxCount = 0;
            int maxValue = 0;
            bool isTie = false;

            foreach (var pair in voteCounts)
            {
                if (pair.Value > maxCount)
                {
                    maxCount = pair.Value;
                    maxValue = pair.Key;
                    isTie = false;
                }
                else if (pair.Value == maxCount)
                {
                    isTie = true;
                }
            }

            return isTie ? 1000 : (ulong)maxValue;
        }

        public override void ShowUI()
        {
            ServerManager.Instance.UseAllUIForVotersRpc(true, false);
        }

        public override void HideUI()
        {
            ServerManager.Instance.UseAllUIForVotersRpc(false, false);
        }
    }
}