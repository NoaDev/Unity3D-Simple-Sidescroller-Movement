using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimator : MonoBehaviour {
	private SpriteRenderer mySpriteRenderer;
	[SerializeField]
	private SimpleAnimation[] animations;
	private SimpleAnimation currentAnimation;
	private int currentFrame; 
	private int stepCount;

	void Awake(){
		mySpriteRenderer = GetComponent<SpriteRenderer>();
	}

	void FixedUpdate(){
		if(currentAnimation != null){
			if(currentFrame < currentAnimation.frames.Length - 1 || currentAnimation.loop){
				stepCount += 1;
				if(stepCount >= currentAnimation.steps){
					currentFrame += 1;
					if(currentFrame == currentAnimation.frames.Length){
						currentFrame = 0;
					}
					mySpriteRenderer.sprite = currentAnimation.frames[currentFrame];
					stepCount = 0;
				}
			}
		}
	}

	public void SetAnimation(string name){
		for(int animationIndex = 0; animationIndex < animations.Length; animationIndex += 1){
			if(animations[animationIndex].name == name){
				currentAnimation = animations[animationIndex];
				mySpriteRenderer.sprite = currentAnimation.frames[0];
				currentFrame = 0;
				stepCount = 0;
			}
		}
	}
}

[System.Serializable]
public class SimpleAnimation{
	public string name;
	public Sprite[] frames;
	public int steps;
	public bool loop;
}
