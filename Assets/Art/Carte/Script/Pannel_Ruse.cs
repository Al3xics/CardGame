using UnityEngine;

public class PannelRuse : MonoBehaviour
{bool toggle=false;
   public GameObject PanelRuse;

	public void OpenPanelRuse()
	{
		
		if(PanelRuse != null)
		{
			toggle = !toggle;
			PanelRuse.SetActive(toggle);
			Debug.Log(toggle);
		}
	}
}
