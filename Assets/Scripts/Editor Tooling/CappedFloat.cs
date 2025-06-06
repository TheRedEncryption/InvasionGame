using UnityEngine;

[System.Serializable]
public class CappedFloat
{
    [SerializeField]
    private float _currValue = 0;

    /// <summary>
    /// The current value stored in the float
    /// </summary>
    public float CurrValue
    {
        get => _currValue;
        set
        {
            if (value == _currValue) ChangeFail?.Invoke();
            else ChangeSuccessful?.Invoke();

            if (value <= 0)
            {
                _currValue = 0;
                OnEmpty?.Invoke();
            }
            else if (value >= MaxValue)
            {
                _currValue = MaxValue;
                OnMax?.Invoke();
            }
            else
            {
                _currValue = value;
            }
        }
    }

    /// <summary>
    /// The maximum allowed value
    /// </summary>
    public float MaxValue;

    #region Events

    /// <summary>
    /// A delegate for events in the <see cref="CappedFloat"/>
    /// </summary>
    public delegate void CappedFloatEvent();

    /// <summary>
    /// The event that triggers whenever the current value is 0
    /// </summary>
    public event CappedFloatEvent OnEmpty;

    /// <summary>
    /// The event that triggers whenever the current value is equal to <see cref="MaxValue"/>
    /// </summary>
    public event CappedFloatEvent OnMax;

    /// <summary>
    /// Called when there is a change to _currValue 
    /// </summary>
    public event CappedFloatEvent ChangeSuccessful;

    /// <summary>
    /// Called when the changed value is the same as the old value
    /// </summary>
    public event CappedFloatEvent ChangeFail;

    #endregion

    #region Operational overrides

    public static CappedFloat operator -(CappedFloat a)
    { a.CurrValue = a.MaxValue - a.CurrValue; return a; }

    public static CappedFloat operator +(CappedFloat a, float b)
    { a.CurrValue += b; return a; }

    public static CappedFloat operator -(CappedFloat a, float b)
    { a.CurrValue -= b; return a; }

    public static CappedFloat operator *(CappedFloat a, float b)
    { a.CurrValue *= b; return a; }

    public static CappedFloat operator /(CappedFloat a, float b)
    { a.CurrValue /= b; return a; }

    public static CappedFloat operator +(float a, CappedFloat b) => b + a;

    public static CappedFloat operator -(float a, CappedFloat b) => b - a;

    public static CappedFloat operator *(float a, CappedFloat b) => b * a;

    public static CappedFloat operator /(float a, CappedFloat b) => b / a;

    /// <summary>
    /// Assigns the value to it's highest possible value
    /// </summary>
    public static CappedFloat operator ++(CappedFloat a)
    { a.CurrValue = a.MaxValue; return a; }

    /// <summary>
    /// Assigns the value to it's lowest possible value
    /// </summary>
    public static CappedFloat operator --(CappedFloat a)
    { a.CurrValue = 0; return a; }

    public static explicit operator int(CappedFloat a) => (int)a.CurrValue;
    public static implicit operator float(CappedFloat a) => a.CurrValue;

    #endregion
}
