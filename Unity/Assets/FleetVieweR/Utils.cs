using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Utils
{
    private Utils()
    {
    }

    public static String Quote(String value)
    {
        return value == null ? "null" : "\"" + value + "\"";
    }
}
