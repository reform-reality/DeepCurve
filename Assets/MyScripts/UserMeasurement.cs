using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UserMeasurement {
    
    private static float S_MAX = 1.65f;
    
    private static float M_MAX = 1.73f;
    
    private static float L_MAX = 1.80f;

    private static float XL_MAX = 1.85f;

    //in meters
    public static float height { get; set; }

    //shoulder dist in centimeters
    public static float width { get; set; }

    //upper height in centimeters
    public static float lenght { get; set; }


    public static string getUserClothSize_old()
    {
        if (height < S_MAX)
            return "SMALL";
        else if (height <= M_MAX)
            return "MEDIUM";
        else if (height <= L_MAX)
            return "LARGE";
        else if (height <= XL_MAX)
            return "X-LARGE";
        else
            return "XX-LARGE";
    }

    public static string getUserClothSize()
    {
        if (lenght <= 71.12) //28 inchs
            return "SMALL";
        else if (lenght <= 73.66) //29 inchs
            return "MEDIUM";
        else if (lenght <= 76.2) //30 inchs
            return "LARGE";
        else if (lenght <= 78.74) //31 inchs
            return "X-LARGE";
        else
            return "XX-LARGE";
    }

}