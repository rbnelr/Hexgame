using UnityEngine;
using UnityEngine.UI;

public class Label : MonoBehaviour {
	Text text;
	public string Name = "Value";
	public Slider Slider;

	void Start () {
		text = GetComponent<Text>();

		Set(Slider.value);
		Slider.onValueChanged.AddListener(Set);
	}
	
	public void Set (float val) {
		text.text = string.Format("{0}: {1}", Name, val);
	}
}
