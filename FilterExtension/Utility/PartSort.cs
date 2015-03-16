using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FilterExtensions.Utility
{
    static class PartSort
    {
        public static void chooseSort(string sortID, List<AvailablePart> parts, bool ascending = true)
        {
            switch (sortID)
            {
                case "Name":
                    sortByTitle(parts, ascending);
                    break;
                case "cfgName":
                    sortByName(parts, ascending);
                    break;
                case "Mass":
                    sortByMass(parts, ascending);
                    break;
                case "Cost":
                    sortByCost(parts, ascending);
                    break;
                case "Size":
                    sortBySize(parts, ascending);
                    break;
            }
        }

        public static void sortByTitle(List<AvailablePart> partsToSort, bool ascending)
        {
            if (ascending)
                partsToSort.Sort((p1, p2) => p1.title.CompareTo(p2.title));
            else
                partsToSort.Sort((p1, p2) => p2.title.CompareTo(p1.title));
        }

        public static void sortByName(List<AvailablePart> partsToSort, bool ascending)
        {
            if (ascending)
                partsToSort.Sort((p1, p2) => p1.name.CompareTo(p2.name));
            else
                partsToSort.Sort((p1, p2) => p2.name.CompareTo(p1.name));
        }

        public static void sortByMass(List<AvailablePart> partsToSort, bool ascending)
        {
            if (ascending)
                partsToSort.Sort((p1, p2) => p1.partPrefab.mass.CompareTo(p2.partPrefab.mass));
            else
                partsToSort.Sort((p1, p2) => p2.partPrefab.mass.CompareTo(p1.partPrefab.mass));
        }

        public static void sortByCost(List<AvailablePart> partsToSort, bool ascending)
        {
            if (ascending)
                partsToSort.Sort((p1, p2) => p1.cost.CompareTo(p2.cost));
            else
                partsToSort.Sort((p1, p2) => p2.cost.CompareTo(p1.cost));
        }

        public static void sortBySize(List<AvailablePart> partsToSort, bool ascending)
        {
            if (ascending)
                partsToSort.Sort((p1, p2) => p1.partSize.CompareTo(p2.partSize));
            else
                partsToSort.Sort((p1, p2) => p2.partSize.CompareTo(p1.partSize));
        }
    }
}
