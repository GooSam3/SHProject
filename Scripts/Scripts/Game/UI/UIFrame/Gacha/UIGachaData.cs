public class UIGachaData
{
	public static string GetVideoName(UIGachaEnum.E_GachaStyle _gachaStyle, UIGachaEnum.E_VideoType _videoType)
	{
		string name = string.Empty;

		switch(_gachaStyle)
		{
			case UIGachaEnum.E_GachaStyle.Pet:
				switch (_videoType)
				{
					case UIGachaEnum.E_VideoType.Pet_Start: name = "Gacha_Pet_00_v"; break;
					case UIGachaEnum.E_VideoType.Pet_End: name = "Gacha_Pet_01_v"; break;
					case UIGachaEnum.E_VideoType.Pet_Playing_1: name = "Gacha_Pet_00_1_v"; break;
					case UIGachaEnum.E_VideoType.Pet_Playing_2: name = "Gacha_Pet_00_2_v"; break;
					case UIGachaEnum.E_VideoType.Pet_Playing_3: name = "Gacha_Pet_00_3_v"; break;
				}
				break;
			case UIGachaEnum.E_GachaStyle.Class:
				switch (_videoType)
				{
					case UIGachaEnum.E_VideoType.Class_Start: name = "Gacha_Trans_01_v"; break;
					case UIGachaEnum.E_VideoType.Class_Die_1: name = "Gacha_Trans_02_v"; break;
					case UIGachaEnum.E_VideoType.Class_Die_2: name = "Gacha_Trans_02_1_v"; break;
					case UIGachaEnum.E_VideoType.Class_Skill_1: name = "Gacha_Trans_Skill01_v"; break;
					case UIGachaEnum.E_VideoType.Class_Skill_2: name = "Gacha_Trans_Skill02_v"; break;
				}
				break;
			case UIGachaEnum.E_GachaStyle.Ride:
				switch (_videoType)
				{
					case UIGachaEnum.E_VideoType.Ride_Start: name = "Gacha_Vehicle_00_v"; break;
					case UIGachaEnum.E_VideoType.Ride_End: name = "Gacha_Vehicle_01_v"; break;
					case UIGachaEnum.E_VideoType.Ride_Playing_1: name = "Gacha_Vehicle_00_1_v"; break;
					case UIGachaEnum.E_VideoType.Ride_Playing_2: name = "Gacha_Vehicle_00_2_v"; break;
					case UIGachaEnum.E_VideoType.Ride_Playing_3: name = "Gacha_Vehicle_00_3_v"; break;
				}
				break;
			case UIGachaEnum.E_GachaStyle.Item:
				{
					switch(_videoType)
					{
						case UIGachaEnum.E_VideoType.Item_Normal: name = "Gacha_Weapon_01_v"; break;
						case UIGachaEnum.E_VideoType.Item_Hidden: name = "Gacha_Weapon_03_v"; break;
					}
				}
				break;
		}

		return name;
	}

	public static string GetTimeLineName(UIGachaEnum.E_GachaStyle _gachaStyle, UIGachaEnum.E_TimeLineType _timeLineType)
	{
		string name = string.Empty;

		switch (_gachaStyle)
		{
			case UIGachaEnum.E_GachaStyle.Pet:
				switch (_timeLineType)
				{
					case UIGachaEnum.E_TimeLineType.Pet_1_Start: name = "Gacha_PetCard_1"; break;
					case UIGachaEnum.E_TimeLineType.Pet_10_Start: name = "Gacha_PetCard_10"; break;
					case UIGachaEnum.E_TimeLineType.Pet_11_Start: name = "Gacha_PetCard_11"; break;
					case UIGachaEnum.E_TimeLineType.Pet_Change: name = "Gacha_PetCard_Change"; break;
				}
				break;
			case UIGachaEnum.E_GachaStyle.Class:
                switch (_timeLineType)
                {
					case UIGachaEnum.E_TimeLineType.Class_1_Start: name = "Gacha_TransCard_1"; break;
					case UIGachaEnum.E_TimeLineType.Class_10_Start: name = "Gacha_TransCard_10"; break;
                    case UIGachaEnum.E_TimeLineType.Class_11_Start: name = "Gacha_TransCard_11"; break;
					case UIGachaEnum.E_TimeLineType.Class_Change: name = "Gacha_TransCard_Change"; break;
                }
                break;
			case UIGachaEnum.E_GachaStyle.Ride:
				switch (_timeLineType)
				{
					case UIGachaEnum.E_TimeLineType.Ride_1_Start: name = "Gacha_VehicleCard_1"; break;
					case UIGachaEnum.E_TimeLineType.Ride_10_Start: name = "Gacha_VehicleCard_10"; break;
					case UIGachaEnum.E_TimeLineType.Ride_11_Start: name = "Gacha_VehicleCard_11"; break;
					case UIGachaEnum.E_TimeLineType.Ride_Change: name = "Gacha_VehicleCard_Change"; break;
				}
				break;
			case UIGachaEnum.E_GachaStyle.Item:
				{
					switch (_timeLineType)
					{
						case UIGachaEnum.E_TimeLineType.Item_1_Start: name = "Gacha_Weapon_02"; break;
						case UIGachaEnum.E_TimeLineType.Item_1_End: name = "Gacha_WeaponCard_1"; break;
						case UIGachaEnum.E_TimeLineType.Item_11_Start: name = "Gacha_Weapon_01"; break;
						case UIGachaEnum.E_TimeLineType.Item_11_End: name = "Gacha_WeaponCard_11"; break;
						case UIGachaEnum.E_TimeLineType.Item_Gold: name = "Gacha_Weapon_03"; break;
					}
				}
				break;
        }

        return name;
    }
}