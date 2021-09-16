using System;

[Serializable]
public readonly struct NonSerializedDrawer
{
    public readonly string[] members;

    public NonSerializedDrawer(params string[] members)
    {
        this.members = members;
    }
}