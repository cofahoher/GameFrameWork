using System.Collections;
using System.Collections.Generic;
using System.Xml;

public abstract class ConfigItem
{
    public int ID = -1;
    
    public virtual bool TryParse(XmlNode node)
    {
        return true;
    }
}