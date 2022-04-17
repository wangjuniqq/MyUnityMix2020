using System;
using UnityEngine;

[System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class HeaderAttribute : PropertyAttribute
{
    public readonly string header;
    public readonly string colorString;
    public readonly Color color;
    public readonly float textHegihtIncrease;


    public HeaderAttribute(string header, string colorString) : this(header, 1, colorString)
    {
    }

    public HeaderAttribute(string header, float textHegihtIncrease = 1, string colorString = "lightblue")
    {
        this.header = header;
        this.colorString = colorString;
        this.textHegihtIncrease = Mathf.Max(1, textHegihtIncrease);

        if (ColorUtility.TryParseHtmlString(colorString, out this.color))
        {
            return;
        }

        this.color = new Color(173, 216, 230);
        this.colorString = "lightblue";
    }
}