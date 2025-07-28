using UnityEngine;

public class Pannel : MonoBehaviour
{bool toggle=false;
   public GameObject Panel;

	public void OpenPanel()
	{
		
		if(Panel != null)
		{
			toggle = !toggle;
			Panel.SetActive(toggle);
			Debug.Log(toggle);
		}
	}
}
