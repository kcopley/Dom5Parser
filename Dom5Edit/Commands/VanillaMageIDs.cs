using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit
{
    public class VanillaMageIDs
    {
        private static Dictionary<string, List<int>> nationToIDs = new Dictionary<string, List<int>>();

        static VanillaMageIDs()
        {
            nationToIDs.Add("EA Arcosephale", new List<int>() { 1075, 1650, 3197, 1606, 1073 });
            nationToIDs.Add("EA Ermor", new List<int>() { 1109, 1110, 1111, 1112, 1113, 1114, 1115 });
            nationToIDs.Add("EA Ulm", new List<int>() { 1159, 1160, 1161 });
            nationToIDs.Add("EA Marverni", new List<int>() { 1204, 1205, 1206, 1225, 2468 });
            nationToIDs.Add("EA Sauromatia", new List<int>() { 1174, 1178, 1180, 1181, 1186, 1188 });
            nationToIDs.Add("EA T'ien Ch'i", new List<int>() { 806, 940, 941 });
            nationToIDs.Add("EA Machaka", new List<int>() { 2291, 2292, 2293, 2295, 2296, 2297, 2403 });
            nationToIDs.Add("EA Mictlan", new List<int>() { 733, 735 });
            nationToIDs.Add("EA Abysia", new List<int>() { 1536, 1538, 1542, 1698, 1699 });
            nationToIDs.Add("EA Caelum", new List<int>() { 1286, 1663, 2557, 2570 });
            nationToIDs.Add("EA C'tis", new List<int>() { 161, 1366, 1387 });
            nationToIDs.Add("EA Pangaea", new List<int>() { 1535, 2187, 2487 });
            nationToIDs.Add("EA Agartha", new List<int>() { 1457, 1460, 1467, 1468, 2491, 2493, 2498, 2499, 2500, 2501 });
            nationToIDs.Add("EA Tir na n'Og", new List<int>() { 1775, 1774, 1773, 1759, 1754, 1752 });
            nationToIDs.Add("EA Fomoria", new List<int>() { 1791, 1792, 1802, 1818, 1819 });
            nationToIDs.Add("EA Helheim", new List<int>() { 847, 1010, 1502, 1505, 1506, 1507 });
            nationToIDs.Add("EA Niefelheim", new List<int>() { 553, 785, 844, 1299, 1300 });
            nationToIDs.Add("EA Rus", new List<int>() { 2986, 2987, 2988, 2999, 3000, 3001 });
            nationToIDs.Add("EA Kailasa", new List<int>() { 2542 });
            nationToIDs.Add("EA Lanka", new List<int>() { 1733, 1734, 1735, 1738, 1739 });
            nationToIDs.Add("EA Yomi", new List<int>() { 1276, 1314, 1315, 1609, 3069, 3070, 1432 });
            nationToIDs.Add("EA Hinnom", new List<int>() { 2013, 2014, 2016, 2017, 2020, 2031, 2032, 2033 });
            nationToIDs.Add("EA Ur", new List<int>() { 2167, 2171, 2179, 2180, 2181, 2182, 2268, 2269 });
            nationToIDs.Add("EA Berytos", new List<int>() { 2263, 2264, 2265, 2266, 2424 });
            nationToIDs.Add("EA Xibalba", new List<int>() { 2677, 2678, 2679, 2680, 2681, 2684, 2736 });
            nationToIDs.Add("EA Mekone", new List<int>() { 3112, 3114, 3115, 3116, 3117, 3118 });
            nationToIDs.Add("EA Ubar", new List<int>() { 3460, 3461, 3462, 3465, 3468, 3469, 3472 });
            nationToIDs.Add("EA Atlantis", new List<int>() { 1692, 1693, 1694, 1695, 1702 });
            nationToIDs.Add("EA R'lyeh", new List<int>() { 1401, 1520, 1521, 2883, 2885, 2886 });
            nationToIDs.Add("EA Pelagia", new List<int>() { 2395, 2396, 2397, 2805, 2814 });
            nationToIDs.Add("EA Therodos", new List<int>() { 2833, 2834, 2835, 2836, 2845 });
            nationToIDs.Add("MA Arcosephale", new List<int>() { 242, 301, 3198 });
            nationToIDs.Add("MA Ermor", new List<int>() { 253, 254, 256, 257, 258 });
            nationToIDs.Add("MA Sceleria", new List<int>() { 669, 670, 2244 });
            nationToIDs.Add("MA Pythium", new List<int>() { 41, 42, 43, 52 });
            nationToIDs.Add("MA Man", new List<int>() { 60, 151, 152, 153, 658, 2439 });
            nationToIDs.Add("MA Eriu", new List<int>() { 848, 850, 856, 1774, 1784, 2425 });
            nationToIDs.Add("MA Ulm", new List<int>() { 325, 1973, 1974, 1982 });
            nationToIDs.Add("MA Marignon", new List<int>() { 148, 149, 222, 223, 224, 225, 440 });
            nationToIDs.Add("MA Mictlan", new List<int>() { 1189, 1190, 1191, 1192, 1193, 1194, 1888, 1907 });
            nationToIDs.Add("MA T'ien Ch'i", new List<int>() { 803, 804, 807, 808, 1890, 1891, 1892, 1893, 1894 });
            nationToIDs.Add("MA Machaka", new List<int>() { 891, 892, 893, 894, 895, 896, 897 });
            nationToIDs.Add("MA Agartha", new List<int>() { 1459, 1473, 1474, 1475, 1499, 2527 });
            nationToIDs.Add("MA Abysia", new List<int>() { 85, 86, 87, 89, 923 });
            nationToIDs.Add("MA Caelum", new List<int>() { 202, 203, 204, 1283 });
            nationToIDs.Add("MA C'tis", new List<int>() { 937 });
            nationToIDs.Add("MA Pangaea", new List<int>() { 237, 238, 516 });
            nationToIDs.Add("MA Asphodel", new List<int>() { 709, 710, 711, 714, 901, 2311, 2312, 2480 });
            nationToIDs.Add("MA Jotunheim", new List<int>() { 913, 3397, 3398, 3399, 3429 });
            nationToIDs.Add("MA Vanarus", new List<int>() { 1938, 1939, 2341, 2342, 2356, 2357 });
            nationToIDs.Add("MA Bandar Log", new List<int>() { 1144 });
            nationToIDs.Add("MA Shinuyama", new List<int>() { 1427, 1429, 1608, 3262 });
            nationToIDs.Add("MA Ashdod", new List<int>() { 2011, 2027, 2028, 2029, 2038, 2039, 2045, 2060 });
            nationToIDs.Add("MA Uruk", new List<int>() { 2943, 2944, 2945, 2946, 2947, 2948, 2949, 2950, 2951, 2952, 2954 });
            nationToIDs.Add("MA Nazca", new List<int>() { 2656, 2657, 2658, 2659, 2661, 2662, 2663, 2664, 2665 });
            nationToIDs.Add("MA Xibalba", new List<int>() { 2682, 2714, 2715, 2716, 2717, 2718, 2719 });
            nationToIDs.Add("MA Phlegra", new List<int>() { 3130, 3131, 3138, 3139, 3141, 3161, 3162 });
            nationToIDs.Add("MA Phaecia", new List<int>() { 3151, 3152, 3155, 3156, 3157, 3158, 3166 });
            nationToIDs.Add("MA Ind", new List<int>() { 3284, 3285, 3286, 3287, 3288, 3289, 3290, 3291, 3297, 3301, 3314, 3317, 3318, 3321, 3322 });
            nationToIDs.Add("MA Na'Ba", new List<int>() { 3339, 3340, 3341, 3343, 3353, 3354, 3357, 3358 });
            nationToIDs.Add("MA Atlantis", new List<int>() { 102, 104, 112, 322, 441, 2859, 3096 });
            nationToIDs.Add("MA Pelagia", new List<int>() { 1088, 1417, 1418, 2422, 2423, 2823, 2865, 2868 });
            nationToIDs.Add("MA Ys", new List<int>() { 2901, 2914, 2917, 2919, 2921 });
            nationToIDs.Add("LA Arcosephale", new List<int>() { 1557, 3128, 3199, 3200 });
            nationToIDs.Add("LA Pythium", new List<int>() { 761, 830, 1872, 1873, 1874, 1875, 1876, 1877, 1878, 1880, 2151 });
            nationToIDs.Add("LA Lemuria", new List<int>() { 679, 680, 681, 2333, 2334, 2335 });
            nationToIDs.Add("LA Man", new List<int>() { 1643, 1644, 1645, 1646, 1647, 1666, 1776 });
            nationToIDs.Add("LA Ulm", new List<int>() { 739, 740, 1011, 1012, 1019, 1023, 1035, 1237, 3244, 3245, 3251, 3252, 3253, 3255 });
            nationToIDs.Add("LA Marignon", new List<int>() { 626, 744, 745, 1031, 1032, 1033, 2198, 2199, 2200, 2428, 3419 });
            nationToIDs.Add("LA Mictlan", new List<int>() { 1420, 1421, 1424, 2892, 2894, 2895 });
            nationToIDs.Add("LA T'ien Ch'i", new List<int>() { 1709, 1710, 1711, 1712, 1895 });
            nationToIDs.Add("LA Jomon", new List<int>() { 1254, 1255, 1258, 1259, 2098, 2104 });
            nationToIDs.Add("LA Agartha", new List<int>() { 1438, 1442, 1443, 1476, 2456, 2521 });
            nationToIDs.Add("LA Abysia", new List<int>() { 991, 1091, 1092, 1965, 1966, 1967, 1969, 1970 });
            nationToIDs.Add("LA Caelum", new List<int>() { 951, 1003, 1004, 1281, 1282 });
            nationToIDs.Add("LA C'tis", new List<int>() { 690, 691, 692, 1036, 1095, 2314 });
            nationToIDs.Add("LA Pangaea", new List<int>() { 703, 705, 706, 2479 });
            nationToIDs.Add("LA Midgard", new List<int>() { 263, 264, 846, 950, 2140 });
            nationToIDs.Add("LA Utgard", new List<int>() { 280, 281, 2149, 3432 });
            nationToIDs.Add("LA Bogarus", new List<int>() { 1919, 1920, 1932, 1933, 1934, 1935, 1936, 1937, 1960, 1961 });
            nationToIDs.Add("LA Patala", new List<int>() { 1320, 1321, 1322, 1323, 1324, 1325 });
            nationToIDs.Add("LA Gath", new List<int>() { 1984, 1986, 1987, 1988, 1997, 2001, 2012, 2069 });
            nationToIDs.Add("LA Ragha", new List<int>() { 2599, 2600, 2601, 2602, 2603, 2604, 2605, 2606 });
            nationToIDs.Add("LA Xibalba", new List<int>() { 2748, 2749, 2750, 2751, 2753, 2754 });
            nationToIDs.Add("LA Phlegra", new List<int>() { 3218, 3220, 3221, 3222, 3225, 3228 });
            nationToIDs.Add("LA Vaettiheim", new List<int>() { 3407, 3408, 3409, 3411 });
            nationToIDs.Add("LA Atlantis", new List<int>() { 1618, 1623, 2083, 2084, 2088 });
            nationToIDs.Add("LA R'lyeh", new List<int>() { 1562, 1563 });
            nationToIDs.Add("LA Erytheia", new List<int>() { 3032, 3033, 3034, 3035, 3038, 3039, 3041, 3043, 3045, 3044, 3042, 3050 });
        }

        public static bool TryGetIDList(string nation, out List<int> ids)
        {
            return nationToIDs.TryGetValue(nation, out ids);
        }
    }
}
