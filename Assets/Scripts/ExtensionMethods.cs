using UnityEngine;

public static class ExtensionMethods { 

    /// <summary>
    /// Finds if there is a rigidbody attached to any parent.
    /// </summary>
    public static Rigidbody GetRigidbody(this Transform trans)
    {
        Rigidbody rb = trans.GetComponent<Rigidbody>();
        if (rb == null)
        {
            if (trans.parent != null)
                return trans.parent.GetRigidbody();
            else
                return null;
        } else
            return rb;
    }	

}