using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FilteredCardListAttribute : Attribute
{
    public string FilterPropertyName;

    public FilteredCardListAttribute(string filterPropertyName)
    {
        FilterPropertyName = filterPropertyName;
    }
}