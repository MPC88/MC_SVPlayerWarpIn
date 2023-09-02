namespace MC_SVPlayerWarpIn
{
	using System;
	using UnityEngine;

	// Token: 0x0200013E RID: 318
	public class WarpOut : MonoBehaviour
	{
		internal bool isPlayer;
		private float volume;

		// Token: 0x06000A03 RID: 2563 RVA: 0x0006B000 File Offset: 0x00069200
		private void Start()
		{
			base.GetComponent<Collider>().enabled = false;
			base.GetComponent<SpaceShip>().enabled = false;
			base.GetComponent<SpaceShip>().HPBar.SetActive(false);
			if (base.GetComponent<AIControl>())
			{
				base.GetComponent<AIControl>().enabled = false;
			}
			volume = SoundSys.SFXvolume;
			if (!isPlayer)
				volume /= 2;
			base.Invoke("WarpStart", 0);
		}

		// Token: 0x06000A04 RID: 2564 RVA: 0x0006B0F8 File Offset: 0x000692F8
		private void WarpStart()
		{
			base.transform.GetChild(0).gameObject.SetActive(false);
			base.transform.GetChild(1).gameObject.SetActive(false);
			base.transform.GetChild(2).gameObject.SetActive(false);
			base.GetComponent<Collider>().enabled = false;
			base.GetComponent<AudioSource>().enabled = false;
			base.GetComponent<Rigidbody>().velocity = base.transform.forward * 400f;
			base.Invoke("WarpFinish", 0.5f);
			GameObject gameObject = GameObject.Instantiate<GameObject>(ObjManager.GetObj("Effects/WarpInEffect"), base.transform.position, base.transform.rotation);			
			gameObject.transform.SetParent(base.transform, true);
			gameObject.GetComponent<AudioSource>().volume = volume;
			//float d = 1f + base.transform.localScale.x / 3f;
			//gameObject.transform.localScale = new Vector3(1f, 1f, 1f) * d;
			gameObject.transform.localScale = Vector3.one * (base.transform.localScale.x * 0.05f);
		}

		// Token: 0x06000A05 RID: 2565 RVA: 0x0006B208 File Offset: 0x00069408
		private void WarpFinish()
		{
			base.GetComponent<Collider>().enabled = true;
			base.GetComponent<SpaceShip>().enabled = true;
			base.GetComponent<SpaceShip>().HPBar.SetActive(true);
			if (base.GetComponent<AIControl>())
			{
				base.GetComponent<AIControl>().enabled = true;
			}
			GameObject.Instantiate<GameObject>(ObjManager.GetObj("Effects/WarpEffect"), base.transform.position, base.transform.rotation).GetComponent<AudioSource>().volume = 0;
			GameObject.Destroy(this);
			if(!isPlayer)
				GameObject.Destroy(base.gameObject);
		}
	}

}
