using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]

public class DBCompose : IGameDBHelper
{
    static Dictionary<byte, PetCompose_Table> dicPetCompose = new Dictionary<byte, PetCompose_Table>();
    static Dictionary<byte, ChangeCompose_Table> dicChangeCompose = new Dictionary<byte, ChangeCompose_Table>();
    static Dictionary<byte, PetCompose_Table> dicRideCompose = new Dictionary<byte, PetCompose_Table>();

    public static uint PET_MAX_TIER = 0;
    public static uint RIDE_MAX_TIER = 0;
    public static uint CHANGE_MAX_TIER = 0;

    public void OnReadyData()
    {
        dicPetCompose.Clear();
        dicChangeCompose.Clear();

        foreach (var iter in GameDBManager.Container.PetCompose_Table_data.Values)
        {
            if (iter.PetType == E_PetType.Pet)
            {
                if (dicPetCompose.ContainsKey(iter.PetMaterialTier))
                    continue;

                if (PET_MAX_TIER < iter.PetMaterialTier)
                    PET_MAX_TIER = iter.PetMaterialTier;

                dicPetCompose.Add(iter.PetMaterialTier, iter);
            }
            else if (iter.PetType == E_PetType.Vehicle)
            {
                if (dicRideCompose.ContainsKey(iter.PetMaterialTier))
                    continue;

                if (RIDE_MAX_TIER < iter.PetMaterialTier)
                    RIDE_MAX_TIER = iter.PetMaterialTier;

                dicRideCompose.Add(iter.PetMaterialTier, iter);
            }
        }

        foreach (var iter in GameDBManager.Container.ChangeCompose_Table_data.Values)
        {
            if (dicChangeCompose.ContainsKey(iter.ChangeMaterialTier))
                continue;

            if (CHANGE_MAX_TIER < iter.ChangeMaterialTier)
                CHANGE_MAX_TIER = iter.ChangeMaterialTier;

            dicChangeCompose.Add(iter.ChangeMaterialTier, iter);
        }
    }

    public static bool GetPetComposeTable(byte grade, out PetCompose_Table table)
    {
        return dicPetCompose.TryGetValue(grade, out table);
    }

    public static bool GetRideComposeTable(byte grade, out PetCompose_Table table)
    {
        return dicRideCompose.TryGetValue(grade, out table);
    }

    public static bool GetChangeComposeTable(byte grade, out ChangeCompose_Table table)
    {
        return dicChangeCompose.TryGetValue(grade, out table);
    }

    // 합성 가격(materialcount 충족 기준)
    public static uint GetPetComposeCost(byte grade)
    {
        if (dicPetCompose.TryGetValue(grade, out var table))
        {
            return table.PetItemCount;
        }

        return 0;
    }

    // 합성시 필요갯수
    public static int GetPetComposeCount(byte grade)
    {
        if (dicPetCompose.TryGetValue(grade, out var table))
        {
            return table.PetMaterialCount;
        }

        return 0;
    }

    // 합성 가격(materialcount 충족 기준)
    public static uint GetRideComposeCost(byte grade)
    {
        if (dicRideCompose.TryGetValue(grade, out var table))
        {
            return table.PetItemCount;
        }

        return 0;
    }

    // 합성시 필요갯수
    public static int GetRideComposeCount(byte grade)
    {
        if (dicRideCompose.TryGetValue(grade, out var table))
        {
            return table.PetMaterialCount;
        }

        return 0;
    }


    // 합성 가격(materialcount 충족 기준)
    public static uint GetChangeComposeCost(byte grade)
    {
        if (dicChangeCompose.TryGetValue(grade, out var table))
        {
            return table.ChangeItemCount;
        }

        return 0;
    }

    // 합성시 필요갯수
    public static int GetChangeComposeCount(byte grade)
    {
        if (dicChangeCompose.TryGetValue(grade, out var table))
        {
            return table.ChangeMaterialCount;
        }

        return 0;
    }
}