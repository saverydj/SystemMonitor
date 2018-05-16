using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PushToElastic.StaticTools;

namespace PushToElastic
{
    public class VehicleTypeList
    {

        public int TypeEnum;
        public List<string> TypeList = new List<string>();

        private string _xmlFilePath
        {
            get { return Config.ConfigPath + Config.VehicleTypeList; }
        }

        public VehicleTypeList()
        {
            UpdateTypeList();
        }

        public void UpdateTypeList()
        {
            XDocument xml = XDocument.Load(_xmlFilePath);

            TypeList.Clear();
            foreach (XElement entry in xml.Descendants().Where(e => e.Name.LocalName == "Entry"))
            {
                TypeList.Add(entry.Value);
            }
        }

        public string GetStringValue(int index)
        {
            if (index >= 0 && index < TypeList.Count) return TypeList[index];
            return String.Empty;
        }

    }
}
