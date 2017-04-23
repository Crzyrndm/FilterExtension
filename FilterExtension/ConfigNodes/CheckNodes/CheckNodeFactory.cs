namespace FilterExtensions.ConfigNodes.CheckNodes
{
    public static class CheckNodeFactory
    {
        public static CheckNode MakeCheck(ConfigNode node)
        {
            string checkID = node.GetValue("type");
            switch (checkID)
            {
                case CheckName.ID:
                    return new CheckName(node);
                case CheckTitle.ID:
                    return new CheckTitle(node);
                case CheckTech.ID:
                    return new CheckTech(node);
                case CheckManufacturer.ID:
                    return new CheckManufacturer(node);
                case CheckFolder.ID:
                    return new CheckFolder(node);
                case CheckPath.ID:
                    return new CheckPath(node);
                case CheckCategory.ID:
                    return new CheckCategory(node);
                case CheckSubcategory.ID:
                    return new CheckSubcategory(node);
                case CheckField.ID:
                    return new CheckField(node);
                case CheckModuleName.ID:
                    return new CheckModuleName(node);
                case CheckModuleTitle.ID:
                    return new CheckModuleTitle(node);
                case CheckProfile.ID:
                    return new CheckProfile(node);
                case CheckTag.ID:
                    return new CheckTag(node);
                case CheckResource.ID:
                    return new CheckResource(node);
                case CheckPropellant.ID:
                    return new CheckPropellant(node);
                case CheckGroup.ID:
                    return new CheckGroup(node);
                case CheckSize.ID:
                    return new CheckSize(node);
                case CheckCrew.ID:
                    return new CheckCrew(node);
                case CheckMass.ID:
                    return new CheckMass(node);
                case CheckCrash.ID:
                    return new CheckCrash(node);
                case CheckMaxTemp.ID:
                    return new CheckMaxTemp(node);
                case CheckCustom.ID:
                    return new CheckCustom(node);
                case CheckCost.ID:
                    return new CheckCost(node);
                default:
                    LoadAndProcess.Log($"unknown check type {checkID}", LoadAndProcess.LogLevel.Error);
                    return null;
            }
        }

        public static ConfigNode MakeCheckNode(string type, string values, bool invert = false, bool contains = true, CompareType equality = CompareType.Equals, bool exact = false)
        {
            ConfigNode tmpNode = new ConfigNode("CHECK");
            tmpNode.AddValue("type", type);
            tmpNode.AddValue("value", values);
            tmpNode.AddValue("invert", invert);
            tmpNode.AddValue("contains", contains);
            tmpNode.AddValue("equality", equality.ToString());
            tmpNode.AddValue("exact", exact);
            return tmpNode;
        }
    }
}