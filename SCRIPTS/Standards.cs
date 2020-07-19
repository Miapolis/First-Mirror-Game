using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Standards
{
    #region Extension Methods

    #endregion

    #region StandardMethods

    public static Vector3 V3Equal(float value)
    {
        return new Vector3(value, value, value);
    }

    public static Color RGBEqual(float value)
    {
        return new Color(value, value, value);
    }

    #endregion
}