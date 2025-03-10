﻿using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Set in Inspector")]
    //This is an unusual but handy use of Vector2s. x holds a min value
    // and y a max value for a Random.Range() that will be called later
    public Vector2 rotMinMax = new Vector2(15, 90);
    public Vector2 driftMinMax = new Vector2(0.25f, 2);
    public float lifeTime = 6f; //Seconds the PowerUp exists
    public float fadeTime = 4f; //Seconds until it will fade

    [Header("Set Dynamically")]
    public WeaponType type; // They type of the PowerUp
    public GameObject cube; //Reference to the Cube child
    public TextMesh letter; // Reference to the TextMesh
    public Vector3 rotPerSecond; //Euler rotation speed
    public float birthTime;

    private Rigidbody rigid;
    private BoundsCheck bndCheck;
    private Renderer cubeRend;

    void Awake()
    {
        //Find the Cube reference
        cube = transform.Find("Cube").gameObject;
        //Find the TextMesh and other components
        letter = GetComponent<TextMesh>();
        rigid = GetComponent<Rigidbody>();
        bndCheck = GetComponent<BoundsCheck>();
        cubeRend = cube.GetComponent<Renderer>();

        //Set a random velocity
        Vector3 vel = Random.onUnitSphere; //Get Random XYZ velocity
        //Random.onUnitSphere gives you a vector point that is somewhere on
        // the surface of the sphere with a radius of 1m around the origin
        vel.z = 0; //Flatten the vel to the XY plane
        vel.Normalize(); //Normalize a Vector3 makes it length 1m
        vel *= Random.Range(driftMinMax.x, driftMinMax.y);
        rigid.linearVelocity = vel;

        //Set the rotation of this GameObject to R:[0, 0, 0]
        transform.rotation = Quaternion.identity;
        //Quaternion.identity is equal to no rotation

        //Set up the rotPerSecond for the Cube child using rotMinMax x and y
        rotPerSecond = new Vector3(Random.Range(rotMinMax.x, rotMinMax.y),
            Random.Range(rotMinMax.x, rotMinMax.y),
            Random.Range(rotMinMax.x, rotMinMax.y));
        birthTime = Time.time;
    }
    void Update()
    {
        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time);

        //Fade out the PowerUp over time
        //Given the default values, a PowerUp will exist for 10 seconds
        // and then fade out over 4 seconds
        float u = (Time.time - (birthTime + lifeTime)) / fadeTime;
        //For lifeTime seconds, u will be <= 0. Then it will transition to 
        // 1 over the course of fadeTime seconds.

        //If u >= 1, Destroy this powerup
        if (u >= 1)
        {
            Destroy(this.gameObject);
            return;
        }
        //Use u to determine the alpha value of the cube and letter
        if (u > 0)
        {
            Color c = cubeRend.material.color;
            c.a = 1f - u;
            cubeRend.material.color = c;
            //Fade the Letter too, just not as much
            c = letter.color;
            c.a = 1f - (u * 0.5f);
            letter.color = c;
        }
        if (!bndCheck.isOnScreen)
        {
            //If the powerup has drifted entirely off screen, destroy it
            Destroy(gameObject);
        }
    }
    public void SetType(WeaponType wt)
    {
        //Grab the WeaponDefinition from Main
        WeaponDefinition def = Main.GetWeaponDefinition(wt);
        //Set the color of the cube child
        cubeRend.material.color = def.color;
        letter.text = def.letter; //Set the letter that is shown
        type = wt;  //Finally actually set the type
    }
    public void AbsorbedBy(GameObject target)
    {
        if (type == WeaponType.nuke) {
            Main.S.DestroyAllEnemies();
        }

        Destroy(this.gameObject);
    }
}
