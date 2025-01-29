using Godot;

public partial class SpinningMap : Node2D
{
	private float _rotationSpeed = 30f;
	private float _spinInterval = 5f;
	private float _timeElapsed = 0f;

	public override void _Process(double delta)
	{
		_timeElapsed += (float)delta;

		if (_timeElapsed >= _spinInterval)
		{
			RotateMap((float)delta);
		}
	}

	private void RotateMap(float delta)
	{
		RotationDegrees += _rotationSpeed * delta;

		if (Mathf.Abs(RotationDegrees) >= 360f)
		{
			RotationDegrees = 0f;
			_timeElapsed = 0f;
		}
	}
}
