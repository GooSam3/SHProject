public class UIGachaEnum
{
	public enum E_GachaStyle
	{
		None = 0,

		Pet = 1,
		Class = 2,
		Ride = 3,
		Item = 4,
	}

	public enum E_StateObjectType
	{
		TimeLine = 0,
		Video = 1,
	}

	public enum E_VideoType
	{
		Item_Normal = 0,
		Item_Hidden = 1,

		Ride_Start = 10,
		Ride_End = 11,
		Ride_Playing_1 = 12,
		Ride_Playing_2 = 13,
		Ride_Playing_3 = 14,

		Class_Start = 20,
		Class_Die_1 = 21,
		Class_Die_2 = 22,
		Class_Skill_1 = 23,
		Class_Skill_2 = 24,

		Pet_Start = 30,
		Pet_End = 31,
		Pet_Playing_1 = 32,
		Pet_Playing_2 = 33,
		Pet_Playing_3 = 34,
	}

	public enum E_TimeLineType
	{
		None = 0,

		Item_1_Start = 1,
		Item_1_End = 2,
		Item_11_Start = 3,
		Item_11_End = 4,
		Item_Gold = 5,

		Ride_1_Start = 10,
		Ride_10_Start = 11,
		Ride_11_Start = 12,
		Ride_Change = 13,
		
		Class_1_Start = 20,
		Class_10_Start = 21,
		Class_11_Start = 22,
		Class_Change = 23,

		Pet_1_Start = 30,
		Pet_10_Start = 31,
		Pet_11_Start = 32,
		Pet_Change = 33,
	}
}