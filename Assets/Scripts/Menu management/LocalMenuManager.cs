using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LocalMenuManager : MonoBehaviour
{
    public GameObject _initalMenu;

    #region  Singleton implementation

    /// <summary>
    /// Access the local menu manager
    /// </summary>
    public static LocalMenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError($"Duplicate {typeof(LocalMenuManager)} found in scene, removing...");
            Destroy(gameObject);
        }
    }

    #endregion

    /// <summary>
    /// A container for the menus / UIs displayed while the game is running.
    /// </summary>
    /// <remarks>It is safe to assume this stack will never be empty, as then the program will be closed.</remarks>
    private Stack<MenuGrouping> _menuGroupings = new();

    public bool IsMainMenu => _menuGroupings.Count == 1;

    #region Menu Display management

    void Start()
    {
        // If there is no Menu Grouping component, it can't go in the stack
        if (!_initalMenu.TryGetComponent(out MenuGrouping myMenuGrouping) || _menuGroupings.Count != 0)
            Debug.LogError($"Invalid starting menu conditions: (Menu Grouping? {myMenuGrouping == null}) || (Stack Count? {_menuGroupings.Count})");

        myMenuGrouping.Display();
        _menuGroupings.Push(myMenuGrouping);
    }

    /// <summary>
    /// Push a menu onto the Stack, displays menu while hiding the previous top
    /// </summary>
    /// <param name="grouping">The Gameobject to display</param>
    public void Push(GameObject grouping)
    {
        // If there is no Menu Grouping component, it can't go in the stack
        if (!grouping.TryGetComponent(out MenuGrouping myMenuGrouping))
            return;

        _menuGroupings.Peek().Hide();
        myMenuGrouping.Display();
        _menuGroupings.Push(myMenuGrouping);
    }

    /// <summary>
    /// Gets the gameobject associated with the MenuGrouping at the top of the stack.
    /// </summary>
    /// <returns>The top of the stack's gameobject.</returns>
    public GameObject PeekObj() => _menuGroupings.Peek().gameObject;

    /// <summary>
    /// Removes the top element in the stack, hiding it's gameobject, and displaying the new top's gameobject.
    /// </summary>
    /// <returns>The original element on top of the stack.</returns>
    public MenuGrouping Pop()
    {
        MenuGrouping top = _menuGroupings.Pop();
        top.Hide();

        _menuGroupings.Peek().Display();

        return top;
    }

    #endregion

    private void Update()
    {
        if (PlayerInputHandler.Instance.PauseTriggered)
        {

        }
    }

}
