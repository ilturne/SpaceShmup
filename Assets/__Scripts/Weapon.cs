using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an enum of the various possible weapon types
/// It also includes a "shield" type tp allow a shield power-up
/// Items marked [NI] below anr Not Implemented in the IGDPD book.
/// </summary>
public enum WeaponType
{
    none,       //The default / no weapon
    blaster,    //A simple blaster
    spread,     //Two shots simultaneously
    phaser,     //[NI] Shots that move in waves
    missile,    //[NI] Homing missles
    laser,      //[Ni] Damage over time
    shield,      //Raise shieldLevel
    nuke        //Destroys all enemies on screen
}
/// <summary>
/// The WeaponDefinition class allows you to set the properties
///     of a specific weapon in the Inspector. The Main class has
///     an array of WeaponDefinitions that makes this possible.
/// </summary>
[System.Serializable]
public class WeaponDefinition
{
    public WeaponType type = WeaponType.none;
    public string letter; //Letter to show on the power-up                     
    public Color color = Color.white; //Color of Collar and power-up
    public GameObject projectilePrefab; // Prefab for projectiles
    public Color projectileColor = Color.white;
    public float damageOnHit = 0; // Amount of damage caused
    public float continuousDamage = 0;  //Damage per second (laser)
    public float delayBetweenShots = 0; 
    public float velocity = 20; //SPeed of projectiles
}
public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Set Dynamically")]
    [SerializeField]
    private WeaponType _type = WeaponType.none;
    public WeaponDefinition def;
    public GameObject collar;
    public float lastShotTime; // Time last shot was fired
    private Renderer collarRend;

    void Start()
    {
        collar = transform.Find("Collar").gameObject;
        collarRend = collar.GetComponent<Renderer>();

        // Call SetType() for the default _type of WeaponType.none
        SetType(_type);

        // Dynamically create an anchor for all Projectiles
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }
        // Find the fireDelegate of the root GameObject
        GameObject rootGO = transform.root.gameObject;
        if (rootGO.GetComponent<Hero>() != null)
        {
            rootGO.GetComponent<Hero>().fireDelegate += Fire;
        }
    }
    public WeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }
    public void SetType(WeaponType wt)
    {
        _type = wt;
        if (type == WeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
        def = Main.GetWeaponDefinition(_type);
        collarRend.material.color = def.color;
        lastShotTime = 0;
    }
    public void Fire()
    {
        // If this.gameObject is inactive, return
        if (!gameObject.activeInHierarchy) return;
        // If it hasn't been enough time between shots, return
        if (Time.time - lastShotTime < def.delayBetweenShots)
        {
            return;
        }
        Projectile p;
        Vector3 vel = Vector3.up * def.velocity;
        if (transform.up.y < 0)
        {
            vel.y = -vel.y;
        }
        switch (type)
        {
            case WeaponType.blaster:
                p = MakeProjectile();
                p.rigid.linearVelocity = vel;
                break;

            case WeaponType.spread:
                // Middle projectile
                p = MakeProjectile();
                p.rigid.linearVelocity = vel;
                // Right projectile
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.rigid.linearVelocity = p.transform.rotation * vel;
                // Left projectile
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.rigid.linearVelocity = p.transform.rotation * vel;
                break;

            case WeaponType.phaser:
                // Phaser: could add wave-like motion behavior.
                p = MakeProjectile();
                p.rigid.linearVelocity = vel;
                // For example, add a component to handle sine-wave movement:
                // p.gameObject.AddComponent<PhaserProjectile>();
                break;

            case WeaponType.missile:
                // Missile: potentially slower with homing behavior.
                p = MakeProjectile();
                p.rigid.linearVelocity = vel * 0.8f; // slightly slower speed
                // Optionally, add a homing behavior script:
                // p.gameObject.AddComponent<MissileHoming>();
                break;

            case WeaponType.laser:
                // Laser: could be a continuous beam or a projectile that lingers.
                p = MakeProjectile();
                p.rigid.linearVelocity = vel;
                // Optionally, modify laser projectile properties such as lifetime or scale:
                // p.gameObject.AddComponent<LaserBehavior>();
                break;
            
            case WeaponType.nuke:
                p = MakeProjectile();
                p.rigid.linearVelocity = vel;
                break;
        }
    }
    public Projectile MakeProjectile()
    {
        GameObject go = Instantiate<GameObject>(def.projectilePrefab);
        if (transform.parent.gameObject.tag == "Hero")
        {
            go.tag = "ProjectileHero";
            go.layer = LayerMask.NameToLayer("ProjectileHero");
        }
        else
        {
            go.tag = "ProjectileEnemy";
            go.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }
        go.transform.position = collar.transform.position;
        go.transform.SetParent(PROJECTILE_ANCHOR, true);
        Projectile p = go.GetComponent<Projectile>();
        p.type = type;
        lastShotTime = Time.time;
        return p;
    }
}
