using System;
using Godot;

public partial class Player : CharacterBody2D
{
	bool dbool = false;
	int dint = 0; // DEBUG int for print messages

	// AnimationSignal animSignal;

	//Sprite 1&2 will switch on and off to change state without flickering
	bool isSprite1 = true;
	bool spriteSwapped = false; // stops animClock from changing values bc they are being set manually
	public Sprite2D CharSprite1;
	public Sprite2D CharSprite2;
	ActionStateType actionType;
	Vector2 direction = Vector2.Zero;

	public CharacterType CharType {get; private set;}
	public CharacterType CharType_ { get { return CharType; } }

	private const string _ROOT_PATH = "res://assets/sprite_sheets";

	int animClock = 0;
	bool couterPause = true;

	// used to set direction if sprite changed but prev angle is needed
	int previousFrameY = 0;

	// for attacks that need to play out before allowing changed action
	// speed will be reduced speed * 0.2f
	bool lockDirection = false; 

	// set to true if other actions should not be permitted
	bool actionQueued = false;
	// set to true if actionQueued should be canceled as soon as possible
	// example: combo attack 2 is not called after combo attack 1
	bool requestActionCancel = false;

	//Animation will begin after values are set in InitializeAnimation()
	bool isInitialized = false;

	// INPUT BOOLS
	// when set to true this input will be called on next update
	bool callIdle, callWalk, callRun, callAtt1, callAtt2, callAtt3, callAtt4, callDodge, callDie;

	public float WalkSpeed = 60.0f;

	/*
						// INSTRUCTIONS //
		Call this Method to set the player type and start the script
		 --> InitializeAnimation(CharacterType type)

	*/

	public override void _Ready()
	{
		// initialize
		CharSprite1 = GetNode<Sprite2D>("/root/Game/BossPlayer/Sprite1");
		CharSprite2 = GetNode<Sprite2D>("/root/Game/BossPlayer/Sprite2");
		CharSprite1.Visible = false;
		CharSprite1.Vframes = 5;
		CharSprite2.Visible = false;
		CharSprite2.Vframes = 5;

		// DEBUGGING 
		InitializeAnimation(CharacterType.Dragon);
		// DEBUGGING
	}

	public override void _PhysicsProcess(double delta)
	{
		//Must be intialized first InitializeAnimation()
		if (isInitialized){

			CheckInputBools();
			AnimTimer();
			HandleMovement(delta);
		}
	}

	public void InitializeAnimation(CharacterType type){
		if (type == CharacterType.none){
			GD.Print("Tried to initialize animation with no CharacterType.");
			return;
		}

		switch(type){

			case CharacterType.Dragon:

				CharType = type;
				SetAnim(ActionStateType.idle);
				WalkSpeed = 60;

				isInitialized = true;
				break;

			case CharacterType.Player:

				CharType = type;
				SetAnim(ActionStateType.idle);
				WalkSpeed = 120;

				isInitialized = true;
				break;
			default:
				GD.Print("Tried to initialize animation but " + type.ToString() + " is not yet supported.");
				return;
		}
	}

	void AnimTimer(){
		if (couterPause){
			//stop counter till CountTime() is finished
			couterPause = false;

			CountTime();
		}
	}
	
	void CheckInputBools(){

		// This should be coppied to be identical to the "ActionStateType_Decoder()" switch() list
		 bool resetBools = false;

		// ReadInput
		if (Input.IsActionPressed("Attack1")){
			callAtt1 = true;
		} else if (Input.IsActionPressed("Attack2")){
			callAtt2 = true;
		}

		if (CharType == CharacterType.Dragon){
			if (callIdle){

				resetBools = true;
			} else if ( callWalk){

				resetBools = true;
			} else if (callRun){

				resetBools = true;
			} else if (callAtt1){

				if (actionType != ActionStateType.attack1 && !lockDirection){
					SetAnim(ActionStateType.attack1);
					requestActionCancel = true;
					actionQueued = false;

				} else if (requestActionCancel == false && actionType == ActionStateType.attack1) {
					actionQueued = true;
				}

				resetBools = true;
			} else if (callAtt2){

				if (actionType != ActionStateType.attack2 && !lockDirection){
					SetAnim(ActionStateType.attack2);
					requestActionCancel = true;
					actionQueued = false;

				} else if (requestActionCancel == false && actionType == ActionStateType.attack2) {
					actionQueued = true;
				}

				resetBools = true;
			} else if (callAtt3){

				resetBools = true;
			} else if (callAtt4){

				resetBools = true;
			} else if (callDodge){

				resetBools = true;
			} else if (callDie){

				resetBools = true;
			}
		}

		if (resetBools){
			callIdle = false;
			callWalk = false;
			callRun = false;
			callAtt1 = false;
			callAtt2 = false;
			callAtt3 = false;
			callAtt4 = false;
			callDodge = false;
			callDie = false;
		}
	}

	async void CountTime(){

		bool actionChanged = false;
		
		await ToSignal(GetTree().CreateTimer(0.08f), "timeout");
		
		// Sprite to handle
		Sprite2D setSprite;
		if (isSprite1) setSprite = CharSprite1;
		else setSprite = CharSprite2;

		if (lockDirection){
			if (!actionQueued && requestActionCancel){
				
				if (animClock + 1 >= setSprite.Hframes){
					actionQueued = false;
					requestActionCancel = false;
					lockDirection = false;
					actionChanged = true;
					
					SetAnim(ActionStateType.idle);

				} 
			} else if (requestActionCancel == false) {
				requestActionCancel = true;
			}
		}
		HandleAnim();

		// set sprite frame
		if (!actionChanged) {
			if (animClock + 1 >= setSprite.Hframes) animClock = 0;
			else animClock++;
		} else animClock = 0;


		//allow counter to be called again
		couterPause = true;
	}
	public void HandleMovement(double delta){

		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			if (!lockDirection){
				
				if (actionType != ActionStateType.walk)
					SetAnim(ActionStateType.walk);

				velocity.X = direction.X * WalkSpeed;
				velocity.Y = direction.Y * WalkSpeed;
			} else {
				velocity.X = direction.X * WalkSpeed * 0.2f;
				velocity.Y = direction.Y * WalkSpeed * 0.2f;
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

		// Sprite to handle
		Sprite2D setSprite;
		if (isSprite1) setSprite = CharSprite1;
		else setSprite = CharSprite2;

		// is input zero
		if (direction.X > -0.10f && direction.X < 0.1f && direction.Y > -0.1 && direction.Y < 0.1f){
			
			//zero detected :: -0.70710677, -0.70710677
			if (setSprite.FrameCoords.Y == 0){
				
				if (isSprite1){
				CharSprite1.Frame = animClock;
				CharSprite2.Frame = 0;
				} else {
				CharSprite1.Frame = 0;
				CharSprite2.Frame = animClock;
				}
			} else{
				if (isSprite1) {
				CharSprite1.Frame = CharSprite1.FrameCoords.Y * CharSprite1.Hframes + animClock;
				CharSprite2.Frame = CharSprite2.FrameCoords.Y * CharSprite2.Hframes;
				} else {
				CharSprite1.Frame = CharSprite1.FrameCoords.Y * CharSprite1.Hframes;
				CharSprite2.Frame = CharSprite2.FrameCoords.Y * CharSprite2.Hframes + animClock;
				}
			}
			return;
		}

		//is character allowed to turn
		if (lockDirection){
			if (previousFrameY == 0){
				if (isSprite1){
				CharSprite1.Frame = animClock;
				CharSprite2.Frame = 0;
				} else {
				CharSprite1.Frame = 0;
				CharSprite2.Frame = animClock;
				}
				}
			else
				if (isSprite1){
				CharSprite1.Frame = previousFrameY * CharSprite1.Hframes + animClock;
				CharSprite2.Frame = previousFrameY * CharSprite2.Hframes;
				} else {
				CharSprite1.Frame = previousFrameY * CharSprite1.Hframes;
				CharSprite2.Frame = previousFrameY * CharSprite2.Hframes + animClock;
				}
			return;
		}

		// find direction
		if (direction.X > -0.55f && direction.X < 0.55f){  // up down
				CharSprite1.FlipH = false;
				CharSprite2.FlipH = false;
			if (direction.Y > 0.2f){ // down
				if (isSprite1) {
				CharSprite1.Frame = animClock;
				CharSprite2.Frame = 0;
				} else {
				CharSprite1.Frame = 0;
				CharSprite2.Frame = animClock;
				}
			} else if (direction.Y < -0.2f){ // up
				if (isSprite1) {
					CharSprite1.Frame = 4 * CharSprite1.Hframes + animClock;
					CharSprite2.Frame = 4 * CharSprite2.Hframes;
				} else {
					CharSprite1.Frame = 4 * CharSprite1.Hframes;
					CharSprite2.Frame = 4 * CharSprite2.Hframes + animClock;
				}
			}
		} else if (direction.X < -0.55f){ // left sides
				CharSprite1.FlipH = false;
				CharSprite2.FlipH = false;

			if (direction.Y > 0.2f){ // downLeft

				if (isSprite1){
					CharSprite1.Frame = CharSprite1.Hframes + animClock;
					CharSprite2.Frame = CharSprite2.Hframes;
				} else {
					CharSprite1.Frame = CharSprite1.Hframes;
					CharSprite2.Frame = CharSprite2.Hframes + animClock;
				}
			} else if (direction.Y < -0.2f){ // upLeft

				if (isSprite1){
				CharSprite1.Frame = 3 * CharSprite1.Hframes + animClock;
				CharSprite2.Frame = 3 * CharSprite2.Hframes;
				} else {
				CharSprite1.Frame = 3 * CharSprite1.Hframes;
				CharSprite2.Frame = 3 * CharSprite2.Hframes + animClock;
				}
			} else { // Left
			
				if (isSprite1) {
				CharSprite1.Frame = 2 * CharSprite1.Hframes + animClock;
				CharSprite2.Frame = 2 * CharSprite2.Hframes;
				} else {
				CharSprite1.Frame = 2 * CharSprite1.Hframes;
				CharSprite2.Frame = 2 * CharSprite2.Hframes + animClock;
				}
			}

		} else if (direction.X > 0.55f){ // right sides
				CharSprite1.FlipH = true;
				CharSprite2.FlipH = true;
			if (direction.Y > 0.2f){ // downRight
			
				if (isSprite1) {
				CharSprite1.Frame = CharSprite1.Hframes + animClock;
				CharSprite2.Frame = CharSprite2.Hframes;
				} else {
				CharSprite1.Frame = CharSprite1.Hframes;
				CharSprite2.Frame = CharSprite2.Hframes + animClock;
				}
			
			} else if (direction.Y < -0.2f){ // upRight
				
				if (isSprite1){
				CharSprite1.Frame = 3 * CharSprite1.Hframes + animClock;
				CharSprite2.Frame = 3 * CharSprite2.Hframes;
				} else {
				CharSprite1.Frame = 3 * CharSprite1.Hframes;
				CharSprite2.Frame = 3 * CharSprite2.Hframes + animClock;
				}
			} else { // Right
				 
				if (isSprite1){
				CharSprite1.Frame = 2 * CharSprite1.Hframes + animClock;
				CharSprite2.Frame = 2 * CharSprite2.Hframes;
				} else {
				CharSprite1.Frame = 2 * CharSprite1.Hframes;
				CharSprite2.Frame = 2 * CharSprite2.Hframes + animClock;
				}
			}
		}
		previousFrameY = setSprite.FrameCoords.Y;
	}
	void SetAnim(ActionStateType type){
		// Changes the amount of Horizontal frames/sets the length of the animation
		// each spritesheet has 5 sets one for each direction this never changes

		if (CharType == CharacterType.Dragon){

			// Sprite to Alternate
			Sprite2D setSprite = CharSprite1;

			switch(type) {
				case ActionStateType.idle:
					
					//DEBUG
					// remove this once an idle anim is set

					if (actionType != ActionStateType.idle){
						actionType = ActionStateType.idle;

						CharSprite1.Visible = false;
						CharSprite2.Visible = false;
						// Alternate Sprite
						if (isSprite1){
							isSprite1 = false;
							setSprite = CharSprite2;
						} else {
							isSprite1 = true;
							setSprite = CharSprite1;
						}

						lockDirection = false;
						spriteSwapped = true;
						setSprite.Texture = GD.Load<Texture2D>(_ROOT_PATH + "/Dragon/Walk/walk_draBody1.PNG");
						setSprite.Hframes = 20;

						if (isSprite1 == false){
							
							CharSprite1.Visible = false;
							CharSprite2.Visible = true;
						} else {
							
							CharSprite2.Visible = false;
							CharSprite1.Visible = true;
						}
					}
					//DEBUG

					break;
				case ActionStateType.walk:

					if (actionType != ActionStateType.walk){
						actionType = ActionStateType.walk;

						CharSprite1.Visible = false;
						CharSprite2.Visible = false;
						// Alternate Sprite
						if (isSprite1){
							isSprite1 = false;
							setSprite = CharSprite2;
						} else {
							isSprite1 = true;
							setSprite = CharSprite1;
						}

						lockDirection = false;
						spriteSwapped = true;
						setSprite.Texture = GD.Load<Texture2D>(_ROOT_PATH + "/Dragon/Walk/walk_draBody1.PNG");
						setSprite.Hframes = 20;

						if (isSprite1 == false){
							
							CharSprite1.Visible = false;
							CharSprite2.Visible = true;
						} else {
							
							CharSprite2.Visible = false;
							CharSprite1.Visible = true;
						}
					}
					break;
				case ActionStateType.attack1:

					if (actionType != ActionStateType.attack1){
						actionType = ActionStateType.attack1;

						CharSprite1.Visible = false;
						CharSprite2.Visible = false;

						// Alternate Sprite
						if (isSprite1){
							isSprite1 = false;
							setSprite = CharSprite2;
						} else {
							isSprite1 = true;
							setSprite = CharSprite1;
						}

						animClock = 0;
						lockDirection = true;
						spriteSwapped = true;
						setSprite.Texture = GD.Load<Texture2D>(_ROOT_PATH + "/Dragon/Bite/bite_draBody1.png");
						setSprite.Hframes = 10;

						if (isSprite1 == false){
							
							CharSprite1.Visible = false;
							CharSprite2.Visible = true;
						} else {
							
							CharSprite2.Visible = false;
							CharSprite1.Visible = true;
						}
					}
					break;
				case ActionStateType.attack2:
					
					if (actionType != ActionStateType.attack2){
						actionType = ActionStateType.attack2;

						CharSprite1.Visible = false;
						CharSprite2.Visible = false;
						// Alternate Sprite
						if (isSprite1){
							isSprite1 = false;
							setSprite = CharSprite2;
						} else {
							isSprite1 = true;
							setSprite = CharSprite1;
						}

						animClock = 0;
						lockDirection = true;
						spriteSwapped = true;
						setSprite.Texture = GD.Load<Texture2D>(_ROOT_PATH + "/Dragon/Claw/claw_draBody1.png");
						setSprite.Hframes = 20;

						if (isSprite1 == false){
							
							CharSprite1.Visible = false;
							CharSprite2.Visible = true;
						} else {
							
							CharSprite2.Visible = false;
							CharSprite1.Visible = true;
						}
					}
					break;
			}

		} else if (CharType == CharacterType.Player){
			GD.Print("Nothing written for player yet.");
		}
	}

	// Converts the ActinStateType into a reliably predictable number so 
	//		the enum values can be added and rearanged without messing up anything
	// If a new ActionStateType is added add it to both these Decoders
	public ActionStateType ActionStateType_Decoder(int type) {
		ActionStateType rValue = ActionStateType.none;

		switch(type){
			case 0:
				rValue = ActionStateType.none;
				break;
			case 1:
				rValue = ActionStateType.idle;
				break;
			case 2:
				rValue = ActionStateType.walk;
				break;
			case 3:
				rValue = ActionStateType.run;
				break;
			case 4:
				rValue = ActionStateType.attack1;
				break;
			case 5:
				rValue = ActionStateType.attack2;
				break;
			case 6:
				rValue = ActionStateType.attack3;
				break;
			case 7:
				rValue = ActionStateType.attack4;
				break;
			case 8:
				rValue = ActionStateType.dodge;
				break;
			case 9:
				rValue = ActionStateType.die;
				break;
		}
		return rValue;
	}
	public int ActionStateType_Decoder(ActionStateType type) {
		int rValue = -1;

		switch(type){
			case ActionStateType.none:
				rValue = 0;
				break;
			case ActionStateType.idle:
				rValue = 1;
				break;
			case ActionStateType.walk:
				rValue = 2;
				break;
			case ActionStateType.run:
				rValue = 3;
				break;
			case ActionStateType.attack1:
				rValue = 4;
				break;
			case ActionStateType.attack2:
				rValue = 5;
				break;
			case ActionStateType.attack3:
				rValue = 6;
				break;
			case ActionStateType.attack4:
				rValue = 7;
				break;
			case ActionStateType.dodge:
				rValue = 8;
				break;
			case ActionStateType.die:
				rValue = 9;
				break;
		}
		return rValue;
	}
}
public enum ActionStateType {
	// If a new ActionStateType is added add it to -both- of the ActionStateType_Decoder() methods
	// 		-AND- add to animBools at header in references
	//		-AND- add to CheckInputBools() switch() statement
	// all of these need to be written in the same order however the enums below can be organized in any order it doesnt matter

		//(ref) idle,walk, run, attack1, attack2,  attack3, attack4, dodge, die

		none,

		idle,
		walk,
		run,
		attack1, 	// dragon-(bite) player-
		attack2, 	// dragon-(claw) player-
		attack3,	// dragon- 		player-
		attack4, 	// dragon- 		player-
		dodge,
		die


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
