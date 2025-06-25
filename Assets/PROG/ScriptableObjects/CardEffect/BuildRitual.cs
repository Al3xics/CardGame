using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wendogo;

[System.Serializable]
public class BuildRitual : CardEffect
{
    public int typeOfRessource = 0;

    public override void Apply(ulong owner, ulong target, int value = default)
    {
        if (typeOfRessource == 0)
        {
            if (target == 0)
            {
                ServerManager.Instance.player1Food.Value -= 1;
            }
            else if (target == 1)
            {
                ServerManager.Instance.player2Food.Value -= 1;
            }
            else if (target == 2)
            {
                ServerManager.Instance.player3Food.Value -= 1;
            }
            else
            {
                ServerManager.Instance.player4Food.Value -= 1;
            }
        }
        else
        {
            if (target == 0)
            {
                ServerManager.Instance.player1Wood.Value -= 1;
            }
            else if (target == 1)
            {
                ServerManager.Instance.player2Wood.Value -= 1;
            }
            else if (target == 2)
            {
                ServerManager.Instance.player3Wood.Value -= 1;
            }
            else
            {
                ServerManager.Instance.player4Wood.Value -= 1;
            }
        }
    }
}
