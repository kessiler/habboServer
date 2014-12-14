using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Cyber.HabboHotel.Misc
{
    class AntiMutant
    {
        // Thanks to Masacre

        private Dictionary<string, Dictionary<string, Figure>> Parts;
        public AntiMutant()
        {
            Parts = new Dictionary<string, Dictionary<string, Figure>>();
            ParseLookXMLFile();
        }

        void ParseLookXMLFile()
        {
            XDocument Doc = XDocument.Load(Core.ExtraSettings.FIGUREDATA_URL);
            var data = from item in Doc.Descendants("sets")
                       from tItem in Doc.Descendants("settype")
                       select new
                       {
                           Part = tItem.Elements("set"),
                           Type = tItem.Attribute("type"),
                       };
            foreach (var item in data)
            {
                foreach (var part in item.Part)
                {
                    string PartName = item.Type.Value;
                    if (!Parts.ContainsKey(PartName))
                        Parts.Add(PartName, new Dictionary<string, Figure>());

                    Figure toAddFigure = new Figure(PartName, part.Attribute("id").Value, part.Attribute("gender").Value,
                        part.Attribute("colorable").Value);

                    if (!Parts[PartName].ContainsKey(part.Attribute("id").Value))
                        Parts[PartName].Add(part.Attribute("id").Value, toAddFigure);
                }
            }
        }

        internal string RunLook(string Look)
        {
            List<string> toReturnFigureParts = new List<string>();
            List<string> fParts = new List<string>();
            string[] requiredParts = { "hd", "ch" };
            bool flagForDefault = false;

            string[] FigureParts = Look.Split('.');
            string genderLook = GetLookGender(Look);
            foreach (string Part in FigureParts)
            {
                string newPart = Part;
                string[] tPart = Part.Split('-');
                if (tPart.Count() < 2)
                {
                    flagForDefault = true;
                    continue;
                }
                string partName = tPart[0];
                string partId = tPart[1];

                if (!Parts.ContainsKey(partName) || !Parts[partName].ContainsKey(partId) ||
                    (genderLook != "U" && Parts[partName][partId].Gender != "U" &&
                     Parts[partName][partId].Gender != genderLook))
                    newPart = SetDefault(partName, genderLook);

                if (!fParts.Contains(partName)) fParts.Add(partName);
                if (!toReturnFigureParts.Contains(newPart)) toReturnFigureParts.Add(newPart);
            }

            if (flagForDefault)
            {
                toReturnFigureParts.Clear();
                toReturnFigureParts.AddRange("hr-115-42.hd-190-1.ch-215-62.lg-285-91.sh-290-62".Split('.'));
            }

            foreach (string requiredPart in requiredParts.Where(requiredPart => !fParts.Contains(requiredPart) &&
                                                                                !toReturnFigureParts.Contains(SetDefault(requiredPart, genderLook))))
            {
                toReturnFigureParts.Add(SetDefault(requiredPart, genderLook));
            }

            return string.Join(".", toReturnFigureParts);
        }

        private string GetLookGender(string Look)
        {
            string[] FigureParts = Look.Split('.');

            foreach (string Part in FigureParts)
            {
                string[] tPart = Part.Split('-');
                if (tPart.Count() < 2) continue;
                string partName = tPart[0];
                string partId = tPart[1];
                if (partName != "hd")
                    continue;

                return Parts.ContainsKey(partName) && Parts[partName].ContainsKey(partId)
                    ? Parts[partName][partId].Gender
                    : "U";
            }
            return "U";
        }

        private string SetDefault(string partName, string Gender)
        {
            string partId = "0";
            if (Parts.ContainsKey(partName))
            {
                KeyValuePair<string, Figure> part = Parts[partName].FirstOrDefault(x => x.Value.Gender == Gender || Gender == "U");
                partId = part.Equals(default(KeyValuePair<string, Figure>)) ? "0" : part.Key;
            }
            return partName + "-" + partId + "-0";
        }
    }

    class Figure
    {
        internal string Part;
        internal string PartId;
        internal string Gender;
        internal string Colorable;

        public Figure(string part, string partId, string gender, string colorable)
        {
            Part = part;
            PartId = partId;
            Gender = gender;
            Colorable = colorable;
        }


    }
}