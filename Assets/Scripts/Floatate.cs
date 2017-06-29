using UnityEngine;

[AddComponentMenu("Codefarts/General")]
public class Floatate : MonoBehaviour
{
	private bool wasBobHeightNeg = false;
	private bool wasBobSpeedNeg = false;

	public float bobSpeed = 3.0f;  //Bob speed
	public float bobHeight = 0.5f; //Bob height
	public float bobOffset = 0.0f;

	public float PrimaryRot = 80.0f;  //First axies degrees per second
	public float SecondaryRot = 40.0f; //Second axies degrees per second
	public float TertiaryRot = 20.0f;  //Third axies degrees per second

	public float bottom;

	public void Reset()
	{
		bobSpeed = 3.0f;
		bobHeight = 0.5f;
		bobOffset = 0.0f;

		PrimaryRot = 80.0f;
		SecondaryRot = 40.0f;
		TertiaryRot = 20.0f;

		wasBobHeightNeg = false;
		wasBobSpeedNeg = false;

		bottom = 0;	
	}

	private void WarnIfNegative(float value, ref bool wasNegative, string valueName)
	{
		if (value < 0) {
			if (!wasNegative) {
				Debug.LogWarning("Negative object " + valueName + " value! May result in undesired behavior. Continuing anyway.", this.gameObject);
				wasNegative = true;
			}
		}
		else if (wasNegative) {
			wasNegative = false;
		}
	}

	public void OnValidate()
	{
		WarnIfNegative(this.bobSpeed, ref wasBobSpeedNeg, "bobSpeed");
		WarnIfNegative(this.bobHeight, ref wasBobHeightNeg, "bobHeight");
	}

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	public void Awake()
	{
		OnValidate();

		this.bottom = this.transform.position.y;
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	public void Update()
	{
		this.transform.Rotate(Vector3.up * this.PrimaryRot * Time.deltaTime, Space.World);
		this.transform.Rotate(Vector3.right * this.SecondaryRot * Time.deltaTime, Space.Self);
		this.transform.Rotate(Vector3.forward * this.TertiaryRot * Time.deltaTime, Space.Self);

		var position = this.transform.position;
		position.y = this.bottom + (((Mathf.Cos((Time.time + this.bobOffset) * this.bobSpeed) + 1) / 2) * this.bobHeight);
		this.transform.position = position;
	}
}