using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Part
{
    //These three fields need to be defined in the inspector pane
    public string name; //The name of this part
    public float health; //The amount of health this part has
    public string[] protectedBy;  //The other parts that protect this

    //These two fields are set automatically in Start().
    //Caching like this makes it faster and easier to find these later
    [HideInInspector] //Makes field on the next line not appear in the INspector
    public GameObject go; //The GameObject of this part
    [HideInInspector]
    public Material mat; //The Material to show damge
}

public class Enemy_4 : Enemy
{
    [Header("Set in Inspector: Enemy_4")]
    public Part[] parts; //The array of ship parts

    private Vector3 p0, p1; //The two points to interpolate
    private float timeStart; //birth time for this enemy
    private float duration = 4; //Duration of movement
    void Start()
    {
        //There is already an initial position chosen by Mina.SpawnEnemy()
        // so add it to points as the initial p0 and p1
        p0 = p1 = pos;

        InitMovement();

        //Cache GameObject and Material of each Part in parts
        Transform t;
        foreach(Part prt in parts)
        {
            t = transform.Find(prt.name);
            if(t != null)
            {
                prt.go = t.gameObject;
                prt.mat = prt.go.GetComponent<Renderer>().material;
            }
        }
    }
    void InitMovement()
    {
        p0 = p1; //Set p0 to the old p1
        //Assign a new on screen location to p1
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        //Reset the time
        timeStart = Time.time;
    }
    public override void Move()
    {
        //This completely overrides Enemy.Move() with a linear interpolation
        float u = (Time.time - timeStart) / duration;

        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }
        u = 1 - Mathf.Pow(1 - u, 2); // Apply Ease Out easing to u
        pos = (1 - u) * p0 + u * p1; //Simple linear interpolation
    }
    //These two function find a part in parts based on name or GameObject
    Part FindPart(string n)
    {
        foreach(Part prt in parts)
        {
            if(prt.name == n)
            {
                return (prt);
            }
        }
        return (null);
    }
    Part FindPart(GameObject go)
    {
        foreach(Part prt in parts)
        {
            if(prt.go == go)
            {
                return (prt);
            }
        }
        return (null);
    }
    //These functions return true if the Part has been destroyed
    bool Destroyed(GameObject go)
    {
        return (Destroyed(FindPart(go)));
    }
    bool Destroyed(string n)
    {
        return (Destroyed(FindPart(n)));
    }
    bool Destroyed(Part prt)
    {
        if(prt == null) //If no real ph was passed in
        {
            return (true); //return true (meaning yes, it was destroyed)
        }
        //Returns the result of the comparison: prt.health <= 0
        //If prt.health is 0 or less, returns true (destroyed)
        return (prt.health <= 0);
    }
    //This changes the color of just one Part to red instead of the whole ship
    void ShowLocalizedDamage(Material m)
    {
        m.color = Color.red;
        damageDoneTime = Time.time + showDamageDuration;
        showingDamage = true;
    }
    //This will override the OnCollisionEnter that is of Enemy.cs
    void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();
                //If this Enemy is off screen, dont damage it
                if (!bndCheck.isOnScreen)
                {
                    Destroyed(other);
                    break;
                }
                //Hurt this enemy
                GameObject goHit = coll.contacts[0].thisCollider.gameObject;
                Part prtHit = FindPart(goHit);
                if(prtHit == null) //If prt wasnt found
                {
                    goHit = coll.contacts[0].otherCollider.gameObject;
                    prtHit = FindPart(goHit);
                }
                //Check whether this part is still protected
                if(prtHit.protectedBy != null)
                {
                    foreach(string s in prtHit.protectedBy)
                    {
                        //If one of the protecting parts hasnt been destroyed
                        if (!Destroyed(s))
                        {
                            //then dont damge this part yet
                            Destroyed(other); //Destroy the ProjectileHero
                            return; //return before damaging enemy4
                        }
                    }
                }
                //Its not protected, so make it take damage
                //Gets the damage amount from the Projectile.type and Main.W_DEFS
                prtHit.health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                //Show damage on part
                ShowLocalizedDamage(prtHit.mat);
                if(prtHit.health <= 0)
                {
                    //Instead of destroying this enemy, disable the damaged part
                    prtHit.go.SetActive(false);
                }
                //Check to see if the whole ship is destroyed
                bool allDestroyed = true; //assume it is destroyed
                foreach(Part prt in parts)
                {
                    if (!Destroyed(prt)) //If a part still exists
                    {
                        allDestroyed = false;//cahnge allDestroyed to false
                        break; // and break out of the foreach loop
                    }
                }
                if (allDestroyed) //If it is completely destroyed
                {
                    //Tell the Main Singleton that this ship was destroyed
                    Main.S.ShipDestroyed(this);
                    //Destroy this ship
                    Destroy(this.gameObject);
                }
                Destroy(other); //Destroy projectile
                break;
        }
    }
}
