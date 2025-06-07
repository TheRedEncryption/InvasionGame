using UnityEngine;

public class DemoTurretShoot : ProjectileSpawner
{
    [SerializeField] private float _splitTime;

    private CappedFloat _timer = new ();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _timer.MaxValue = _splitTime;
        _timer.CurrValue = 0;
        _timer.OnMax += SpawnProjectile;
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
    }

    // This section is so unreasonably hard to read it's not getting changed lmao
    #region --- Handle counting --- 

    Vector3 _dirCurrent;

    int _counter = 0;

    private void UpCounter() { _counter++; if (_counter == 4) _counter = 0; }
    
    private Vector3 GetDir() => _counter switch
    {
        0 => transform.forward,
        1 => transform.right,
        2 => - transform.right,
        3 => -transform.forward,
        _ => Vector3.positiveInfinity
    };

    private void UpdateDirection() { UpCounter(); _dirCurrent = GetDir(); }

    #endregion

    protected override Vector3 GetSpawnPoint() => _dirCurrent + transform.position;
    protected override Vector3 GetSpawnDir() => _dirCurrent;

    public override void SpawnProjectile()
    {
        UpdateDirection();
        base.SpawnProjectile();

        UpdateDirection();
        base.SpawnProjectile();

        _timer.CurrValue = 0;
    }
}
