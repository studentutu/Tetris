using UnityEngine;

public abstract class BasePanel : MonoBehaviour
{

    [HideInInspector]
    public int panelType;

    /// <summary>
    /// if need put the panel in stack, set it true
    /// </summary>
    [HideInInspector]
    public bool stack = false;


    public abstract BasePanel Show();

    /// <summary>
    /// update data
    /// </summary>
    public virtual void UpdatePanelInfo(int[] info) { }

    /// <summary>
    /// Data initialization, button monitoring
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// Hide + (stack pop)
    /// </summary>
    public abstract void Hide();

    public abstract void Destroy();
}
