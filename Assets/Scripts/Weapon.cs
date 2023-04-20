using UnityEngine;
using Unity;
using System;
using System.Collections;
using Vector3 = UnityEngine.Vector3;

public class Weapon : MonoBehaviour
{
    [SerializeField] protected float range;
    [SerializeField] protected float fireRate;
    [SerializeField] protected float damage;
    [SerializeField] protected Enemy target;
    [SerializeField] protected GameObject projectile;
    [SerializeField] protected GameObject baseEffect;
    [SerializeField] private float turnSpeed = 10.0f;
    [SerializeField] private float fireCountdown = 0.0f;
    [SerializeField] private GameObject bulletPrefab;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("AcquireTarget", 0.0f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        fireCountdown -= Time.deltaTime;
        if (target is not null && !IsInRange(target))
        {
            target = null;
        }


        if (target == null)
            return;

        AimAtTarget();
        Shoot();
    }

    private void Shoot()
    {
        if (fireCountdown <= 0)
        {
            fireCountdown = 1f / fireRate;
            GameObject b = Instantiate(bulletPrefab, transform.position, transform.rotation, transform.parent);
            b.GetComponent<Bullet>().SetTarget(target.transform);
        }
    }

    private void AimAtTarget()
    {
        Vector3 dir = target.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(projectile.transform.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        projectile.transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    public void CreateBullet(Vector3 startPos, Vector3 aim)
    {

    }

    public void AcquireTarget()
    {
        if (target != null)
            return;

        foreach (Enemy e in GameManager.EnemyManager.Enemies)
        {
            if (IsInRange(e))
            {
                Debug.Log("Set Target.");
                target = e;
                target.OnDeath += UnTargetEnemy;
                break;
            }
        }
    }

    private void UnTargetEnemy()
    {
        Debug.Log("Untargeting Death Enemy.");
        target = null;
    }

    private bool IsInRange(Enemy e) => Vector3.Distance(transform.position, e.transform.position) <= range ? true : false;

    void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = new Color(1, 1, 0, 0.75F);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}