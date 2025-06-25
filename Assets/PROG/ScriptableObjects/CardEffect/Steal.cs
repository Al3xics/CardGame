using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wendogo;

[System.Serializable]
public class Steal : CardEffect
{
    public int typeOfRessource = 0;

    public override void Apply(ulong owner, ulong target, int value = default)
    {
        if(typeOfRessource == 0)
        {
            //ScavengeFood.Instance.Apply(owner, owner, 1);
            if (owner == 0)
            {
                ServerManager.Instance.player1Food.Value += 1;
            }
            else if (owner == 1)
            {
                ServerManager.Instance.player2Food.Value += 1;
            }
            else if (owner == 2)
            {
                ServerManager.Instance.player3Food.Value += 1;
            }
            else
            {
                ServerManager.Instance.player4Food.Value += 1;
            }

            //ScavengeFood.Instance.Apply(owner, target, -1);
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
            //ScavengeWood.Instance.Apply(owner, owner, 1);
            if (owner == 0)
            {
                ServerManager.Instance.player1Wood.Value += 1;
            }
            else if (owner == 1)
            {
                ServerManager.Instance.player2Wood.Value += 1;
            }
            else if (owner == 2)
            {
                ServerManager.Instance.player3Wood.Value += 1;
            }
            else
            {
                ServerManager.Instance.player4Wood.Value += 1;
            }

            //ScavengeWood.Instance.Apply(owner, target, -1);
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