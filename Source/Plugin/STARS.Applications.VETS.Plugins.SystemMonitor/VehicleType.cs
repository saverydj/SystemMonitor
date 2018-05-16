using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    public class VehicleType
    {

        public int TypeEnum;
        public List<string> TypeList = new List<string>();

        private string _xmlFilePath
        {
            get { return Config.EnumerationsPath + Config.VehicleTypeList; }
        }

        public VehicleType()
        {
            UpdateTypeList();
            SetAsNoRunningTest();
        }

        public void UpdateTypeList()
        {
            try
            {
                XDocument xml = XDocument.Load(_xmlFilePath);

                List<string> typeListTemp = new List<string>();
                foreach (XElement entry in xml.Descendants().Where(e => e.Name.LocalName == "Entry"))
                {
                    typeListTemp.Add(entry.Value);
                }

                //Leave the existing typeList unchanged if we break while reading the xml
                TypeList.Clear();
                foreach (string listItem in typeListTemp)
                {
                    TypeList.Add(listItem);
                }
            }
            catch (Exception e)
            {
                SystemLogService.DisplayErrorInVETSLogNoReturn(String.Format("Error reading file '{0}'. Message was: {1}", _xmlFilePath, e.Message));
            }
        }

        public void SetByString(string typeString)
        {
            UpdateTypeList();
            SetAsOther();
            for (int i = 0; i < TypeList.Count; i++)
            {
                if (TypeList[i] == typeString)
                {
                    TypeEnum = i;
                    return;
                }
            }
        }

        public string GetStringValue()
        {
            if (TypeEnum >= 0 && TypeEnum < TypeList.Count) return TypeList[TypeEnum];
            return String.Empty;
        }

        public void SetAsOther()
        {
            TypeEnum = TypeList.Count - 1;
        }

        public void SetAsNoRunningTest()
        {
            TypeEnum = TypeList.Count - 2;
        }

    }
}
