using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wendogo;

[System.Serializable]
public class ScavengeWood : CardEffect
{
    public int woodGained = 1;

    public override void Apply(ulong owner, ulong target, int value = default)
    {
        if (target == 0)
        {
            ServerManager.Instance.player1Wood.Value += woodGained;
        }
        else if (target == 1)
        {
            ServerManager.Instance.player2Wood.Value += woodGained;
        }
        else if (target == 2)
        {
            ServerManager.Instance.player3Wood.Value += woodGained;
        }
        else
        {
            ServerManager.Instance.player4Wood.Value += woodGained;
        }
    }
}
