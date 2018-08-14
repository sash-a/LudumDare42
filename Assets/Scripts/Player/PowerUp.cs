using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {

    public enum Type { FIRE_RATE, HEALTH, SPEED ,SHOTGUN};
    public Type type;

	// Use this for initialization
	void Start () {
        type = (Type)(UnityEngine.Random.Range(0, 4));
        SpriteRenderer render = GetComponent<SpriteRenderer>();
        if (type.Equals(Type.FIRE_RATE)) {
            render.sprite = Resources.Load<Sprite>("Sprites/FireRatePowerup");
        }
        if (type.Equals(Type.HEALTH))
        {
            render.sprite = Resources.Load<Sprite>("Sprites/HealthPowerup");
        }
        if (type.Equals(Type.SPEED))
        {
            render.sprite = Resources.Load<Sprite>("Sprites/SpeedPowerup");
        }
        if (type.Equals(Type.SHOTGUN))
        {
            render.sprite = Resources.Load<Sprite>("Sprites/StrengthPowerup");
        }

    }

    // Update is called once per frame
    void Update () {
		
	}
}
