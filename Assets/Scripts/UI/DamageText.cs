using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
namespace UI
{
	public class DamageText : MonoBehaviour
	{
		TMP_Text text;
		Vector3 velocity;
		float lifetime;

		void Awake()
		{
			text = GetComponent<TMP_Text>();
		}

		public void Set(float amount)
		{
			text.text = Math.Abs(amount).ToString(CultureInfo.InvariantCulture);
			text.color = Math.Sign(amount) >= 0 ? Color.red : Color.green;
			velocity = new Vector3(Random.Range(-0.5f, 0.5f), 1.2f, 0);
			lifetime = 1.6f;
		}

		void Update()
		{
			transform.position += velocity * Time.deltaTime;
			velocity *= 0.9f;
			lifetime -= Time.deltaTime;

			Color c = text.color;
			c.a = lifetime / 0.8f;
			text.color = c;

			if (lifetime <= 0)
			{
				Destroy(gameObject);
			}
		}
	}
}
