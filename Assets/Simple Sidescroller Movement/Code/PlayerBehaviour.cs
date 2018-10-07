using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour {
    public const int STATE_NONE = 0;
	public const int STATE_STAND = 1;
	public const int STATE_WALK = 2;
	public const int STATE_IN_AIR = 3;

    private int state = STATE_NONE;
	private Rigidbody2D body;
	private BoxCollider2D box;
	private SpriteRenderer mySpriteRenderer;
	private SimpleAnimator simpleAnimator;
	private RaycastHit2D[] groundCheckResults;
	private bool inGround;
	private bool hasJumped;
	private bool jumpCanceled;

	public float moveAcceleration;
	public Vector2 maxMoveVelocity;
	public float groundCheckDistance;
	public float iceStaticFriction;
	public float iceTurnDrag;
	public float jumpVelocity;
	public float jumpCancelFriction;

	void Awake(){
		body = GetComponent<Rigidbody2D>();
		box = GetComponent<BoxCollider2D>();
		mySpriteRenderer = GetComponent<SpriteRenderer>();
		simpleAnimator = GetComponent<SimpleAnimator>();
		groundCheckResults = new RaycastHit2D[2];
	}
    
    void Start () {
		SetState(STATE_STAND);
	}

	void FixedUpdate () {
		Vector2 velocity = body.velocity;
		inGround = false;
		int hits = 0;
		if(velocity.y <= 0){
			box.enabled = false;
			hits = Physics2D.BoxCastNonAlloc(new Vector2(transform.position.x, transform.position.y - groundCheckDistance) + box.offset, box.size, 0, Vector2.down, groundCheckResults, groundCheckDistance);
			box.enabled = true;
			for(int hitIndex = 0; hitIndex < hits; hitIndex += 1){
				if(groundCheckResults[hitIndex].collider != null && groundCheckResults[hitIndex].normal.x == 0){
					inGround = true;
					break;
				}
			}
		}
		bool lowFrictionSurface = false;
		if(inGround){
			for(int hitIndex = 0; hitIndex < hits; hitIndex += 1){
				if(groundCheckResults[hitIndex].normal.x == 0 && groundCheckResults[hitIndex].collider.gameObject.name == "Ice"){
					lowFrictionSurface = true;
					break;
				}
			}		
		}
		if(Input.GetAxisRaw("Horizontal") == 0){
			if(inGround){
				if(lowFrictionSurface){
					velocity.x *= iceStaticFriction;
				}
				else{
					velocity.x = 0;
				}
			}
			else{
				velocity.x = 0;
			}
		}
		else{
			if(velocity.x != 0 && Mathf.Sign(velocity.x) != Mathf.Sign(Input.GetAxisRaw("Horizontal"))){
				if(lowFrictionSurface){
					velocity.x += moveAcceleration * Input.GetAxisRaw("Horizontal") * iceTurnDrag;
				}
				else{
					velocity.x = moveAcceleration * Input.GetAxisRaw("Horizontal");
				}
				
			}
			else{
				velocity.x += moveAcceleration * Input.GetAxisRaw("Horizontal");
			}
			if(velocity.x > maxMoveVelocity.x){
				velocity.x = maxMoveVelocity.x;
			}
			else if(velocity.x < -maxMoveVelocity.x){
				velocity.x = -maxMoveVelocity.x;
			}
		}
		if(state == STATE_IN_AIR && hasJumped && velocity.y >= 0){
			if(jumpCanceled){
				velocity.y *= jumpCancelFriction;
			}
			else if(!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.U)){
				jumpCanceled = true;
			}
		}
		if(velocity.y > maxMoveVelocity.y){
			velocity.y = maxMoveVelocity.y;
		}
		else if(velocity.y < -maxMoveVelocity.y){
			velocity.y = -maxMoveVelocity.y;
		}
		body.velocity = velocity;
	}

    void Update () {
		if(Input.GetAxisRaw("Horizontal") != 0){
			mySpriteRenderer.flipX = Input.GetAxisRaw("Horizontal") > 0 ? false : true;
		}
		switch(state){
			case STATE_STAND:{
				if(!inGround || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.U)){
					SetState(STATE_IN_AIR);
				}
				else if(Input.GetAxisRaw("Horizontal") != 0){
					SetState(STATE_WALK);
				}
				break;
			}
			case STATE_WALK:{
				if(!inGround || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.U)){
					SetState(STATE_IN_AIR);
				}
				else if(Input.GetAxisRaw("Horizontal") == 0){
					SetState(STATE_STAND);
				}
				break;
			}
			case STATE_IN_AIR:{
				if(inGround){
					if(Input.GetAxisRaw("Horizontal") == 0){
						SetState(STATE_STAND);
					}
					else {
						SetState(STATE_WALK);
					}
				}
				break;
			}
		}
    }

	void SetState(int nextState){
		state = nextState;
		switch(state){
			case STATE_STAND:{
				simpleAnimator.SetAnimation("stand");
				break;
			}
			case STATE_WALK:{
				simpleAnimator.SetAnimation("walk");
				break;
			}
			case STATE_IN_AIR:{
				simpleAnimator.SetAnimation("in air");
				if(inGround){
					hasJumped = true;
					jumpCanceled = false;
					body.velocity = new Vector2(body.velocity.x, jumpVelocity);
					inGround = false;
				}
				else{
					hasJumped = false;
				}
				break;
			}
		}
	}
}