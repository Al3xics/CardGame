using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Analytics;
using UnityEngine;

namespace Wendogo
{
    [CreateAssetMenu(fileName = "GroupAttack", menuName = "Card Effects/Group Attack")]
    public class GroupAttack : CardEffect
    {
        public int attackValue = 2;

        public override void Apply(ulong owner, ulong target, int value = -1)
        {
            if (value <= -1) value = 0;
            var newValue = attackValue + value;
            var votes = ServerManager.Instance.Votes;
            ulong votedPlayer = VoteResult(votes);

            if (votedPlayer != 1000)
            {
                var targetPlayer = PlayerController.GetPlayer(votedPlayer);
                
                if (targetPlayer != null)
                {
                    AnalyticsManager.Instance.RecordEvent(new CustomEvent("groupAttackActiveCardWasApplied"));
                    
                    if (!targetPlayer.hasGuardian.Value)
                    {
                        ServerManager.Instance.ChangePlayerHealthRpc(-newValue, targetPlayer.OwnerClientId);
                    }
                    else
                    {
                        ServerManager.Instance.ChangePlayerHealthRpc(-newValue, targetPlayer.guardian.OwnerClientId);
                        targetPlayer.hasGuardian.Value = false;
                    }
                }
            }
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
            ServerManager.Instance.UseAllUIForVotersRpc(true, true);
        }

        public override void HideUI()
        {
            ServerManager.Instance.UseAllUIForVotersRpc(false, false);
        }
    }
}