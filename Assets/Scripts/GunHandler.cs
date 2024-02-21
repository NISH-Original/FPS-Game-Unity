using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunHandler : MonoBehaviour
{
    public GameObject gunObject;
    [SerializeField] PlayerController pc;
    [SerializeField] Renderer scopeRenderer;
    public AudioSource audioSource;
    public bool isReloading;
    
    [Header("Gun Properties")]
    public string gunName;
    public bool isAutomatic;
    public bool isShotgun;
    public bool isSniper;
    public float fireRate;
    public int pelletsPerAttack;
    public float spread;
    public int magSize;
    public float pumpTime;
    public float bodyDamage;
    public float headDamage;
    [HideInInspector] public int currentAmmo;
    float scopeDepth;
    public float sniperScopeSpeed;
    
    [Header("Scoping")]
    public float scopeInSpeed;
    public float scopeZoomMult;
    public Vector3 adsFirePos;
    
    [Header("Weapon Shooting")]
    public float lastShot;
    [SerializeField] GameObject bulletImpactPrefab;
    [SerializeField] GameObject bloodImpactPrefab;
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] ParticleSystem shellEjection;
    [SerializeField] float shootingTime;

    [Header("Weapon Scoping")]
    [SerializeField] Camera cam;
    [SerializeField] Vector3 baseLocalPosition;
    [SerializeField] float baseCameraFOV;

    [Header("Weapon Reloading")]
    [SerializeField] float reloadTime;

    [Header("Weapon Animations")]
    public Animator animator;

    [Header("Visual Recoil Settings")]
    public float positionalRecoilSpeed = 8f;
    public float rotationalRecoilSpeed = 8f;
    public float positionalReturnSpeed = 18f;
    public float rotationalReturnSpeed = 38f;
    public Vector3 RecoilRotation = new Vector3(10, 5, 7);
    public Vector3 RecoilKickBack = new Vector3(0.015f, 0f, -0.2f);
	public Vector3 RecoilRotationAim = new Vector3(10, 4, 6);
	public Vector3 RecoilKickBackAim = new Vector3(0.015f, 0f, -0.2f);
    public Transform rotationPoint;
    public Transform recoilPosition;
    public Vector3 rotationalRecoil;
	public Vector3 positionalRecoil;
    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;
    Vector3 camTargetRotation;
    Vector3 camCurrentRotation;
    Vector3 Rot;

    [Header("Camera Recoil Settings")]
    public float camRecoilX;
    public float camRecoilY;
    public float camRecoilZ;
    public float aimCamRecoilX;
    public float aimCamRecoilY;
    public float aimCamRecoilZ;
    public float camSnappiness;
    public float camReturnSpeed;

    [Header("Bullet Trails")]
    [SerializeField] TrailRenderer bulletTrail;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] float bulletSpeed = 100f;

    [Header("Sway Settings")]
    public float hipSwayAmount;
    public float hipSwaySpeed;
    public float adsSwayAmount;
    public float adsSwaySpeed;
    [SerializeField] Transform swayPivot;

    [Header("Sounds")]
    public AudioClip shotSound;
    public AudioClip pumpSound;
    public AudioClip reloadSound;
    public AudioClip dryFire;

    float hipMouseX;
    float hipMouseY;
    float hipMouseZ;

    float adsMouseX;
    float adsMouseY;
    float adsMouseZ;

    void Start()
    {
        pc.isScoped = false;
        baseCameraFOV = cam.fieldOfView;
        baseLocalPosition = gunObject.transform.localPosition;
        newWeaponRotation = gunObject.transform.localRotation.eulerAngles;
        currentAmmo = magSize;
        audioSource = GetComponent<AudioSource>();
        isReloading = false;
        animator.keepAnimatorControllerStateOnDisable = true;

        //if(isSniper)
        //    scopeRenderer.material.shader = Shader.Find("ScopeShader");
    }

    void OnEnable()
    {
        Debug.Log(gunName + " equipped!");
        animator.Rebind();
        animator.Update(0f);
    }

    void Awake()
    {
        //animator.SetTrigger("Equipped");
        animator.SetBool("isSprinting", false);
        scopeDepth = 0;
    }

    void Update()
    {   
        HandleScoping();
        HandleSway();
        if(!pc.isScoped)
        {
            animator.SetFloat("SpeedX", pc.horizontalInput);
            animator.SetFloat("SpeedY", pc.verticalInput);
            if(pc.state == PlayerController.MovementState.sprinting)
                animator.SetBool("isSprinting", true);
            else
                animator.SetBool("isSprinting", false);
        }
        else
        {
            animator.SetFloat("SpeedX", 0);
            animator.SetFloat("SpeedY", 0);
        }

        if(isSniper)
        {
            scopeRenderer.material.SetFloat("_Depth", scopeDepth);
        }
        
    }
    
    void FixedUpdate()
    {
        rotationalRecoil = Vector3.Lerp(rotationalRecoil, Vector3.zero, rotationalReturnSpeed * Time.deltaTime);
		positionalRecoil = Vector3.Lerp(positionalRecoil, Vector3.zero, positionalReturnSpeed * Time.deltaTime);

		recoilPosition.localPosition = Vector3.Slerp(recoilPosition.localPosition, positionalRecoil, positionalRecoilSpeed * Time.deltaTime);
		Rot = Vector3.Slerp(Rot, rotationalRecoil, rotationalRecoilSpeed * Time.deltaTime);
		rotationPoint.localRotation = Quaternion.Euler(Rot);

        camTargetRotation = Vector3.Lerp(camTargetRotation, Vector3.zero, camReturnSpeed * Time.deltaTime);
        camCurrentRotation = Vector3.Slerp(camCurrentRotation, camTargetRotation, camSnappiness * Time.fixedDeltaTime);
        cam.transform.localRotation = Quaternion.Euler(camCurrentRotation);
    }

    void HandleScoping()
    {
        float scopedFOV = baseCameraFOV * scopeZoomMult;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, pc.isScoped ? scopedFOV : baseCameraFOV, scopeInSpeed * Time.deltaTime);

        Vector3 targetLocalPosition = pc.isScoped ? adsFirePos : baseLocalPosition;
        gunObject.transform.localPosition = Vector3.Lerp(gunObject.transform.localPosition, targetLocalPosition, scopeInSpeed * Time.deltaTime);

        if(isSniper)
        {
            scopeDepth = Mathf.Lerp(scopeDepth, pc.isScoped ? 2 : 0, sniperScopeSpeed * Time.deltaTime);
        }
    }

    /* void Use()
    {
        Shoot();
    }

    void UseAutomatic()
    {
        if(!isAutomatic)
            return;
        Shoot();
    } */
    
    public void Shoot()
    {   
        if(isReloading)
            return;

        if(currentAmmo < 1)
        {
            return;
            
            //StartCoroutine(Reload());
            
            //if(!isShotgun)
            //    StartCoroutine(Reload());
            //else
            //{
            //    for(int i = 0; i < (magSize - currentAmmo); i++)
            //    {
            //        if(currentAmmo > magSize)
            //            return;
            //
            //        StartCoroutine(Reload());
            //    }
            //}
        }
        
        if (Time.time < lastShot + fireRate)
            return;

        StartCoroutine(ShootAnimation());

        for (int i = 0; i < pelletsPerAttack; i++) 
        {
            Vector2 gunSpread = Random.insideUnitCircle * spread;
            Ray ray = cam.ViewportPointToRay(new Vector2(0.5f, 0.5f) + gunSpread / (cam.fieldOfView / baseCameraFOV));
            ray.origin = cam.transform.position;
            RecoilFire();
            muzzleFlash.Play();
            shellEjection.Play();
            audioSource.PlayOneShot(shotSound);
            TrailRenderer trail = Instantiate(bulletTrail, bulletSpawnPoint.position, Quaternion.identity);

            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                Collider[] colliders = Physics.OverlapSphere(hit.point, 0.3f);
                EnemyHandler enemy = hit.transform.GetComponent<EnemyHandler>();

                if(colliders.Length != 0)
                {   
                    if(enemy != null)
                    {
                        enemy.TakeDamage(bodyDamage);
                        pc.hitMarkerAudioSource.PlayOneShot(pc.hitSound);
                        Debug.Log(enemy.currentHealth);

                        if(enemy.currentHealth <= 0)
                            StartCoroutine(ShowKillMarker());
                        else
                            StartCoroutine(ShowHitMarker());

                        GameObject bloodImpactObj = Instantiate(bloodImpactPrefab, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal, Vector3.up) * bloodImpactPrefab.transform.rotation);
                        Destroy(bloodImpactObj, 2f);
                        bloodImpactObj.transform.SetParent(colliders[0].transform);
                    }
                    else
                    {
                        GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal, Vector3.up) * bulletImpactPrefab.transform.rotation);
                        Destroy(bulletImpactObj, 2f);
                        bulletImpactObj.transform.SetParent(colliders[0].transform);
                    }
                }
                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal));
            }
            else
            {
                Vector3 direction = transform.forward;
                StartCoroutine(SpawnTrail(trail, direction * 100, Vector3.zero));
            }
        }

        if(!isShotgun)
            currentAmmo -= pelletsPerAttack;
        else
        {
            currentAmmo -= 1;
            //StartCoroutine(PumpShotgun());
            //audioSource.PlayOneShot(pumpSound);
        }

        rotationalRecoil += new Vector3(-GetRecoilRotation().x, Random.Range(-GetRecoilRotation().y, GetRecoilRotation().y), Random.Range(-GetRecoilRotation().z, GetRecoilRotation().z));
		positionalRecoil += new Vector3(Random.Range(-GetRecoilKickBack().x, GetRecoilKickBack().x), Random.Range(-GetRecoilKickBack().y, GetRecoilKickBack().y), GetRecoilKickBack().z);
 
        lastShot = Time.time;
    }

    public IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, Vector3 hitNormal) 
    {
        Vector3 startPosition = trail.transform.position;
        Vector3 direction = (hitPoint - trail.transform.position).normalized;

        float distance = Vector3.Distance(trail.transform.position, hitPoint);
        float startingDistance = distance;

        while(distance > 0)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (distance / startingDistance));
            distance -= Time.deltaTime * bulletSpeed;

            yield return null;
        }

        trail.transform.position = hitPoint;

        Destroy(trail.gameObject, trail.time);
    }

    public IEnumerator ShowHitMarker()
    {
        if(isAutomatic)
        {
            if(pc.hitmarkerRight)
            {
                pc.hitmarker.rotation = Quaternion.Euler(0f, 0f, pc.hitmarkerRot);
                pc.hitmarkerRight = false;
            }
            else
            {
                pc.hitmarker.rotation = Quaternion.Euler(0f, 0f, -pc.hitmarkerRot);
                pc.hitmarkerRight = true;
            }
        }
        else
        {
            pc.hitmarker.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        pc.hitmarkerCanvas.alpha = 1;

        yield return new WaitForSeconds(pc.hitmarkerLastingTime);

        pc.hitmarkerCanvas.alpha = 0;
    }

    public IEnumerator ShowKillMarker()
    { 
        pc.hitmarkerCanvas.alpha = 0;
        pc.killmarker.SetActive(true);

        yield return new WaitForSeconds(pc.killmarkerLastingTime);

        pc.killmarker.SetActive(false);
    }

    public IEnumerator Reload()
    {
        isReloading = true;
        animator.SetBool("isReloading", true);

        yield return new WaitForSeconds(reloadTime);

        if (isShotgun)
        {
            currentAmmo = Mathf.Min(currentAmmo + 1, magSize);
        } else
        {
            currentAmmo = magSize;
        }
        
        isReloading = false;
        animator.SetBool("isReloading", false);
    }

    public IEnumerator ShootAnimation()
    {
        animator.SetBool("shooting", true);

        yield return new WaitForSeconds(shootingTime);

        animator.SetBool("shooting", false);
    }

    //public void ShotgunReload()
    //{
    //    for(int i = 0; i < (magSize - currentAmmo); i++)
    //    {
    //        if(currentAmmo > magSize)
    //            return;
    //        
    //        StartCoroutine(Reload());
    //    }
    //}

    /*public IEnumerator PumpShotgun()
    {   
        animator.SetTrigger("Pumping");

        yield return new WaitForSeconds(pumpTime);

        animator.ResetTrigger("Pumping");

        Debug.Log("pumped!");
    }*/

    protected virtual void RecoilFire()
    {
        if (pc.isScoped) camTargetRotation += new Vector3(aimCamRecoilX, Random.Range(-aimCamRecoilY, aimCamRecoilY), Random.Range(-aimCamRecoilZ, aimCamRecoilZ));
        else camTargetRotation += new Vector3(camRecoilX, Random.Range(-camRecoilY, camRecoilY), Random.Range(-camRecoilZ, camRecoilZ));
    }

    protected virtual Vector3 GetRecoilRotation()
    {
        if (pc.isScoped)
            return RecoilRotationAim;
        else
            return RecoilRotation;
    }

    protected virtual Vector3 GetRecoilKickBack()
    {
        if (pc.isScoped)
            return RecoilKickBackAim;
        else
            return RecoilKickBack;
    }

    protected virtual void HandleSway()
    {
        if (!pc.isScoped)
        {
            hipMouseX = Input.GetAxis("Mouse X") * hipSwayAmount;
            hipMouseY = Input.GetAxis("Mouse Y") * hipSwayAmount;
            hipMouseZ = Input.GetAxis("Horizontal") * hipSwayAmount;

            Quaternion hipRotationX = Quaternion.AngleAxis(-hipMouseY, Vector3.right);
            Quaternion hipRotationY = Quaternion.AngleAxis(hipMouseX, Vector3.up);
            Quaternion hipRotationZ = Quaternion.AngleAxis(-hipMouseZ, Vector3.forward);

            Quaternion hipTargetRotation = hipRotationX * hipRotationY * hipRotationZ;
        
            swayPivot.localRotation = Quaternion.Lerp(transform.localRotation, hipTargetRotation, hipSwaySpeed * Time.deltaTime);
        }
        else
        {
            adsMouseX = Input.GetAxis("Mouse X") * adsSwayAmount;
            adsMouseY = Input.GetAxis("Mouse Y") * adsSwayAmount;
            adsMouseZ = Input.GetAxis("Horizontal") * adsSwayAmount;

            Quaternion adsRotationX = Quaternion.AngleAxis(-adsMouseY, Vector3.right);
            Quaternion adsRotationY = Quaternion.AngleAxis(adsMouseX, Vector3.up);
            Quaternion adsRotationZ = Quaternion.AngleAxis(-adsMouseZ, Vector3.forward);

            Quaternion adsTargetRotation = adsRotationX * adsRotationY * adsRotationZ;
        
            swayPivot.localRotation = Quaternion.Lerp(transform.localRotation, adsTargetRotation, adsSwaySpeed * Time.deltaTime);
        }
    }

    void OnDisable()
    {
        animator.Play("Draw",0,0f);
    }
}
