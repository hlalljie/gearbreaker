using Godot;

public partial class SpinningMap : Node2D
{
	private float _rotationSpeed = 30f; // Degrees per second
	private float _spinInterval = 5f; // Spin every 5 seconds
	private float _timeElapsed = 0f;

	public override void _Process(double delta)
	{
		// Increment the time elapsed
		_timeElapsed += (float)delta;

		// Check if it's time to spin
		if (_timeElapsed >= _spinInterval)
		{
			RotateMap((float)delta); // Rotate the map
		}
	}

	private void RotateMap(float delta)
	{
		// Rotate by a small amount each frame
		RotationDegrees += _rotationSpeed * delta;

		// Reset the interval if a full spin occurs (360 degrees)
		if (Mathf.Abs(RotationDegrees) >= 360f)
		{
			RotationDegrees = 0f;
			_timeElapsed = 0f; // Reset timer for the next spin
		}
	}
}
