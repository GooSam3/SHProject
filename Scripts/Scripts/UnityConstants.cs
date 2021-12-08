// This file is auto-generated. Modifications are not saved.

namespace UnityConstants
{
    public static class Tags
    {
        public const string Untagged = "Untagged";
        public const string Respawn = "Respawn";
        public const string Finish = "Finish";
        public const string EditorOnly = "EditorOnly";
        public const string MainCamera = "MainCamera";
        public const string Player = "Player";
        public const string GameController = "GameController";
        public const string UIRoot = "UIRoot";
        public const string MAST_Holder = "MAST_Holder";
    }

    public static class SortingLayers
    {
        public const int Default = 0;
        public const int UIBack = -794112165;
        public const int UI = 1733819345;
        public const int UIFront = 428657645;
    }

    public static class Layers
    {
        public const int Default = 0;
        public const int TransparentFX = 1;
        public const int Ignore_Raycast = 2;
        public const int Water = 4;
        public const int UI = 5;
        public const int PostProcessing = 8;
        public const int Floor = 10;
        public const int UIModel = 11;
        public const int Gimmick = 14;
        public const int Entity = 15;
        public const int Player = 16;
        public const int MiniGameObject = 20;
        public const int EventTrigger = 21;
        public const int IgnoreCollision = 28;
        public const int UIFront = 30;
        public const int UI3DOverlay = 31;


        public const int DefaultMask = 1 << 0;
        public const int TransparentFXMask = 1 << 1;
        public const int Ignore_RaycastMask = 1 << 2;
        public const int WaterMask = 1 << 4;
        public const int UIMask = 1 << 5;
        public const int PostProcessingMask = 1 << 8;
        public const int FloorMask = 1 << 10;
        public const int UIModelMask = 1 << 11;
        public const int GimmickMask = 1 << 14;
        public const int EntityMask = 1 << 15;
        public const int PlayerMask = 1 << 16;
        public const int MiniGameObjectMask = 1 << 20;
        public const int EventTriggerMask = 1 << 21;
        public const int IgnoreCollisionMask = 1 << 28;
        public const int UIFrontMask = 1 << 30;
        public const int UI3DOverlayMask = 1 << 31;

		public static int OnlyIncluding( params int[] layers )
		{
			int mask = 0;
			for( var i = 0; i < layers.Length; i++ )
				mask |= ( 1 << layers[i] );

			return mask;
		}

		public static int EverythingBut( params int[] layers )
		{
			return ~OnlyIncluding( layers );
		}
    }

    public static class Scenes
    {
        public const int Start = 0;
        public const int LoadingScreen = 1;
    }

    public static class Axes
    {
        public const string Horizontal = "Horizontal";
        public const string Vertical = "Vertical";
        public const string Fire1 = "Fire1";
        public const string Fire2 = "Fire2";
        public const string Fire3 = "Fire3";
        public const string Jump = "Jump";
        public const string Mouse_X = "Mouse X";
        public const string Mouse_Y = "Mouse Y";
        public const string Mouse_ScrollWheel = "Mouse ScrollWheel";
        public const string Submit = "Submit";
        public const string Cancel = "Cancel";
        public const string Enable_Debug_Button_1 = "Enable Debug Button 1";
        public const string Enable_Debug_Button_2 = "Enable Debug Button 2";
        public const string Debug_Reset = "Debug Reset";
        public const string Debug_Next = "Debug Next";
        public const string Debug_Previous = "Debug Previous";
        public const string Debug_Validate = "Debug Validate";
        public const string Debug_Persistent = "Debug Persistent";
        public const string Debug_Multiplier = "Debug Multiplier";
        public const string Debug_Horizontal = "Debug Horizontal";
        public const string Debug_Vertical = "Debug Vertical";
    }
}

