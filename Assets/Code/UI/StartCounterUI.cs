using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class StartCounterUI : MonoBehaviour
{
	[SerializeField] Text _counterText;

	public void StartCount(double delta)
	{
		Timing.RunCoroutine(_countDown(delta));
	}
	
	IEnumerator<float> _countDown(double delta)
	{
		double timer = PhotonNetwork.time - delta;

		_counterText.gameObject.SetActive(true);

		while(timer < 3)
		{
			timer += Time.deltaTime;

			_counterText.text = timer.ToString("0");

			yield return Timing.WaitForOneFrame;
		}

		_counterText.gameObject.SetActive(false);

		Match.instance.OnCounterZero();
	}
}
