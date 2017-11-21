using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class ConfigTable
{
	private Dictionary<int, ConfigItem> m_ID2ConfigItem = new Dictionary<int, ConfigItem>();

	public ConfigItem GetConfig (int id)
	{
        if (m_ID2ConfigItem.ContainsKey(id))
            return m_ID2ConfigItem[id];
		else
			return null;
	}
}


