using System;
using Godot;

public partial class Player : CharacterBody2D
{
	int dint = 0; // DEBUG int for print messages

	public Sprite2D CharSprite1;
	public Sprite2D CharSprite2;
	ActionStateType actionType;
	Vector2 direction = Vector2.Zero;

	public CharacterType CharType {get; private set;}
	public CharacterType CharType_ { get { return CharType; } }

	private const string _ROOT_PATH = "res://assets/sprite_sheets";

	int animClock = 0;
	bool animBool = true;

	// used to set direction if sprite changed but prev angle is needed
	int previousFrameY = 0;

	// for attacks that need to play out before allowing changed action
	// speed will be reduced to 1/2
	bool lockDirection = false; 

	// set to true if other actions should not be permitted
	bool actionQueued = false;
	// set to true if actionQueued should be canceled as soon as possible
	// example: combo attack 2 is not called after combo attack 1
	bool requestActionCancel = false;

	//Animation will begin after values are set in InitializeAnimation()
	bool isInitialized = false;

	public const float WalkSpeed = 60.0f;


    public override void _Ready()
    {
		CharSprite1 = GetNode<Sprite2D>("/root/Game/BossPlayer/Sprite1");
		CharSprite2 = GetNode<Sprite2D>("/root/Game/BossPlayer/Sprite2");
		CharSprite1.Visible = false;
		CharSprite2.Visible = false;

		// DEBUGGING 
		InitializeAnimation(CharacterType.Dragon);
		// DEBUGGING
    }

    public override void _PhysicsProcess(double delta)
	{
		//Must be intialized first InitializeAnimation()
		if (isInitialized){

			// Detect input and start new action
			
			if (!actionQueued && Input.IsKeyPressed(Key.Shift)){ //Bite
				actionQueued = true;

				if (actionType != ActionStateType.draBite){
					GD.Print("Calling bite");
					SetAnim(ActionStateType.draBite);
				}

			} else if (actionQueued && Input.IsKeyPressed(Key.Shift) == false){
				
				// check if animation needs to play out before returning to movable state
				actionQueued = false;
				requestActionCancel = true;
				
			}
			
			AnimTimer();
			HandleMovement(delta);
		}
	}

	public void InitializeAnimation(CharacterType type_){
		if (type_ == CharacterType.none){
			GD.Print("Tried to initialize animation with no CharacterType.");
			return;
		}

		switch(type_){
			case CharacterType.Dragon:
			
				CharType = type_;
				SetAnim(ActionStateType.draIdle);
				break;

			default:
				GD.Print("Tried to initialize animation but " + type_.ToString() + " is not yet supported.");
				return;
		}

		CharSprite1.Visible = true;
		isInitialized = true;
	}

	void AnimTimer(){
		if (animBool){
			//stop counter till CountTime() is finished
			animBool = false;

			CountTime();
		}
	}
	async void CountTime(){

		bool actionChanged = false;
		

		await ToSignal(GetTree().CreateTimer(0.08f), "timeout");
		
		HandleAnim();

		if (lockDirection){
			if (!actionQueued && requestActionCancel){
				if (animClock + 1 >= CharSprite1.Hframes){
					actionQueued = false;
					requestActionCancel = false;
					lockDirection = false;
					actionChanged = true;
					
					SetAnim(ActionStateType.draWalk);
				}
			} 
		}

		// set sprite frame
		if (!actionChanged) {
			if (animClock + 1 >= CharSprite1.Hframes) animClock = 0;
			else animClock++;
		} else animClock = 0;


		//allow counter to be called again
		animBool = true;
	}
	public void HandleMovement(double delta){

		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			if (!lockDirection){
				
				if (actionType != ActionStateType.draWalk)
					SetAnim(ActionStateType.draWalk);

				velocity.X = direction.X * WalkSpeed;
				velocity.Y = direction.Y * WalkSpeed;
			} else {
				velocity.X = direction.X * WalkSpeed * 0.1f;
				velocity.Y = direction.Y * WalkSpeed * 0.1f;
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, WalkSpeed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, WalkSpeed);
		}
		
		Velocity = velocity;
		MoveAndSlide();
	}
	void HandleAnim(){
		
		//DEBUG
		GD.Print(actionType.ToString() + " " + animClock.ToString());
		//DEBUG

		// is input zero
		if (direction.X > -0.10f && direction.X < 0.1f && direction.Y > -0.1 && direction.Y < 0.1f){
			
			//zero detected :: -0.70710677, -0.70710677
			if (CharSprite1.FrameCoords.Y == 0){
				CharSprite1.Frame = animClock;
			} else
				CharSprite1.Frame = CharSprite1.FrameCoords.Y * CharSprite1.Hframes + animClock;
			
			return;
		}

		//is character allowed to turn
		if (lockDirection){
			if (previousFrameY == 0)
				CharSprite1.Frame = animClock;
			else
				CharSprite1.Frame = previousFrameY * CharSprite1.Hframes + animClock;
			return;
		}

		// find direction
		if (direction.X > -0.55f && direction.X < 0.55f){  // up down
				CharSprite1.FlipH = false;
			if (direction.Y > 0.2f){ // down
				CharSprite1.Frame = animClock;
			} else if (direction.Y < -0.2f){ // up
				CharSprite1.Frame = 4 * CharSprite1.Hframes + animClock;
			}
		} else if (direction.X < -0.55f){ // left sides
				CharSprite1.FlipH = false;

			if (direction.Y > 0.2f){ // downLeft
				CharSprite1.Frame = CharSprite1.Hframes + animClock;

			} else if (direction.Y < -0.2f){ // upLeft
				CharSprite1.Frame = 3 * CharSprite1.Hframes + animClock;

			} else { // Left
				CharSprite1.Frame = 2 * CharSprite1.Hframes + animClock;
			}

		} else if (direction.X > 0.55f){ // right sides
				CharSprite1.FlipH = true;
			if (direction.Y > 0.2f){ // downRight
				CharSprite1.Frame = CharSprite1.Hframes + animClock;
			
			} else if (direction.Y < -0.2f){ // upRight
				CharSprite1.Frame = 3 * CharSprite1.Hframes + animClock;
			
			} else { // Right
				CharSprite1.Frame = 2 * CharSprite1.Hframes + animClock;
			}
		}
		previousFrameY = CharSprite1.FrameCoords.Y;
	}
	void SetAnim(ActionStateType type){
		// Changes the amount of Horizontal frames/sets the length of the animation
		// each spritesheet has 5 sets one for each direction this never changes

		if (CharType == CharacterType.Dragon){

			switch(type) {
				case ActionStateType.draIdle:

					GD.Print("idle called!");
					
					//DEBUG
					// remove this once an idle anim is set
					lockDirection = false;
					actionType = ActionStateType.draWalk;
					CharSprite1.Texture = GD.Load<Texture2D>(_ROOT_PATH + "/Dragon/Walk/walk_draBody1.PNG");
					CharSprite1.Hframes = 20;
					//DEBUG

					break;
				case ActionStateType.draWalk:

					GD.Print("Walk called.");

					lockDirection = false;
					actionType = ActionStateType.draWalk;
					CharSprite1.Texture = GD.Load<Texture2D>(_ROOT_PATH + "/Dragon/Walk/walk_draBody1.PNG");
					CharSprite1.Hframes = 20;
					break;
				case ActionStateType.draBite:

					GD.Print("Bite called.");

					animClock = 0;
					lockDirection = true;
					actionType = ActionStateType.draBite;
					CharSprite1.Texture = GD.Load<Texture2D>(_ROOT_PATH + "/Dragon/Bite/bite_draBody1.png");
					CharSprite1.Hframes = 10;
					break;
				case ActionStateType.draClaw:

					GD.Print("Claw called.");
					
					animClock = 0;
					lockDirection = true;
					actionType = ActionStateType.draClaw;
					CharSprite1.Hframes = 20;
					break;
			}

		} else if (CharType == CharacterType.Player){
			GD.Print("Nothing written for player yet.");
		}
	}
}
public enum ActionStateType {
		none,

		// (dra)gon
		draIdle,
		draWalk,
		draRun,
		draBite,
		draClaw
	};
	public enum AnimType {
		none,

		// (dra)gon
		draBody1, // body type 1
		draFurnace1,
		draWings1

	}
	public enum CharacterType {
		none,
		Dragon,
		Player
	}
