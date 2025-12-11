using System.Globalization;
using Mirror;
using TMPro;
using UnityEngine;
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
			text.text = amount.ToString(CultureInfo.InvariantCulture);
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
