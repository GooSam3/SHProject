using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임상에서 쓰일 설정값들 모음.
/// </summary>
[UnityEngine.Scripting.Preserve]
public class DBSample : IGameDBHelper
{
    #region :: Global Constant ::
    /// <summary>[로컬 플레이어] 이펙트 보여주는 범위</summary>
#if UNITY_EDITOR
    public const float EffectShowInSqrDistance = 100f * 100f;
#else
    public const float EffectShowInSqrDistance = 20f * 20f;
#endif
    #endregion

    /// <summary> 발사체 공통 속도값 </summary>
    public static int Projectile_Speed { get; private set; } = 20;
	/// <summary> 기본 타겟 찾기 거리 </summary>
	public static float SearchTargetRange = 15f;
	/// <summary> 자동모드에서 배회할 거리 </summary>
	public static float WanderRadius = 7f;
	/// <summary> 자동사냥시, 몬스터 탐색 범위</summary>
	public static float Auto_Search_Range = 20f;
	/// <summary> AI가 타겟 공격시도 하는 도중 일정시간이내로 공격못하면, 어그로 변경하기</summary>
	public static float MobAI_NoAttack_Time = 5f;
	/// <summary> 현상범(보라돌이) 유지 시간</summary>
	public static uint Offender_Time = 10_000;
	/// <summary> 어그로 최대 유지 시간 (단위:timestamp) </summary>
	public static ulong MaxAggroHoldingTime = 10_000;
	/// <summary> 원거리 공격에 의해 멈추는 시간 </summary>
	public static float PauseTimeByHit = 0.1f;
	/// <summary> 원거리 공격에 의한 경직 이뮨 시간 </summary>
	public static float Stiff_Immune_Time = 0.5f;

	/// <summary> 로컬플레이어 생성후, 네트워크 상 객체들 동기화 받는 타이밍 딜레이 시키고 싶을때 사용 (첫 진입 네트워크 부하를 줄이기 위한 변수) </summary>
	public static float MyPlayer_SyncAllDelay = 0f;

	/// <summary> 현재 게임에서 사용되는 튜토리얼 스테이지 </summary>
	public static uint TutorialStageTID { get; private set; }
	/// <summary> 대련장 </summary>
	public static uint TrainingStageTID { get; private set; } = 10002;
	/// <summary> 펫주문서 사용시 AbilityActionID가 하나도 없어서, 동기화 용도 </summary>
	public static uint PetScroll_AbilityActionTID { get; private set; } = 700000;

	/// <summary>시작 시 인벤토리 개수</summary>
	public static int Start_Inventory { get; private set; }
	/// <summary>시작 시 창고 개수</summary>
	public static int Start_Storage_Count { get; private set; }

	/// <summary>캐릭터 이름 최소 만족해야하는 글자 수</summary>
	public static int NickName_Length_Min { get; private set; }
	/// <summary>캐릭터 이름 최대 설정 가능 글자 수</summary>
	public static int NickName_Length_Max { get; private set; }

	/// <summary>쪽지 제목 제한 글자(한글)</summary>
	public static int Message_Title_Text_Max { get; private set; }
	/// <summary>쪽지 내용 제한 글자(한글)</summary>
	public static int Message_Content_Text_Max { get; private set; }
	/// <summary>쪽지 이용 가능 레벨</summary>
	public static uint Message_Use_Level { get; private set; }
	/// <summary>쪽지 보내기 수수료(골드)</summary>
	public static uint Message_Gold_Cost { get; private set; }
	/// <summary>쪽지 보내기 수수료(골드)</summary>
	public static uint Guild_Message_Gold_Cost { get; private set; }
	/// <summary>일일 개인 쪽지 발송 제한 수</summary>
	public static uint Day_Message_Send_Limit { get; private set; }
	/// <summary>일일 길드 쪽지 발송 제한 수</summary>
	public static uint Day_GuildMessage_Send_Limit { get; private set; }
	
	/// <summary>이벤트 초기화 시간</summary>
	public static uint Event_Reset_Time { get; private set; }
	/// <summary>골드 아이템 아이디</summary>
	public static uint Gold_ID { get; private set; }
    /// <summary>룬 해제 시 소모되는 아이템 아이디(골드)</summary>
	public static uint Runelift_Use_Item { get; private set; }
    /// <summary>경험치 아이템 아이디</summary>
	public static uint Exp_Item { get; private set; }
    /// <summary>다이아 아이템 아이디</summary>
    public static uint Diamond_ID { get; private set; }
    /// <summary>거래소 거래 재화</summary>
    public static uint Exchange_Goods { get; private set; }
    /// <summary>거래소 수수료 재화</summary>
    public static uint Exchange_Commission_Goods { get; private set; }
    /// <summary>개인거래 거래 재화</summary>
    public static uint Private_Transaction_Trade_Goods { get; private set; }
    /// <summary>개인거래 거래 등록 수수료</summary>
    public static uint Private_Transaction_Commission_Goods { get; private set; }
    /// <summary>변신 소모되는 아이템의 아이디</summary>
    public static uint Change_Use_Item { get; private set; }
	/// <summary>펫 소환 시 소모되는 아이템 아이디</summary>
	public static uint Pet_Summon_Item { get; private set; }
    /// <summary>루비 아이템 아이디</summary>
    public static uint Ruby_ID { get; private set; }

    /// <summary>변신 합성 실패시, 누적되는 마일리지 샵 아이디</summary>    
    public static uint Mileage_Change5T_ShopID { get; private set; }
    public static uint Mileage_Change6T_ShopID { get; private set; }
    public static uint Mileage_Change7T_ShopID { get; private set; }
    /// <summary>펫 합성 실패시, 누적되는 마일리지 샵 아이디</summary>    
    public static uint Mileage_Pet5T_ShopID { get; private set; }
    public static uint Mileage_Pet6T_ShopID { get; private set; }
    public static uint Mileage_Pet7T_ShopID { get; private set; }

    /// <summary>문양 오픈/사용 가능 레벨</summary>
    public static uint Mark_Use_Level { get; private set; }
    /// <summary>문양 강화시 필요 아이템 아이디</summary>
    public static uint MarkEnchant_Use_Item { get; private set; }
	/// <summary>문양 강화시 필요 아이템 갯수 (문양 강화 n차시도시 아이템소모비용 0부터 시작)</summary>
	public static List<uint> MarkEnchant_TryCosts = new List<uint>(30);

	/// <summary>문양 초기화 시 필요 아이템 갯수</summary>
	public static uint MarkStep_ResetCount { get; private set; }
    /// <summary>문양 보호제 아이템 아이디</summary>
    public static uint Mark_Protect_Item { get; private set; }

    /// <summary>레벨 성장 시 획득하는 스텟포인트 아이템 아이디</summary>
    public static uint GetStatPoint_ItemID { get; private set; }

    /// <summary>스텟 초기화 전용 아이템 아이디</summary>
    public static uint StatUp_ResetItem_ID { get; private set; }
    /// <summary>스텟 초기화 시 소모되는 다이아 아이디</summary>
    public static uint StatPoint_ResetID { get; private set; }
    /// <summary>스텟 초기화 시 다이아 소모 량</summary>
    public static uint StatPoint_ResetCount { get; private set; }

    /// <summary>반지 동일 아이템 장착 가능 수</summary>
    public static uint SameRing_EquipCount { get; private set; }


    /// <summary>파티 최대 인원 수</summary>
    public static uint Party_Max_Character { get; private set; }

    /// <summary>인벤토리 1회 확장시 늘어나는 슬롯수</summary>
    public static uint Expend_Inventory_Each { get; private set; }
    /// <summary>인벤토리 1회 확장시 다이아  금액</summary>
    public static uint Expend_Inventory_Diamond { get; private set; }
    /// <summary>인벤토리 1획 확장 전용 아이템 아이디</summary>
    public static uint Inven_Slot_Plus_ID { get; private set; }
    /// <summary>인벤토리 최고 개수</summary>
    public static uint Max_Inventory { get; private set; }

    /// <summary>창고 최고 개수</summary>
    public static uint Max_Storage_Count { get; private set; }
    /// <summary>창고 1회 확장시 늘어나는 슬롯수</summary>
    public static uint Expend_Storage_Each { get; private set; }
    /// <summary>창고 1회 확장시 다이아 금액</summary>
    public static uint Expend_Storage_Diamond { get; private set; }
    /// <summary>창고 1회 확장 전용 아이템 아이디</summary>
    public static uint Storage_Slot_Plus_ID { get; private set; }

    /// <summary>최대 반지 슬롯 수</summary>
    public static uint Max_Ring_Slot_Count { get; private set; }
    /// <summary>반지 확장 전용 아이템 아이디</summary>
    public static uint Ring_Slot_Open_ID { get; private set; }

    /// <summary>퀵슬롯 최고 개수</summary>
    public static uint Max_QuickSlot_Count { get; private set; }
    /// <summary>퀵슬롯 1회 확장시 확장개수</summary>
    public static uint QuickSlot_Open_Count { get; private set; }
    /// <summary>퀵슬롯 확장시 소요 다이아 개수</summary>
    public static uint QuickSlot_Open_ItemCount { get; private set; }
    /// <summary>퀵슬롯 확장 전용 아이템 아이디</summary>
    public static uint QuickSlot_Slot_Open_ID { get; private set; }

    /// <summary> 최대 캐릭터 슬롯 수(내부 구조는 8개 까지) </summary>
    public static uint Max_Character_Slot_Count { get; private set; }
    /// <summary> 캐릭터 확장 아이템 아이디 </summary>
    public static uint Character_Slot_Open_ID { get; private set; }

    /// <summary> 파티원 경험치 공유 거리 [1=반지름1m] </summary>
    public static uint Party_Exp_Range { get; private set; }

    /// <summary> 레벨 성장 시 획득하는 궁극기 스킬포인트 아이템 아이디 </summary>
    public static uint GetUASPoint_ItemID { get; private set; }

    /// <summary> 궁극기 초기화 시 다이아 소모 량 </summary>
    public static uint SpecialSkillPoint_ResetCount { get; private set; }
    /// <summary> 궁극기 초기화 전용 아이템 아이디 </summary>
    public static uint SpecialSkillUp_ResetItem_ID { get; private set; }

    /// <summary> 캐릭터당 궁극기 최대 슬롯수 </summary>
    public static uint Max_SpecialSkill_SlotCount { get; private set; }
    /// <summary> 캐릭터 궁극기 확장시 소모되는 전용 아이템 </summary>
    public static uint Expend_SpecialSkillSlot_ItemID { get; private set; }
    /// <summary> 캐릭터 궁극기 확장시 소모되는 아이템 수량 </summary>
    public static uint Expend_SpecialSkillSlot_ItemCount { get; private set; }

    /// <summary> 무기/방어구 자동강화 가능 단계 추가 </summary>
    public static uint EquipEnchant_AutoStep { get; private set; }

    /// <summary> 전용 채널 입장 유지 버프 아이디(AbilityAction_Table의 AbilityActionID) </summary>
    public static uint ChannelPrivate_BuffID { get; private set; }


    /// <summary> 길드 최대 인원 수 </summary>
    public static uint Guild_Max_Character { get; private set; }
    /// <summary> 길드창설 및 가입 가능 레벨 </summary>
    public static uint Guild_Application_Level { get; private set; }
    /// <summary> 길드창설 소모아이템 </summary>
    public static uint Guild_FoundingID { get; private set; }
    /// <summary> 길드창설 소모아이템 개수 </summary>
    public static uint Guild_FoundingCount { get; private set; }
    /// <summary> 길드 이름 최소 만족해야하는 글자 수 </summary>
    public static uint GuildName_Length_Min { get; private set; }
    /// <summary> 길드 이름 최대 설정 가능 글자 수 </summary>
    public static uint GuildName_Length_Max { get; private set; }
    /// <summary> 길드 공지 최대 글자수 </summary>
    public static uint GuildNotice_Length_Max { get; private set; }
    /// <summary> 길드 소개 최대 글자수 </summary>
    public static uint GuildIntroduce_Length_Max { get; private set; }
    /// <summary> 길드 남김말 최대 글자수 </summary>
    public static uint GuildDelivery_Length_Max { get; private set; }
    /// <summary> 길드마크 변경시 소모아이템 개수 </summary>
    public static uint GuildMark_Change_Count { get; private set; }
    /// <summary> 길드자금 아이템 아이디 </summary>
    public static uint Guild_Money_ID { get; private set; }
    /// <summary> 길드마크 변경후 다시 변경까지 쿨타임  [1=1초] </summary>
    public static uint GuildMark_Change_Time { get; private set; }

    /// <summary> 길드 골드 기부 금액 </summary>
    public static uint Guild_GoldGive_Count { get; private set; }
    /// <summary> 길드 골드 기부 시 길드 경험치 상승 량 </summary>
    public static uint Guild_GoldGive_Exp { get; private set; }
    /// <summary> 길드 골드 기부 보상으로  획득하는 길드자금양 </summary>
    public static uint Guild_GoldGive_GuildMoneyCount { get; private set; }
    /// <summary> 길드 골드 기부 보상 아이템 아이디 </summary>
    public static uint Guild_GoldGive_RewardID { get; private set; }
    /// <summary> 길드 골드 기부 보상 아이템 개수 </summary>
    public static uint Guild_GoldGive_RewardCount { get; private set; }
    /// <summary> 길드 다이아 작은 기부 금액 </summary>
    public static uint Guild_DiamondGiveSmall_Count { get; private set; }
    /// <summary> 길드 다이아 작은 기부 시 길드 경험치 상승 량 </summary>
    public static uint Guild_DiamondGiveSmall_Exp { get; private set; }
    /// <summary> 길드 다이아 작은 기부 보상으로  획득하는 길드자금양 </summary>
    public static uint Guild_DiamondGiveSmall_GuildMoneyCount { get; private set; }
    /// <summary> 길드 다이아 작은 기부 보상 아이템 아이디 </summary>
    public static uint Guild_DiamondGiveSmall_RewardID { get; private set; }
    /// <summary> 길드 다이아 작은 기부 보상 아이템 개수 </summary>
    public static uint Guild_DiamondGiveSmall_RewardCount { get; private set; }
    /// <summary> 길드 다이아 큰 기부 금액 </summary>
    public static uint Guild_DiamondGiveBig_Count { get; private set; }
    /// <summary> 길드 다이아 큰 기부 시 길드 경험치 상승 량 </summary>
    public static uint Guild_DiamondGiveBig_Exp { get; private set; }
    /// <summary> 길드 다이아 큰 기부 보상으로  획득하는 길드자금양 </summary>
    public static uint Guild_DiamondGiveBig_GuildMoneyCount { get; private set; }
    /// <summary> 길드 다이아 큰 기부 보상 아이템 아이디 </summary>
    public static uint Guild_DiamondGiveBig_RewardID { get; private set; }
    /// <summary> 길드 다이아 큰 기부 보상 아이템 개수 </summary>
    public static uint Guild_DiamondGiveBig_RewardCount { get; private set; }
    /// <summary>길드 매일 기부 최대 횟수</summary>
    public static uint Guild_Give_Limit { get; private set; }
    /// <summery>길드 배틀 포인트</summery>
    public static uint Guild_Battle_Point_Item { get; private set; }

    /// <summary>마을 포탈 아이디</summary>
    public static uint Town_Portal_ID { get; private set; }

	/// <summary사망 패널티 적용 레벨(이 수치 이하는 경험치 하락 및 사망디버프 획득하지 않음)</summary>
	public static uint Death_DebuffPenalty_Level { get; private set; }

	/// <summary>1단계 사망 디버프 AbilityAction_Table 아이디</summary>
	public static uint Death_DebuffPenalty_AbilityActionID1 { get; private set; }
    /// <summary>2단계 사망 디버프 AbilityAction_Table 아이디</summary>
    public static uint Death_DebuffPenalty_AbilityActionID2 { get; private set; }
    /// <summary>3단계 사망 디버프 AbilityAction_Table 아이디</summary>
    public static uint Death_DebuffPenalty_AbilityActionID3 { get; private set; }

    /// <summary>1단계 사망 디버프 제거 골드 소모량</summary>
    public static uint Death_DebuffPenalty_RestoreCount1 { get; private set; }
    /// <summary>2단계 사망 디버프 제거 골드 소모량</summary>
    public static uint Death_DebuffPenalty_RestoreCount2 { get; private set; }
    /// <summary>3단계 사망 디버프 제거 골드 소모량</summary>
    public static uint Death_DebuffPenalty_RestoreCount3 { get; private set; }


    /// <summary> 사망 복구 EXP 재화 </summary>
    public static uint Death_ExpPenalty_RestoreID { get; private set; }


    /// <summary> 사망 패널티 중첩 수에 따른 경험치 복구 비용 최대 골드 수치 </summary>
    public static uint DeathPenalty_Max_Gold { get; private set; }
    /// <summary> 경험치 사망 패널티 복구 실버 비용(복구 수에 따라 가격 배수 상승) </summary>
    public static uint DeathPenalty_Gold_Count { get; private set; }
    /// <summary> 경험치 사망 패널티 복구 다이아 비용 </summary>
    public static uint DeathPenalty_Diamond_Count { get; private set; }

    /// <summary> 사망 부활시 HP 량 </summary>
    public static float Death_HP_Rate { get; private set; }
	public static float Death_MP_Rate { get; private set; }

	/// <summary>기본 HP포션 아이디</summary>
	public static uint HPPotion_Normal_ItemID { get; private set; }
    /// <summary>고급 HP포션 아이디</summary>
    public static uint HPPotion_High_ID { get; private set; }
    /// <summary>MP포션 아이디</summary>
    public static uint MPPotion_Normal_ItemID { get; private set; }

    /// <summary>자동분해 펫컬렉션 아이디</summary>
    public static uint AutoBreak_PetCollectionID { get; private set; }

    /// <summary>자동구매 펫컬렉션 아이디</summary>
    public static uint PetCollection_Auto_HpPotion { get; private set; }

    /// <summary>신의축복 버프 아이디</summary>
    public static uint GodBless_AbilityActionID { get; private set; }
    /// <summary>제로의 축복  버프 아이디</summary>
    public static uint ZeroBless_AbilityActionID { get; private set; }
    /// <summary>신의 권능  버프 아이디</summary>
    public static uint GodPower_AbilityActionID { get; private set; }

    /// <summary> 차단목록 등록수 </summary>
    public static uint Block_Max_Character { get; private set; }

    /// <summary> PK후 조롱을 하기위한 비용 </summary>
    public static uint PKSneer_Gold_Cost { get; private set; }
    /// <summary> PK후 조롱이 가능한 시간 (=1초) </summary>
    public static uint PKSneer_Time { get; private set; }

    /// <summary> 거래소 아이템 판매 가격 최소 </summary>
    public static uint Exchange_SellPrice_Min { get; private set; }
    /// <summary> 거래소 등록 골드 수수료 % (등록 다이아 비용 비례 골드 수수료) [1=1%] </summary>
    public static uint Exchange_Selling_Commission { get; private set; }
    /// <summary> 거래소 판매 다이아 수수료 % (판매 다이아 비용 비례) [1=1%] </summary>
    public static uint Exchange_Sell_Commission { get; private set; }
    /// <summary> 거래소 아이템 등록 맥스 </summary>
    public static uint Exchange_SellRegister_Max { get; private set; }

    /// <summary> 다이아 개인거래 시 최소 판매 금액 </summary>
    public static uint Private_Transaction_Min_Diamond { get; private set; }
    /// <summary> 다이아 개인거래 비밀번호 자리 수 </summary>
    public static uint Transaction_Password_Count { get; private set; }
    /// <summary> 다이아 개인거래 등록 시 실버 수수료[1=1%] </summary>
    public static uint Register_Commission { get; private set; }
    /// <summary> 다이아 개인거래 판매 수수료[1=1%] </summary>
    public static uint Sell_Commission { get; private set; }
    /// <summary> 다이아 개인거래 판매 최대 리스트 </summary>
    public static uint Sell_List_Count { get; private set; }


    /// <summary> 거래 시스템 On/Off (1=On, 0=Off) </summary>
    public static bool WTrade_Open { get; private set; }

    /// <summary> 개인 거래 시스템 On/Off (1=On, 0=Off) </summary>
    public static bool Private_Transaction_Open { get; private set; }

    /// <summary> 길드 버프 이전버프 사라지는 시점이전 자동구입 시점 [=1초] </summary>
    public static uint GuildBuff_AutoBuy_Time { get; private set; }

    /// <summary> - 가방게이지 무게패널티 없을때 표시되는 색상 등록 </summary>
    public static Color WeightGuage_BasicColor { get; private set; }

    /// <summary> - 마을 이동 아이템(귀환석) 아이디 </summary>
    public static uint Town_Move_ItemID { get; private set; }

    /// <summary> - PK 시 포탈 사용 불가 시간 </summary>
    public static uint PK_Portal_Unuseable_Time { get; private set; }

    /// <summary> - 공지 출력 유지 시간 [1=1초] </summary>
    public static uint Notice_Time { get; private set; }
    /// <summary> - 글로벌 메시지 출력 유지시간 [1=1초] </summary>
    public static uint MessageGlobal_Time { get; private set; }
    /// <summary> - 시스템 메시지 출력 유지시간 [1=1초] </summary>
    public static uint MessageSystem_Time { get; private set; }

    /// <summary> - 유물 시도 최대 가능 수 </summary>
    public static uint Mark_MaxAttempt_Count { get; private set; }


    /// <summary> - 변신 및 뽑기 마일리지 정보 추가 </summary>
    public static uint ChangeGachaMileage_ID { get; private set; }
    public static uint PetGachaMileage_ID { get; private set; }

    /// <summary> - 정수 아이템 아이디 </summary>
    public static uint Essence_ID { get; private set; }


    /// <summary> - 신속포션아이템 </summary>
    public static uint SpeedUP_ItemID { get; private set; }
    /// <summary> - 신속어빌리티액션 </summary>
    public static uint SpeedUP_AbilityActionID { get; private set; }
    /// <summary> - 신의축복아이템 </summary>
    public static uint GodTear_ItemID { get; private set; }
    /// <summary> - 신의축복아이템(창고이동불가) </summary>
    public static uint GodTear_ItemID_02 { get; private set; }

    /// <summary> - 캐릭터 삭제 대기 시간 </summary>
    public static uint Char_Delete_Time { get; private set; }

    /// <summary> - 잦은 채팅에 대한 시간 체크   [1=1초] </summary>
    public static uint Chat_Ban_CheckTime { get; private set; }
    /// <summary> - 잦은 채팅에 대한 시간동안 채팅입력횟수 </summary>
    public static uint Chat_Ban_CheckCount { get; private set; }
    /// <summary> - 잦은 채팅에 대한 밴시간   [1=1초] </summary>
    public static uint Chat_Ban_Time { get; private set; }

    /// <summary> - 채팅 레벨 제한 </summary>
    public static uint Chat_All_Level { get; private set; }

    /// <summary> - 가방 정렬 제한 시간  [1=1초] </summary>
    public static uint InvenArray_Time { get; private set; }

    /// <summary> - 컬렉션 달성시 알림 연출 보여주는 시간 </summary>
    public static uint Collection_Complete_Time { get; private set; }

    /// <summary> - 메세지 알림 표기 시간 </summary>
    public static uint MessageSystem_TooltipTime { get; private set; }

    /// <summary> - 실버 제작 MAKE 아이디 (실버부족시 바로가기용) </summary>
    public static uint MakeSilver_MakeID { get; private set; }


    /// <summary> - 변신 카드변경 최대횟수 </summary>
    public static uint CardChange_Change_Count { get; private set; }
    /// <summary> - 펫 카드변경 최대횟수 </summary>
    public static uint CardChange_Pet_Count { get; private set; }
    /// <summary> - 카드 획득후 카드 교체 가능한 시간 </summary>
    public static uint CardChange_ChangeTime { get; private set; }
    /// <summary> - 카드변경 다이아 비용 추가 </summary>
    public static uint CardChange_Diamond { get; private set; }

    /// <summary> - 다시 뽑기 시 4티어 변신 리스트 </summary>
    public static uint Change_Gacha_4tier { get; private set; }
    /// <summary> - 다시 뽑기 시 5티어 변신 리스트 </summary>
    public static uint Change_Gacha_5tier { get; private set; }
    /// <summary> - 다시 뽑기 시 6티어 변신 리스트 </summary>
    public static uint Change_Gacha_6tier { get; private set; }
    /// <summary> - 다시 뽑기 시 7티어 변신 리스트 </summary>
    public static uint Change_Gacha_7tier { get; private set; }
    /// <summary> - 다시 뽑기 시 4티어 펫 리스트 </summary>
    public static uint Pet_Gacha_4tier { get; private set; }
    /// <summary> - 다시 뽑기 시 5티어 펫 리스트 </summary>
    public static uint Pet_Gacha_5tier { get; private set; }
    /// <summary> - 다시 뽑기 시 6티어 펫 리스트 </summary>
    public static uint Pet_Gacha_6tier { get; private set; }
    /// <summary> - 다시 뽑기 시 7티어 펫 리스트 </summary>
    public static uint Pet_Gacha_7tier { get; private set; }

    /// <summary> - 거래소 아이템 판매 가격 최대 </summary>
    public static uint Exchange_SellPrice_Max { get; private set; }

    /// <summary> - 채널 원활 최대 비율 </summary>
    public static uint Chenel_State_Good { get; private set; }
    /// <summary> - 채널 혼잡 최대 비율 </summary>
    public static uint Chenel_State_Busy { get; private set; }
    /// <summary> - 채널 포화 최대 비율 </summary>
    public static uint Chenel_State_Full { get; private set; }

    /// <summary> - 보스전 시작전 입장 가능 시간 [=1초] </summary>
    public static uint BossWar_Enter_Time { get; private set; }

    /// <summary> - 친구 최대 인원 수 </summary>
    public static uint Friend_Max_Character { get; private set; }
    /// <summary> - 친구 보낸 요청: </summary>
    public static uint Friend_Invite_Max { get; private set; }
    /// <summary> - 친구 받은 요청 </summary>
    public static uint Friend_Invited_Max { get; private set; }
    /// <summary> - 경계 최대 </summary>
    public static uint Alert_Max_Character { get; private set; }

    /// <summary> - 친구 목록에 시간 정보 표기 ( 0 = 시간정보 표기 / 1 = 로그인,로그오프만 표기) </summary>
    public static bool Friend_Time_Check { get; private set; }
    /// <summary> - 경계 목록에 시간 정보 표기 ( 0 = 시간정보 표기 / 1 = 로그인,로그오프만 표기) </summary>
    public static bool Alert_Time_Check { get; private set; }

    /// <summary> - 부길드장 최대 수 </summary>
    public static uint Guild_SubMaster_Count { get; private set; }

    /// <summary> - 스텟 맥스 값 </summary>
    public static uint Stat_Max_Count { get; private set; }

    /// <summary> - 서비스 이용약관 </summary>
    public static string WLogin_Clause_GameService { get; private set; }
    /// <summary> - 개인정보 수집/이용 </summary>
    public static string WLogin_Clause_UserInfo { get; private set; }

    /// <summary> - 사냥터 이동 불가 사망 패널티 단계 </summary>
    public static uint Death_Penalty_NotPortal { get; private set; }

	/// <summary> 인터보스전 보스 죽이고나서 모두 내보내기전까지 대기 시간 </summary>
	public static uint BossWar_End_AfterTime { get; private set; }
	/// <summary> 보스로부터 거리를 재서 이값을 초과하면 기여도에서 제외 </summary>
	public static float BossWar_Contribution_Distance { get; private set; }
	/// <summary> 보스를 마지막 때린 이후 이 시간을 초과하면 기여도에서 제외 </summary>
	public static uint BossWar_Contribution_Time { get; private set; }
	/// <summary> 보스전 기여도 랭킹 포함 가능한 최소 누적 데미지 </summary>
	public static float BossWar_RankStartDamage { get; private set; } = 10000;
	/// <summary> 보스전 PK시 대상이 가지고 있는 누적 데미지 빼았는 % (1=1%)</summary>
	public static float BossWar_PKRewardRate { get; private set; } = 20f;

	/// <summary> 명예 코인 제작 가능 레벨 </summary>
	public static uint Make_HonorCoin_View_Level { get; private set; }
    /// <summary> 명예코인 아이템 아이디 </summary>
    public static uint Honor_Coin_ID { get; private set; }

    /// <summary> 길드 던전 on/off </summary>
    public static uint WGuildDungeon_Open { get; private set; }

	/// <summary> 어그로 스킬 사용 유예 시간 </summary>
	public static float Aggro_Skill_Use_Time { get; private set; }
	/// <summary> 어그로 교체 횟수 </summary>
	public static uint Aggro_Skill_Use_Count { get; private set; }

    /// <summary> 설문 등장 레벨 </summary>
    public static uint Survery_Open_Level { get; private set; }
    /// <summary> 설문 작성시 보상 </summary>
    public static uint Survery_Reward_ItemID { get; private set; }
    /// <summary> 설문 작성시 보상 수 </summary>
    public static uint Survery_Reward_ItemCount { get; private set; }

    /// <summary> 카페 주소 </summary>
    public static string WLogin_Clause_Cafe { get; private set; }

    /// <summary> 자동구매 샵 아이디 일반 </summary>
    public static uint AutoBuy_HpPotion { get; private set; }
    /// <summary> 자동구매 샵 아이디 고급 </summary>
    public static uint AutoBuy_HpPotion_Large { get; private set; }
    /// <summary> 가방에 이수량보다 작으면 구입 </summary>
    public static uint AutoBuy_HpPotion_MinCount { get; private set; }
    /// <summary> 자동구매 구입시 개수 </summary>
    public static uint AutoBuy_HpPotion_BuyCount { get; private set; }

    /// <summary> 제작시 경험치 수량 </summary>
    public static uint Exp_HonorCoin_Trade { get; private set; }

    /// <summary> 컨텐츠 사용 유무 </summary>
    public static uint WPvP_Duel_Use { get; private set; }
    /// <summary> 컨텐츠 사용 레벨 </summary>
    public static uint WPvP_Duel_Level { get; private set; }

    /// <summary> 제로 조각 아이템 아이디 </summary>
    public static uint ZeroP_ItemID { get; private set; }

    /// <summary> 콜로세움 딜레이 타임 </summary>
    public static uint WPvP_Matching_Waiting { get; private set; }

    /// <summary> 최대 이펙트 스폰 갯수 </summary>
    public static uint Effect_Limit { get; private set; }


    /// <summary> 변신 합성 가능 최대 티어 </summary>
    public static uint Change_Compose_MaxTier { get; private set; }
    /// <summary> 펫 합성 가능 최대 티어 </summary>
    public static uint Pet_Compose_MaxTier { get; private set; }

    /// <summary> 룬 강화 진행시간[1=1초] </summary>
    public static uint Rune_Enchant_Time { get; private set; }

    /// <summary> 룬 인벤토리 최대 개수 </summary>
    public static uint Rune_Inventory_Max_Count { get; private set; }

    /// <summary> 마법 공격력 수치 표시 용 나누기 값 </summary>
    public static float MagicAttackViewValue { get; private set; }

    /// <summary> Dissolve 시간. 테이블 셋팅 필요.</summary>
    public static float DissolveFadeOutDuration { get; private set; }

    /// <summary> Dissolve 시간. 테이블 셋팅 필요.</summary>
    public static float DissolveFadeInDuration { get; private set; }
    
    /// <summary> 알림 창 생성 캐릭터 최소 레벨 </summary>
    public static uint Infor_Min_Level { get; private set; }
    /// <summary> 알림 창 생성 캐릭터 최대 레벨 </summary>
    public static uint Infor_Max_Level { get; private set; }    

    /// <summary>인던 보스 소환까지 시간(초)</summary>
    public static uint Instance_Dungeon_BossSummon_Time { get; private set; } = 300;
	/// <summary>인던 보상 획득가능 횟수(참여는 무한, 클리어 2회 제한)</summary>
	public static uint Instance_Dungeon_Reward_Cnt { get; private set; } = 2;

    /// <summary>무한의탑 클리어까지 제한 시간</summary>
    public static uint InfinityDungeon_PlayTime { get; private set; }

    /// <summary>무한의 탑 초기화 아이템</summary>
    public static uint Infinity_Dungeon_Reset_ItemID { get; private set; }
    /// <summary>무한의 탑 초기화 아이템의 개수</summary>
    public static uint Infinity_Dungeon_Reset_ItemCnt { get; private set; }

    /// <summary>미니게임 오픈 여부 (0=Off / 1=Open)</summary>
    public static bool MiniGame_Open { get; private set; }

    /// <summary> 튜토리얼 스킵시 해당 id의 퀘스트까지 스킵. (QuestType과 QuestSequence로 체크) </summary>
    public static uint Skip_Tutorial_Tid { get; private set; }

    /// <summary> 상급 물약 옵션 On/Off </summary>
    public static bool HPPotion_High_Option { get; private set; }


    /// <summary> 거래소 오픈 레벨 </summary>
    public static uint Exchange_OpenLv { get; private set; }
    /// <summary> 개인거래 오픈 레벨 </summary>
    public static uint Private_Transaction_OpenLv { get; private set; }

    /// <summary>해당 캐릭터 레벨 이하 시 전투지역에서 변신/펫 소환을 하지 않을 경우 알림 메시지 출력 </summary>
    public static uint Summon_Tip_Level { get; private set; }

    /// <summary>아이템 드랍량에 따른 드랍 이펙트 최대 범위 (반지름 6미터) / 드랍품목 적으면 몹이랑 가까운곳에 떨어짐 </summary>
    public static uint ItemDropEffect_Max_Radius { get; private set; }

    /// <summary> 아이템 드랍 최대 범위의 아이템 최대 드랍량 </summary>
    public static uint ItemDropEffect_Max_Standard_Cnt { get; private set; }

    /// <summary> 퀘스트 이벤트 최종 보상 펫! </summary>
    public static uint QuestEvent_FinalReward_PetID { get; private set; }

    /// <summary> 장비 컬렉션 온오프 (0 = off / 1 = on) </summary>
    public static bool EquipCollection_OnOff { get; private set; }

    /// <summary> 타워 맵(라데스탑, 절망의탑) 온오프 (0 = off / 1 = on) </summary>
    public static bool Tower_OnOff { get; private set; }

    /// <summary> 아르엘의 축복 남은 시간 경고 (1=1초) </summary>
    public static uint ArelAlert_Time_Check { get; private set; }

    /// <summary> 뽑기시 나올 수 있는 최대 티어 </summary>
    public static byte Max_Pet_Tier { get; private set; }

    /// <summary> 뽑기시 나올 수 있는 최대 티어 </summary>
    public static byte Max_Change_Tier { get; private set; }

    /// <summary> 뽑기시 나올 수 있는 최대 티어 </summary>
    public static byte Max_EquipItem_Tier { get; private set; }

    /// <summary> 시간의 방 On/Off </summary>
    public static bool Scenario_Dungeon_Menu { get; private set; }

	/// <summary> 레이드 그림리퍼 방패 쿨타임 </summary>
	public static float Raid_FlinsShield_CoolTime { get; private set; }
	/// <summary> 레이드 그림리퍼 방패 효과시간 </summary>
	public static float Raid_FlinsShield_Duration { get; private set; }


    /// <summary> 레이드 초대 대기 시간 </summary>
    public static float Raid_Invite_WaitTime { get; private set; }

    /// <summary> 레이드 메뉴 on/off </summary>
    public static bool Raid_Button_OnOff { get; private set; }

    /// <summary> 레이드 제목 제한 갯수 </summary>
    public static uint Raid_RoomTitle_Count { get; private set; }
	/// <summary> 레이드 재접속 최대 유예시간 </summary>
	public static uint Raid_Connect_CutTime { get; private set; }

    /// <summary> 1티어 젬 판매 시 획득 아이템 </summary>
    public static uint GemSell_ItemID_1T { get; private set; }
    /// <summary> 2티어 젬 판매 시 획득 아이템 </summary>
    public static uint GemSell_ItemID_2T { get; private set; }
    /// <summary> 3티어 젬 판매 시 획득 아이템 </summary>
    public static uint GemSell_ItemID_3T { get; private set; }
    /// <summary> 4티어 젬 판매 시 획득 아이템 </summary>
    public static uint GemSell_ItemID_4T { get; private set; }
    /// <summary> 5티어 젬 판매 시 획득 아이템 </summary>
    public static uint GemSell_ItemID_5T { get; private set; }
    /// <summary> 6티어 젬 판매 시 획득 아이템 </summary>
    public static uint GemSell_ItemID_6T { get; private set; }


    /// <summary> 숫자 만큼만 탭 표기 하도록</summary>
    public static uint Mark_Use_Tab { get; private set; }

    public void OnReadyData()
    {
  //      Projectile_Speed = GetInt32("Projectile_Speed", 20);
  //      TutorialStageTID = GetUInt32("Tutorial_Stage_ID", 10000);
		//TrainingStageTID = GetUInt32("Training_Stage_ID", 10002);
		//PetScroll_AbilityActionTID = GetUInt32("PetScroll_AbilityActionTID", 700000);
		//SearchTargetRange = GetFloat("SearchTargetRange", 15f);
		//Auto_Search_Range = GetFloat("Auto_Search_Range", 20f);
		//WanderRadius = GetFloat("WanderRadius", 7);
		//MobAI_NoAttack_Time = GetUInt32("MobAI_NoAttack_Time", 5);
		//Offender_Time = GetUInt32("Offender_Time", 10_000);
		//MaxAggroHoldingTime = GetUInt32("MaxAggroHoldingTime", 10_000);
		////PauseTimeByHit = GetUInt32("PauseTimeByHit", 100) * TimeHelper.Unit_MsToSec;
		//Stiff_Immune_Time = GetFloat("Stiff_Immune_Time", 0.5f);

		//MyPlayer_SyncAllDelay = GetFloat("MyPlayer_SyncAllDelay", 0f);

		//Town_Portal_ID = GetUInt32("Town_Portal_ID");

  //      Start_Inventory = GetInt32("Start_Inventory");
  //      Start_Storage_Count = GetInt32("Start_Storage_Count");

  //      NickName_Length_Min = GetInt32("NickName_Length_Min");
  //      NickName_Length_Max = GetInt32("NickName_Length_Max");

  //      Message_Title_Text_Max = GetInt32("Message_Title_Text_Max");
  //      Message_Content_Text_Max = GetInt32("Message_Content_Text_Max");
  //      Message_Use_Level = GetUInt32("Message_Use_Level");
  //      Message_Gold_Cost = GetUInt32("Message_Gold_Cost");
  //      Guild_Message_Gold_Cost = GetUInt32("Guild_Message_Gold_Cost");
  //      Day_Message_Send_Limit = GetUInt32("Day_Message_Send_Limit");
  //      Day_GuildMessage_Send_Limit = GetUInt32("Day_GuildMessage_Send_Limit");

  //      Event_Reset_Time = GetUInt32("Event_Reset_Time");
  //      Gold_ID = GetUInt32("Gold_ID");
  //      Runelift_Use_Item = GetUInt32("Runelift_Use_Item");
  //      Exp_Item = GetUInt32("Exp_Item");
  //      Diamond_ID = GetUInt32("Diamond_ID");
  //      Exchange_Goods = GetUInt32("Exchange_Goods");
  //      Exchange_Commission_Goods = GetUInt32("Exchange_Commission_Goods");
  //      Private_Transaction_Trade_Goods = GetUInt32("Private_Transaction_Trade_Goods");
  //      Private_Transaction_Commission_Goods = GetUInt32("Private_Transaction_Commission_Goods");
  //      Change_Use_Item = GetUInt32("Change_Use_Item");
  //      Pet_Summon_Item = GetUInt32("Pet_Summon_Item");
  //      Ruby_ID = GetUInt32("Ruby_ID");
                
  //      Mileage_Change5T_ShopID = GetUInt32("Mileage_Change5T_ShopID");
  //      Mileage_Change6T_ShopID = GetUInt32("Mileage_Change6T_ShopID");
  //      Mileage_Change7T_ShopID = GetUInt32("Mileage_Change7T_ShopID");
                
  //      Mileage_Pet5T_ShopID = GetUInt32("Mileage_Pet5T_ShopID");
  //      Mileage_Pet6T_ShopID = GetUInt32("Mileage_Pet6T_ShopID");
  //      Mileage_Pet7T_ShopID = GetUInt32("Mileage_Pet7T_ShopID");
        
  //      Mark_Use_Level = GetUInt32("Mark_Use_Level");

  //      MarkEnchant_Use_Item = GetUInt32("MarkEnchant_Use_Item");

  //      MarkEnchant_TryCosts.Clear();
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try1_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try2_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try3_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try4_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try5_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try6_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try7_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try8_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try9_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try10_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try11_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try12_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try13_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try14_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try15_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try16_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try17_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try18_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try19_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try20_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try21_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try22_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try23_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try24_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try25_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try26_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try27_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try28_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try29_Cost"));
  //      MarkEnchant_TryCosts.Add(GetUInt32("MarkEnchant_Try30_Cost"));

  //      MarkStep_ResetCount = GetUInt32("MarkStep_ResetCount");
  //      Mark_Protect_Item = GetUInt32("Mark_Protect_Item");

  //      GetStatPoint_ItemID = GetUInt32("GetStatPoint_ItemID");

  //      StatUp_ResetItem_ID = GetUInt32("StatUp_ResetItem_ID");
  //      StatPoint_ResetID = GetUInt32("StatPoint_ResetID");
  //      StatPoint_ResetCount = GetUInt32("StatPoint_ResetCount");

  //      SameRing_EquipCount = GetUInt32("SameRing_EquipCount");

  //      Party_Max_Character = GetUInt32("Party_Max_Character");

  //      Expend_Inventory_Each = GetUInt32("Expend_Inventory_Each");
  //      Expend_Inventory_Diamond = GetUInt32("Expend_Inventory_Diamond");
  //      Inven_Slot_Plus_ID = GetUInt32("Inven_Slot_Plus_ID");
  //      Max_Inventory = GetUInt32("Max_Inventory");

  //      Max_Storage_Count = GetUInt32("Max_Storage_Count");
  //      Expend_Storage_Each = GetUInt32("Expend_Storage_Each");
  //      Expend_Storage_Diamond = GetUInt32("Expend_Storage_Diamond");
  //      Storage_Slot_Plus_ID = GetUInt32("Storage_Slot_Plus_ID");

  //      Max_Ring_Slot_Count = GetUInt32("Max_Ring_Slot_Count");
  //      Ring_Slot_Open_ID = GetUInt32("Ring_Slot_Open_ID");

  //      Max_QuickSlot_Count = GetUInt32("Max_QuickSlot_Count");
  //      QuickSlot_Open_Count = GetUInt32("QuickSlot_Open_Count");
  //      QuickSlot_Open_ItemCount = GetUInt32("QuickSlot_Open_ItemCount");
  //      QuickSlot_Slot_Open_ID = GetUInt32("QuickSlot_Slot_Open_ID");

  //      Max_Character_Slot_Count = GetUInt32("Max_Character_Slot_Count");
  //      Character_Slot_Open_ID = GetUInt32("Character_Slot_Open_ID");

  //      Party_Exp_Range = GetUInt32("Party_Exp_Range");

  //      SpecialSkillPoint_ResetCount = GetUInt32("SpecialSkillPoint_ResetCount");
  //      SpecialSkillUp_ResetItem_ID = GetUInt32("SpecialSkillUp_ResetItem_ID");

  //      Max_SpecialSkill_SlotCount = GetUInt32("Max_SpecialSkill_SlotCount");
  //      Expend_SpecialSkillSlot_ItemID = GetUInt32("Expend_SpecialSkillSlot_ItemID");
  //      Expend_SpecialSkillSlot_ItemCount = GetUInt32("Expend_SpecialSkillSlot_ItemCount");

  //      GetUASPoint_ItemID = GetUInt32("GetUASPoint_ItemID");

  //      EquipEnchant_AutoStep = GetUInt32("EquipEnchant_AutoStep");

  //      ChannelPrivate_BuffID = GetUInt32("ChannelPrivate_BuffID");

  //      Guild_Max_Character = GetUInt32("Guild_Max_Character");
  //      Guild_Application_Level = GetUInt32("Guild_Application_Level");
  //      Guild_FoundingID = GetUInt32("Guild_FoundingID");
  //      Guild_FoundingCount = GetUInt32("Guild_FoundingCount");

  //      GuildName_Length_Min = GetUInt32("GuildName_Length_Min");
  //      GuildName_Length_Max = GetUInt32("GuildName_Length_Max");
  //      GuildNotice_Length_Max = GetUInt32("GuildNotice_Length_Max");
  //      GuildIntroduce_Length_Max = GetUInt32("GuildIntroduce_Length_Max");
  //      GuildDelivery_Length_Max = GetUInt32("GuildDelivery_Length_Max");
  //      GuildMark_Change_Count = GetUInt32("GuildMark_Change_Count");
  //      Guild_Money_ID = GetUInt32("Guild_Money_ID");
  //      GuildMark_Change_Time = GetUInt32("GuildMark_Change_Time");

  //      Guild_GoldGive_Count = GetUInt32("Guild_GoldGive_Count");
  //      Guild_GoldGive_Exp = GetUInt32("Guild_GoldGive_Exp");
  //      Guild_GoldGive_GuildMoneyCount = GetUInt32("Guild_GoldGive_GuildMoneyCount");
  //      Guild_GoldGive_RewardID = GetUInt32("Guild_GoldGive_RewardID");
  //      Guild_GoldGive_RewardCount = GetUInt32("Guild_GoldGive_RewardCount");
  //      Guild_DiamondGiveSmall_Count = GetUInt32("Guild_DiamondGiveSmall_Count");
  //      Guild_DiamondGiveSmall_Exp = GetUInt32("Guild_DiamondGiveSmall_Exp");
  //      Guild_DiamondGiveSmall_GuildMoneyCount = GetUInt32("Guild_DiamondGiveSmall_GuildMoneyCount");
  //      Guild_DiamondGiveSmall_RewardID = GetUInt32("Guild_DiamondGiveSmall_RewardID");
  //      Guild_DiamondGiveSmall_RewardCount = GetUInt32("Guild_DiamondGiveSmall_RewardCount");
  //      Guild_DiamondGiveBig_Count = GetUInt32("Guild_DiamondGiveBig_Count");
  //      Guild_DiamondGiveBig_Exp = GetUInt32("Guild_DiamondGiveBig_Exp");
  //      Guild_DiamondGiveBig_GuildMoneyCount = GetUInt32("Guild_DiamondGiveBig_GuildMoneyCount");
  //      Guild_DiamondGiveBig_RewardID = GetUInt32("Guild_DiamondGiveBig_RewardID");
  //      Guild_DiamondGiveBig_RewardCount = GetUInt32("Guild_DiamondGiveBig_RewardCount");
  //      Guild_Give_Limit = GetUInt32("Guild_Give_Limit");
  //      Guild_Battle_Point_Item = GetUInt32("GuildBattlePoint");

  //      Death_DebuffPenalty_Level = GetUInt32("Death_DebuffPenalty_Level");

  //      Death_DebuffPenalty_AbilityActionID1 = GetUInt32("Death_DebuffPenalty_AbilityActionID1");
  //      Death_DebuffPenalty_AbilityActionID2 = GetUInt32("Death_DebuffPenalty_AbilityActionID2");
  //      Death_DebuffPenalty_AbilityActionID3 = GetUInt32("Death_DebuffPenalty_AbilityActionID3");

  //      Death_DebuffPenalty_RestoreCount1 = GetUInt32("Death_DebuffPenalty_RestoreCount1");
  //      Death_DebuffPenalty_RestoreCount2 = GetUInt32("Death_DebuffPenalty_RestoreCount2");
  //      Death_DebuffPenalty_RestoreCount3 = GetUInt32("Death_DebuffPenalty_RestoreCount3");

		//Death_HP_Rate = GetFloat("Death_HP_Rate", 20f);
		//Death_MP_Rate = GetFloat("Death_MP_Rate", 20f);

		//HPPotion_Normal_ItemID = GetUInt32("HPPotion_Normal_ItemID");
  //      HPPotion_High_ID = GetUInt32("HPPotion_High_ID");
  //      MPPotion_Normal_ItemID = GetUInt32("MPPotion_Normal_ItemID");

  //      AutoBreak_PetCollectionID = GetUInt32("AutoBreak_PetCollectionID");
  //      PetCollection_Auto_HpPotion = GetUInt32("PetCollection_Auto_HpPotion");        

  //      GodBless_AbilityActionID = GetUInt32("GodBless_AbilityActionID");
  //      ZeroBless_AbilityActionID = GetUInt32("ZeroBless_AbilityActionID");
  //      GodPower_AbilityActionID = GetUInt32("GodPower_AbilityActionID");

  //      Block_Max_Character = GetUInt32("Block_Max_Character");

  //      PKSneer_Gold_Cost = GetUInt32("PKSneer_Gold_Cost");
  //      PKSneer_Time = GetUInt32("PKSneer_Time");

  //      Exchange_SellPrice_Min = GetUInt32("Exchange_SellPrice_Min");
  //      Exchange_Selling_Commission = GetUInt32("Exchange_Selling_Commission");
  //      Exchange_Sell_Commission = GetUInt32("Exchange_Sell_Commission");
  //      Exchange_SellRegister_Max = GetUInt32("Exchange_SellRegister_Max");

  //      GuildBuff_AutoBuy_Time = GetUInt32("GuildBuff_AutoBuy_Time");

  //      if (ColorUtility.TryParseHtmlString(Get("WeightGuage_BasicColor"), out Color parseColor))
  //          WeightGuage_BasicColor = parseColor;

  //      Town_Move_ItemID = GetUInt32("Town_Move_ItemID");

  //      PK_Portal_Unuseable_Time = GetUInt32("PK_Portal_Unuseable_Time");

  //      Notice_Time = GetUInt32("Notice_Time");
  //      MessageGlobal_Time = GetUInt32("MessageGlobal_Time");
  //      MessageSystem_Time = GetUInt32("MessageSystem_Time");

  //      Mark_MaxAttempt_Count = GetUInt32("Mark_MaxAttempt_Count");

  //      ChangeGachaMileage_ID = GetUInt32("ChangeGachaMileage_ID");
  //      PetGachaMileage_ID = GetUInt32("PetGachaMileage_ID");

  //      Essence_ID = GetUInt32("Essence_ID");

  //      SpeedUP_ItemID = GetUInt32("SpeedUP_ItemID");
  //      SpeedUP_AbilityActionID = GetUInt32("SpeedUP_AbilityActionID");
  //      GodTear_ItemID = GetUInt32("GodTear_ItemID");
  //      GodTear_ItemID_02 = GetUInt32("GodTear_ItemID_02");

  //      Char_Delete_Time = GetUInt32("Char_Delete_Time");

  //      Chat_Ban_CheckTime = GetUInt32("Chat_Ban_CheckTime");
  //      Chat_Ban_CheckCount = GetUInt32("Chat_Ban_CheckCount");
  //      Chat_Ban_Time = GetUInt32("Chat_Ban_Time");

  //      InvenArray_Time = GetUInt32("InvenArray_Time");

  //      Collection_Complete_Time = GetUInt32("Collection_Complete_Time");

  //      MessageSystem_TooltipTime = GetUInt32("MessageSystem_TooltipTime");

  //      MakeSilver_MakeID = GetUInt32("MakeSilver_MakeID");

  //      CardChange_Change_Count = GetUInt32("CardChange_Change_Count");
  //      CardChange_Pet_Count = GetUInt32("CardChange_Pet_Count");
  //      CardChange_ChangeTime = GetUInt32("CardChange_ChangeTime");
  //      CardChange_Diamond = GetUInt32("CardChange_Diamond");

  //      Change_Gacha_4tier = GetUInt32("Change_Gacha_4tier");
  //      Change_Gacha_5tier = GetUInt32("Change_Gacha_5tier");
  //      Change_Gacha_6tier = GetUInt32("Change_Gacha_6tier");
  //      Change_Gacha_7tier = GetUInt32("Change_Gacha_7tier");
  //      Pet_Gacha_4tier = GetUInt32("Pet_Gacha_4tier");
  //      Pet_Gacha_5tier = GetUInt32("Pet_Gacha_5tier");
  //      Pet_Gacha_6tier = GetUInt32("Pet_Gacha_6tier");
  //      Pet_Gacha_7tier = GetUInt32("Pet_Gacha_7tier");

  //      Exchange_SellPrice_Max = GetUInt32("Exchange_SellPrice_Max");

  //      Chenel_State_Good = GetUInt32("Chenel_State_Good");
  //      Chenel_State_Busy = GetUInt32("Chenel_State_Busy");
  //      Chenel_State_Full = GetUInt32("Chenel_State_Full");

  //      BossWar_Enter_Time = GetUInt32("BossWar_Enter_Time");

  //      Friend_Max_Character = GetUInt32("Friend_Max_Character");
  //      Friend_Invite_Max = GetUInt32("Friend_Invite_Max");
  //      Friend_Invited_Max = GetUInt32("Friend_Invited_Max");
  //      Alert_Max_Character = GetUInt32("Alert_Max_Character");

  //      Friend_Time_Check = GetUInt32("Friend_Time_Check") == 1;
  //      Alert_Time_Check = GetUInt32("Alert_Time_Check") == 1;

  //      Guild_SubMaster_Count = GetUInt32("Guild_SubMaster_Count");

  //      Stat_Max_Count = GetUInt32("Stat_Max_Count");

  //      WLogin_Clause_GameService = Get("WLogin_Clause_GameService");
  //      WLogin_Clause_UserInfo = Get("WLogin_Clause_UserInfo");

  //      Death_Penalty_NotPortal = GetUInt32("Death_Penalty_NotPortal");

		//BossWar_End_AfterTime = GetUInt32("BossWar_End_AfterTime", 15);
		//BossWar_Contribution_Distance = GetFloat("BossWar_Contribution_Distance", 30);
		//BossWar_Contribution_Time = GetUInt32("BossWar_Contribution_Time", 10);
		//BossWar_RankStartDamage = GetFloat("BossWar_RankStartDamage", 10000f);
		//BossWar_PKRewardRate = GetUInt32("BossWar_PKRewarRate", 20);

		//Make_HonorCoin_View_Level = GetUInt32("Make_HonorCoin_View_Level");

  //      Honor_Coin_ID = GetUInt32("Honor_Coin_ID");

  //      WGuildDungeon_Open = GetUInt32("WGuildDungeon_Open");

		//Aggro_Skill_Use_Time = GetFloat("Aggro_Skill_Use_Time", 10);
		//Aggro_Skill_Use_Count = GetUInt32("Aggro_Skill_Use_Count", 3);

  //      Survery_Open_Level = GetUInt32("Survery_Open_Level");
  //      Survery_Reward_ItemID = GetUInt32("Survery_Reward_ItemID");
  //      Survery_Reward_ItemCount = GetUInt32("Survery_Reward_ItemCount");

  //      WLogin_Clause_Cafe = Get("WLogin_Clause_Cafe");

  //      Chat_All_Level = GetUInt32("Chat_All_Level");

  //      AutoBuy_HpPotion = GetUInt32("AutoBuy_HpPotion");
  //      AutoBuy_HpPotion_Large = GetUInt32("AutoBuy_HpPotion_Large");
  //      AutoBuy_HpPotion_MinCount = GetUInt32("AutoBuy_HpPotion_MinCount");
  //      AutoBuy_HpPotion_BuyCount = GetUInt32("AutoBuy_HpPotion_BuyCount");

  //      Death_ExpPenalty_RestoreID = GetUInt32("Death_ExpPenalty_RestoreID");

  //      DeathPenalty_Max_Gold = GetUInt32("DeathPenalty_Max_Gold");
  //      DeathPenalty_Gold_Count = GetUInt32("DeathPenalty_Gold_Count");
  //      DeathPenalty_Diamond_Count = GetUInt32("DeathPenalty_Diamond_Count");

  //      Exp_HonorCoin_Trade = GetUInt32("Exp_HonorCoin_Trade");

  //      ZeroP_ItemID = GetUInt32("ZeroP_ItemID");

  //      WPvP_Duel_Use = GetUInt32("WPvP_Duel_Use");
  //      WPvP_Duel_Level = GetUInt32("WPvP_Duel_Level");

  //      WPvP_Matching_Waiting = GetUInt32("WPvP_Matching_Waiting");

  //      Effect_Limit = GetUInt32("Effect_Limit");

  //      Change_Compose_MaxTier = GetUInt32("Change_Compose_MaxTier");
  //      Pet_Compose_MaxTier = GetUInt32("Pet_Compose_MaxTier");

  //      Private_Transaction_Min_Diamond = GetUInt32("Private_Transaction_Min_Diamond");
  //      Transaction_Password_Count = GetUInt32("Transaction_Password_Count");
  //      Register_Commission = GetUInt32("Register_Commission");
  //      Sell_Commission = GetUInt32("Sell_Commission");
  //      Sell_List_Count = GetUInt32("Sell_List_Count");

  //      Private_Transaction_Open = Get("Private_Transaction_Open") == "1";
  //      WTrade_Open = Get("WTrade_Open") == "1";

  //      Rune_Enchant_Time = GetUInt32("Rune_Enchant_Time");
  //      Rune_Inventory_Max_Count = GetUInt32("Rune_Inventory_Max_Count");

  //      MagicAttackViewValue = GetFloat("MagicAttackViewValue");

  //      DissolveFadeOutDuration = GetFloat("DissolveFadeOutDuration", 3f);
  //      DissolveFadeInDuration = GetFloat("DissolveFadeInDuration", 0f);

  //      Infor_Min_Level = GetUInt32("Infor_Min_Level", 5);
  //      Infor_Max_Level = GetUInt32("Infor_Max_Level", 40);

  //      Instance_Dungeon_BossSummon_Time = GetUInt32("Instance_Dungeon_BossSummon_Time", 300);
		//Instance_Dungeon_Reward_Cnt = GetUInt32("Instance_Dungeon_Reward_Cnt", 2);

  //      MiniGame_Open = GetUInt32("MiniGame_Open") == 1;

  //      Skip_Tutorial_Tid = GetUInt32("Skip_Tutorial_Tid", 1028);

  //      HPPotion_High_Option = GetUInt32("HPPotion_High_Option") == 1;

  //      Exchange_OpenLv = GetUInt32("Exchange_OpenLv");
  //      Private_Transaction_OpenLv = GetUInt32("Private_Transaction_OpenLv");

  //      Summon_Tip_Level = GetUInt32("Summon_Tip_Level", 30);

  //      ItemDropEffect_Max_Radius = GetUInt32("ItemDropEffect_Max_Radius", 6);
  //      ItemDropEffect_Max_Standard_Cnt = GetUInt32("ItemDropEffect_Max_Standard_Cnt", 20);

  //      QuestEvent_FinalReward_PetID = GetUInt32("QuestEvent_FinalReward_PetID");

  //      EquipCollection_OnOff = GetUInt32("EquipCollection_OnOff") == 1;

  //      Tower_OnOff = GetUInt32("Tower_OnOff") == 1;

  //      ArelAlert_Time_Check = GetUInt32("ArelAlert_Time_Check");


  //      Max_Pet_Tier = (byte)GetUInt32("Max_Pet_Tier", 6);
  //      Max_Change_Tier = (byte)GetUInt32("Max_Change_Tier", 6);
  //      Max_EquipItem_Tier = (byte)GetUInt32("Max_Change_Tier");

  //      InfinityDungeon_PlayTime = GetUInt32("InfinityDungeon_PlayTime");
  //      Infinity_Dungeon_Reset_ItemID = GetUInt32("Infinity_Dungeon_Reset_ItemID");
  //      Infinity_Dungeon_Reset_ItemCnt = GetUInt32("Infinity_Dungeon_Reset_ItemCnt");

  //      Scenario_Dungeon_Menu = GetUInt32("Scenario_Dungeon_Menu") == 1;

		//Raid_FlinsShield_CoolTime = GetFloat("Raid_FlinsShield_CoolTime", 10f);
		//Raid_FlinsShield_Duration = GetFloat("Raid_FlinsShield_Duration", 2f);


  //      Raid_Invite_WaitTime = GetFloat("Raid_Invite_WaitTime", 10f);

  //      Raid_Button_OnOff = GetUInt32("Raid_Button_OnOff", 1) == 1;

  //      Raid_RoomTitle_Count = GetUInt32("Raid_RoomTitle_Count");
		//Raid_Connect_CutTime = GetUInt32("Raid_Connect_CutTime");

  //      GemSell_ItemID_1T = GetUInt32("GemSell_ItemID_1T");
  //      GemSell_ItemID_2T = GetUInt32("GemSell_ItemID_2T");
  //      GemSell_ItemID_3T = GetUInt32("GemSell_ItemID_3T");
  //      GemSell_ItemID_4T = GetUInt32("GemSell_ItemID_4T");
  //      GemSell_ItemID_5T = GetUInt32("GemSell_ItemID_5T");
  //      GemSell_ItemID_6T = GetUInt32("GemSell_ItemID_6T");

  //      Mark_Use_Tab = GetUInt32("Mark_Use_Tab");
    }

 //   /// <summary>리플렉션은 쓰기 싫고...</summary>
 //   /// <param name="_defaultValue">테이블 데이터가 없거나 유효하지 않는 값이라면 해당값 사용</param>
 //   static int GetInt32(string _key, int _defaultValue = 0)
	//{
	//	if (GameDBManager.Container.Config_Table_data.TryGetValue(_key, out var table))
	//	{
	//		int foundValue;
	//		if (int.TryParse(table.Value, out foundValue))
	//		{
	//			return foundValue;
	//		}
	//		else
	//		{
	//			Debug.LogError($"{nameof(Config_Table)}[{_key}]의 값이 Int32로 변환불가능한 값입니다. 필히 체크바람!");
	//			return _defaultValue;
	//		}
	//	}
	//	else
	//	{
	//		Debug.LogError($"{nameof(Config_Table)}[{_key}] 이 존재하지 않습니다. 필히 체크바람!");
	//		return _defaultValue;
	//	}
	//}

	//static uint GetUInt32(string _key, uint _defaultValue = 0)
	//{
	//	if (GameDBManager.Container.Config_Table_data.TryGetValue(_key, out var table))
	//	{
	//		uint foundValue;
	//		if (uint.TryParse(table.Value, out foundValue))
	//		{
	//			return foundValue;
	//		}
	//		else
	//		{
	//			Debug.LogError($"{nameof(Config_Table)}[{_key}]의 값이 UInt32로 변환불가능한 값입니다. 필히 체크바람!");
	//			return _defaultValue;
	//		}
	//	}
	//	else
	//	{
	//		Debug.LogError($"{nameof(Config_Table)}[{_key}] 이 존재하지 않습니다. 필히 체크바람!");
	//		return _defaultValue;
	//	}
	//}

	//static float GetFloat(string _key, float _defaultValue = 0)
	//{
	//	if (GameDBManager.Container.Config_Table_data.TryGetValue(_key, out var table))
	//	{
	//		float foundValue;
	//		if (float.TryParse(table.Value, out foundValue))
	//		{
	//			return foundValue;
	//		}
	//		else
	//		{
	//			Debug.LogError($"{nameof(Config_Table)}[{_key}]의 값이 Float로 변환불가능한 값입니다. 필히 체크바람!");
	//			return _defaultValue;
	//		}
	//	}
	//	else
	//	{
	//		Debug.LogError($"{nameof(Config_Table)}[{_key}] 이 존재하지 않습니다. 필히 체크바람!");
	//		return _defaultValue;
	//	}
 //   }

 //   static string Get(string _key, string _defaultValue = "")
 //   {
 //       if (GameDBManager.Container.Config_Table_data.TryGetValue(_key, out var table))
 //       {
 //           return table.Value;
 //       }
 //       else
 //       {
 //           Debug.LogError($"{nameof(Config_Table)}[{_key}] 이 존재하지 않습니다. 필히 체크바람!");
 //           return _defaultValue;
 //       }
 //   }
}
