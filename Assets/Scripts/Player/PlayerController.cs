using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;

    public static float fixedMoveSpeed = 5;
    public float moveSpeed = fixedMoveSpeed;

    public float bulletSpeed = 5;
    float selectDistance = 1.7f;

    public static float fixedFirePeriod = 0.12f;//how many seconds between fires
    public float firePeriod = fixedFirePeriod;//how many seconds between fires

    float timeSinceFire = 0;
    float passiveHeal = 5f;//health gained per second of not moving

    public GameObject bulletPrefab;
    public GameObject airBulletPrefab;
    public GameObject heaterPrefab;
    public GameObject graveArrowPrefab;

    public int playerNumber;
    public Color playerColour;
    public XboxController ControllerNumber = XboxController.First;

    public int kills;
    private bool canPlace;
    Building selectedBuilding;
    Heater carryingHeater;
    Health health;

    public bool useController;
    Vector3Int selectIndex = Vector3Int.zero;


    public Animator animator;
    private bool isAiming;
    private bool isHolding;
    public bool isDowned = false;
    public float powerTime ;
    public bool usingShotgun ;

    CurrencyManager wallet;

    public AudioSource gunShot;
    public AudioSource shotgunSound;
    public AudioSource Run;
    private bool isRunning = false;
    private int runCount = 0;
    public AudioSource blowerSound;
    private bool isBlowing = false;
    private int blowCount = 0;
    private float windPitch;
    public AudioSource repairSound;
    private bool isReparing = false;
    private int repairCount = 0;

    Heater deathHeater;

    public ParticleSystem reviveEffect;
    public ParticleSystem downedEffect;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        useController = GameInfo.usingControllers;
        canPlace = true;
        ControllerNumber = (XboxController)(playerNumber + 1);
        health = GetComponent<Health>();
        wallet = GetComponent<CurrencyManager>();
    }

    // Update is called once per frame
    void Update()
    {
        powerTime -= Time.deltaTime;
        if (powerTime < 0)
        {
            firePeriod = fixedFirePeriod;
            moveSpeed = fixedMoveSpeed;
            usingShotgun = false;
        }
        if (isDowned)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        timeSinceFire += Time.deltaTime;

        bool pickedUp = false;
        Vector2 moveInput = Vector2.zero;
        Ray2D shootingRay = new Ray2D();


        //buttons
        bool placeTemp = false;
        bool placePerm = false;
        bool shoot = false;
        bool blow = false;
        bool upgrade = false;
        bool pickUp = false;
        bool fix = false;

        //Shooting (Mouse and keyboard)
        if (!useController)
        {
            //Movement
            moveInput = new Vector2(-Input.GetAxisRaw("Keys_Horizontal"), Input.GetAxisRaw("Keys_Vertical")).normalized;
            shootingRay = new Ray2D(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);


            //Placement highlighting
            Vector2 selectPos = shootingRay.GetPoint(selectDistance);
            if ((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).magnitude > 0)
            {
                selectIndex = WorldManager.singleton.tilemap.WorldToCell(new Vector3(selectPos.x, selectPos.y, 0));
            }

            blow = Input.GetAxis("Keys_Blow") > 0;
            shoot = Input.GetAxis("Keys_Shoot") > 0;
            placeTemp = Input.GetKeyDown(KeyCode.C);
            placePerm = Input.GetKeyDown(KeyCode.F);
            upgrade = Input.GetKeyDown(KeyCode.Q);
            pickUp = Input.GetKeyDown(KeyCode.E);
            fix = Input.GetKey(KeyCode.R);
        }

        //Shooting (With Controller)
        if (useController)
        {
            //Movement
            moveInput = new Vector2(-XCI.GetAxisRaw(XboxAxis.LeftStickX, ControllerNumber), XCI.GetAxisRaw(XboxAxis.LeftStickY, ControllerNumber)).normalized;
            Vector2 playerDirection = Vector2.right * -XCI.GetAxisRaw(XboxAxis.RightStickX, ControllerNumber) + Vector2.up * XCI.GetAxisRaw(XboxAxis.RightStickY, ControllerNumber);
            shootingRay = new Ray2D(transform.position, playerDirection);



            //Placement highlighting
            Vector2 selectPos = shootingRay.GetPoint(selectDistance);
            if (playerDirection.magnitude > 0)
            {
                selectIndex = WorldManager.singleton.tilemap.WorldToCell(new Vector3(selectPos.x, selectPos.y, 0));

            }


            shoot = XCI.GetAxis(XboxAxis.RightTrigger, ControllerNumber) > 0 && playerDirection.magnitude > 0;
            blow = XCI.GetAxis(XboxAxis.LeftTrigger, ControllerNumber) > 0 && playerDirection.magnitude > 0;
            placeTemp = XCI.GetButtonDown(XboxButton.LeftBumper, ControllerNumber);
            placePerm = XCI.GetButtonDown(XboxButton.RightBumper, ControllerNumber);
            upgrade = XCI.GetButtonDown(XboxButton.Y, ControllerNumber);
            pickUp = XCI.GetButtonDown(XboxButton.B, ControllerNumber);
            fix = XCI.GetButton(XboxButton.X, ControllerNumber);

        }



        rb.velocity = moveInput * moveSpeed;

        //Sound
        if (moveInput.magnitude > 0)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        if (isRunning == true && runCount == 0)
        {
            Run.Play();
            runCount++;
        }
        else if (isRunning == false)
        {
            Run.Pause();
            runCount = 0;
        }

        if (shootingRay.direction.magnitude > 0)
        {
            //for animation 
            isAiming = true;
        }
        else
        {
            isAiming = false;
        }

        if (moveInput.magnitude > 0)
        {
            health.heal(passiveHeal * Time.deltaTime);
        }
        if (selectedBuilding != null)
        {
            selectedBuilding.renderer.color = Color.white;
            SpriteRenderer s = selectedBuilding.transform.GetChild(4).gameObject.GetComponent<SpriteRenderer>();
            s.sprite = null;
            s.color = Color.white;
        }

        selectedBuilding = WorldManager.singleton.SelectCell(
                new Vector2Int(selectIndex.x - WorldManager.singleton.tilemap.origin.x,
                    selectIndex.y - WorldManager.singleton.tilemap.origin.y), playerNumber, playerColour);


        //Rotating sprite
        if (shootingRay.direction.magnitude != 0)
        {
            float angle = Mathf.Atan2(shootingRay.direction.x, shootingRay.direction.y) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(-angle + 90, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, bulletSpeed * Time.deltaTime);
        }

        //Random pitch
        windPitch += UnityEngine.Random.Range(-0.03f, 0.03f);
        windPitch = Math.Min(Math.Max(0.75f, windPitch), 0.85f);

        if (blow)
        {
            Blow(shootingRay.direction);
            isBlowing = true;
        }
        else
        {
            isBlowing = false;
        }

        //Sound
        if (isBlowing == true && blowCount == 0)
        {
            blowerSound.Play();
            blowerSound.pitch = windPitch;
            blowCount++;
        }
        else if (isBlowing == false)
        {
            blowerSound.Pause();
            blowCount = 0;
        }

        if (shoot && timeSinceFire > firePeriod)
        {
            Shoot(shootingRay.direction);
            //Sound
           
        }

        if (placeTemp && canPlace && carryingHeater == null && wallet.cash >= 6)
        {
            GameObject heater = WorldManager.singleton.placeObjectOnSelectedTile(heaterPrefab, playerNumber, true);
            if (heater != null)
            {
                heater.GetComponent<Heater>().type = Heater.Type.Temporary;
                wallet.useCurrency(6);
                StartCoroutine(placementCooldown());
            }




        }
        else if (placePerm && canPlace && carryingHeater == null && wallet.cash >= 15)
        {
            GameObject heater = WorldManager.singleton.placeObjectOnSelectedTile(heaterPrefab, playerNumber, true);
            if (heater != null)
            {
                wallet.useCurrency(15);
                StartCoroutine(placementCooldown());
            }
        }

        if (selectedBuilding != null)
        {
            //Debug.Log("selecting a building: " + selectedBuilding.gameObject.name);
            Heater h = selectedBuilding.gameObject.GetComponent<Heater>();
            if (h != null && !h.type.Equals(Heater.Type.Temporary))
            {
                if (upgrade && !h.type.Equals(Heater.Type.LV3) && wallet.cash >= 12)
                {//upgrade
                    h.upgrade();
                    wallet.useCurrency(12);
                }
                if (pickUp)
                {//pick up
                    if (carryingHeater == null)
                    {
                        carryingHeater = h;
                        WorldManager.singleton.removeHeaterAt(h.attachedTileIndex);
                        h.transform.parent = transform;
                        pickedUp = true;
                    }
                }
            }
            selectedBuilding.renderer.color = new Color(0, 30, 60);
            try
            {
                //selectedBuilding.transform.GetChild(4).gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/FullMenu" + (useController ? "XBOX" : "Keys"));
                SpriteRenderer s = selectedBuilding.transform.GetChild(4).gameObject.GetComponent<SpriteRenderer>();
                s.sprite = Resources.Load<Sprite>("Sprites/FullMenu" + (useController ? "XBOX" : "Keys"));
                s.color = playerColour;
            }
            catch {
                Debug.LogError("failed to find sprite: " + "Sprites/FullMenu" + (useController ? "XBOX" : "Keys"));
            }
        }


        carryHeater(pickedUp, pickUp);
        if (!checkForDownedPlayers(fix))
        {//didnt find a nearby downed player
            if (fix && selectedBuilding != null && !selectedBuilding.gameObject.GetComponent<Heater>().type.Equals(Heater.Type.Temporary))
            {
                selectedBuilding.gameObject.GetComponent<Health>().heal(40 * Time.deltaTime);

                //Sound
                isReparing = true;
            }
            else
            {
                isReparing = false;
            }
        }
        checkIfInSnow();


        if (isReparing == true && repairCount == 0 && !selectedBuilding.gameObject.GetComponent<Health>().isMax())
        {
            repairSound.Play();
            repairCount++;
        }
        else if (isReparing == false)
        {
            repairSound.Pause();
            repairCount = 0;
        }

        //Animation
        animator.SetBool("isAiming", isAiming);
        animator.SetBool("isHolding", carryingHeater != null);

        Vector2 pos = new Vector2(Math.Max(-64, Math.Min(-16, transform.position.x)), Math.Max(-33, Math.Min(-2, transform.position.y)));
        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    private void checkIfInSnow()
    {
        Vector3Int cellPos = WorldManager.singleton.tilemap.WorldToCell(transform.position);
        cellPos.x -= WorldManager.singleton.tilemap.origin.x;
        cellPos.y -= WorldManager.singleton.tilemap.origin.y;
        //Debug.Log("player on tile: " + cellPos + " selected tile: " + WorldManager.singleton.currentlySelectedTiles[playerNumber].attachedTileIndex);
        if (WorldManager.singleton.snowEffects.ContainsKey(new Vector2Int(cellPos.x, cellPos.y)) && WorldManager.singleton.snowEffects[new Vector2Int(cellPos.x, cellPos.y)].isInAnchorPosition())
        {
            health.damage(18 * Time.deltaTime);
        }
    }

    private bool checkForDownedPlayers(bool fix)
    {
        foreach (GameObject player in WorldManager.singleton.players)
        {
            if (player.GetComponent<PlayerController>().isDowned)
            {
                if (Vector3.Distance(transform.position, player.transform.position) < selectDistance)
                {
                    if (XCI.GetButtonDown(XboxButton.X, ControllerNumber))
                    {
                        player.GetComponent<PlayerController>().revive();
                    }
                    return true;
                }
            }
        }

        return false;
    }

    private void revive()
    {
        health.health = health.maxHealth / 2;
        isDowned = false;
        Destroy(deathHeater.gameObject);
        GetComponent<SpriteRenderer>().enabled = true;
        // Effect
        var effectMain = reviveEffect.main;
        effectMain.startColor = playerColour;
        reviveEffect.Play();
    }

    public void carryHeater(bool pickedUp, bool pressedPickUp)
    {
        try
        {
            if (carryingHeater == null)
            {
                return;
            }

            try
            {
                //Sprite s  = 
                SpriteRenderer s = carryingHeater.transform.GetChild(4).gameObject.GetComponent<SpriteRenderer>();
                s.sprite = Resources.Load<Sprite>("Sprites/DropMenu" + (useController ? "XBOX" : "Keys"));
                s.color = playerColour;

            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError("failed to find sprite: " + "Sprites/DropMenu" + (useController ? "XBOX" : "Keys"));
            }
            carryingHeater.isActive = false;
            //Debug.Log("renderer: " + carryingHeater.renderer);
            if (carryingHeater.renderer == null)
            {
                carryingHeater.renderer = carryingHeater.gameObject.GetComponent<SpriteRenderer>();
            }
            carryingHeater.renderer.color = new Color(0, 50, 0, 140);

            //Debug.Log("carrying heater");
            carryingHeater.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            if (pressedPickUp && !pickedUp)
            {
                WorldManager.singleton.placeObjectOnSelectedTile(carryingHeater.gameObject, playerNumber, false);
                carryingHeater.isActive = true;
                carryingHeater = null;
            }
        }
        catch {
            try
            {
                WorldManager.singleton.placeObjectOnSelectedTile(carryingHeater.gameObject, playerNumber, false);
                carryingHeater.isActive = true;
                carryingHeater = null;
            }
            catch {
                Destroy(carryingHeater.gameObject);
                carryingHeater = null;
            }
        }


    }
    IEnumerator placementCooldown()
    {
        canPlace = false;
        yield return new WaitForSeconds(1);
        canPlace = true;
    }

    float shotGunSpread = 0.2f;

    void Shoot(Vector2 direction)
    {
        timeSinceFire = 0;
        if (carryingHeater != null) return;
        var bullet = Instantiate(bulletPrefab, transform.position + new Vector3(direction.x, direction.y, 0) * 0.5f, transform.rotation);
        bullet.GetComponent<Rigidbody2D>().velocity = direction.normalized * bulletSpeed;
        bullet.GetComponent<Bullet>().shooter = this;
        Destroy(bullet.gameObject, 2f);

        if (usingShotgun)
        {
            //Debug.Log("uing shotgun");
            for (int i = 1; i < 3; i++)
            {
                for (int j = -1; j < 2; j += 2)
                {
                    bullet = Instantiate(bulletPrefab, transform.position + new Vector3(direction.x, direction.y, 0) * 0.5f, transform.rotation);
                    bullet.GetComponent<Rigidbody2D>().velocity = (direction.normalized + new Vector2(direction.y, -direction.x).normalized * j * i * shotGunSpread) * bulletSpeed;
                    bullet.GetComponent<Bullet>().shooter = this;
                    Destroy(bullet.gameObject, 2f);
                }
            }
         
        }
        
            gunShot.pitch = 0.55f + UnityEngine.Random.Range(-0.03f, 0.03f);
            gunShot.Play();
        
    }

    void Blow(Vector2 direction)
    {
        if (carryingHeater != null) return;
        Rigidbody2D rigidbody = Instantiate(airBulletPrefab, transform.position + new Vector3(direction.x, direction.y, 0) * 0.5f, Quaternion.identity).GetComponent<Rigidbody2D>();
        rigidbody.velocity = direction.normalized * bulletSpeed * 0.6f;
        Destroy(rigidbody.gameObject, 2f);
    }


    internal void downPlayer()
    {
        Vector2Int cellPos = getCellPos();
        isDowned = true;
        deathHeater = Heater.getDeathHeater(cellPos);
        deathHeater.type = Heater.Type.Death;
        GetComponent<SpriteRenderer>().enabled = false;
        if (WorldManager.singleton.snowEffects.ContainsKey(cellPos))
        {
            WorldManager.singleton.destroySnowTile(cellPos);
        }
        deathHeater.AnchorPosition = WorldManager.singleton.tilemap.CellToWorld(new Vector3Int(WorldManager.singleton.tilemap.origin.x + cellPos.x, WorldManager.singleton.tilemap.origin.y + cellPos.y, 1));
        deathHeater.attachedTileIndex = cellPos;
        transform.position = deathHeater.AnchorPosition;
        deathHeater.gameObject.GetComponent<SpriteRenderer>().color = playerColour;
        
        // Effect
        var effectMain = reviveEffect.main;
        effectMain.startColor = playerColour;
        reviveEffect.Play();
        //GameObject arrow = Instantiate(graveArrowPrefab, transform.position+new Vector3(0,2,0), Quaternion.Euler(0, 0, 0));
        //arrow.GetComponent<SpriteRenderer>().color = playerColour;
        //arrow.transform.parent = transform;
        //arrow.transform.localPosition = Vector3.zero;
    }

    public Vector2Int getCellPos()
    {
        Vector3Int cellPos = WorldManager.singleton.tilemap.WorldToCell(transform.position);
        return new Vector2Int(cellPos.x - WorldManager.singleton.tilemap.origin.x, cellPos.y - WorldManager.singleton.tilemap.origin.y);
    }


}
