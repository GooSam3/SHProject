//***Auto Generation Code****

namespace GameDB
{
	public enum E_MonsterType
	{
		Normal = 0, /*일반 몬스터*/
		FieldBoss = 1, /*필드 보스 몬스터*/
		WorldBoss = 2, /*월드 보스 몬스터*/
		SeverBoss = 3, /*인터서버 보스 몬스터*/
		Lucky = 4, /*럭키 몬스터*/
		InstanceBoss = 5, /*인스턴스 던전 보스*/
		RaidBoss = 6, /*레이드 던전 보스*/
		Flag = 7, /*지역점령 깃발*/
	}

	public enum E_SkillArouseType
	{
		None = 0, /*없음*/
		AttackState = 1, /*공격상태*/
		ReSpawn = 2, /*리스폰 시 발동*/
	}

	public enum E_CollisionType
	{
		None = 0, /*사용않함*/
		NotCollision = 1, /*충돌 미적용*/
		Collision = 2, /*충돌 적용*/
	}

	public enum E_HitPossibleType
	{
		None = 0, /*사용않함*/
		Hit = 1, /*피격 가능*/
		NotHit = 2, /*피격 불가능(공격 타겟 제외)*/
	}

	public enum E_RotationType
	{
		None = 0, /*사용않함*/
		NotRotation = 1, /*몬스터 회전 불가능*/
		Rotation = 2, /*몬스터 회전 가능*/
	}

	public enum E_MoveType
	{
		None = 0, /*사용않함*/
		Stand = 1, /*2발 이동*/
		Prostrate = 2, /*4발 이상 이동(경사면 기울기와 동일하게 기울어짐)*/
		Fly = 3, /*공중 형*/
		NotMove = 4, /*이동 불가능*/
	}

	public enum E_BattleType
	{
		Friendly = 0, /*평화 몬스터, 공격을 받아도 대응하지 않음*/
		Neutral = 1, /*비선공 몬스터, 공격을 받으면 대응*/
		Hostile = 2, /*선공 몬스터*/
	}

	public enum E_SpawnType
	{
		None = 0, /*사망 후 리스폰 되지 않음*/
		Die = 1, /*사망 일정시간 후 리스폰*/
		Count = 2, /*지역 몬스터 일정 개수 사망 후*/
		Time = 3, /*고정된 시간*/
	}

	public enum E_ReturnType
	{
		Return = 0, /*리턴 범위를 벗어나는 순간 전투가 종료, 전투를 시작 위치로 되돌아감*/
		Tracking = 1, /*타겟을 찾지못할 때까지 쫓아감*/
	}

	public enum E_NPCType
	{
		Normal = 0, /*일반 NPC*/
		JobNPC = 1, /*특정 컨텐츠 기능을 가지고 있는 NPC*/
		Quest = 2, /*퀘스트*/
		Prop = 3, /*프랍*/
	}

	public enum E_JobType
	{
		None = 0, /*직업 없음*/
		Store = 1, /*상점 관리인*/
		Storage = 2, /*창고 관리인*/
		Trade = 3, /*거래소 관리인*/
		Cleric = 4, /*성직자 (사망 패널티 관리인)*/
		InterPortal = 5, /*인터서버 포탈*/
		Quest = 6, /*퀘스트 반응용*/
		SkillStore = 7, /*스킬북 관리인*/
		Raid_Interaction = 8, /*레이드 인터렉션*/
	}

	public enum E_TargetUseType
	{
		None = 0, /*사용 않함*/
		Not = 1, /*선택 불가능*/
		Target = 2, /*선택 가능*/
	}

	public enum E_NPCSpawnType
	{
		Always = 0, /*항상 노출*/
		Quest = 1, /*퀘스트 획득 시 노출*/
	}

	public enum E_ToolTipType
	{
		None = 0, /*말풍선 없음*/
		Target = 1, /*NPC 터치 시, 타겟을 잡았을때*/
		Search = 2, /*인식 범위에 캐릭터 위치 시*/
	}

	public enum E_PropType
	{
		None = 0, /*선택 불가능*/
		Target = 1, /*선택 가능*/
	}

	public enum E_InteractionType
	{
		None = 0, /*직업 없음*/
		Raid = 1, /*레이드 인터렉션*/
		Pickup = 2, /*줍기 인터렉션*/
	}

	public enum E_LevelUpType
	{
		Up = 0, /*다음 레벨로 상승 가능*/
		End = 1, /*다음 레벨로 상승 불가능*/
	}

	public enum E_EffectType
	{
		Once = 0, /*1회 출력 후 사라짐*/
		Loop = 1, /*설정된 시간만큼 반복*/
	}

	public enum E_PlayType
	{
		Point = 0, /*발생한 위치에서 이동 없음*/
		Move = 1, /*주체의 노드에 따라 같이 이동*/
		MoveRot = 2, /*주체의 노드에 따라 같이 이동 및 회전*/
	}

	public enum E_ModelSocket
	{
		Root = 0, /*캐릭터 발밑 중앙*/
		Hit = 1, /*피격 FX 출력 및 발사된 Projectile의 목표 등의 목적으로 활용될 예정*/
		Head = 2, /*캐릭터/몬스터의 머리로부터 일정 거리 올라간 곳에 인게임 내 UI(Hp바, 이름)가 출력될 위치*/
		Weapon_L = 3, /*무기에 붙는 이펙트*/
		Weapon_R = 4, /*무기에 붙는 이펙트*/
		Projectile = 5, /*발사체 나가는 위치*/
		Riding = 6, /*탑승시 붙는 위치*/
		Wing = 7, /*날개 붙는 위치*/
	}

	public enum E_EffectOffsetType
	{
		None = 0, /**/
		Attacker = 1, /*공격자의 공격 위치(스킬 관련 이펙트일 경우)*/
	}

	[System.Flags]
	public enum E_ConditionControl
	{
		None = 0, /*없음*/
		NotMove = 1, /*이동 불가*/
		NotAttack = 2, /*일반공격 불가*/
		NotPotion = 4, /*물약 사용 불가*/
		NotReturn = 8, /*귀환 불가*/
		NotSkill = 16, /*스킬 사용 불가*/
		Vision = 32, /*시야 감소*/
		SpecialVision = 64, /*스페셜 시야 감소*/
		NotRide = 128, /*탈것 탑승 불가*/
	}

	[System.Flags]
	public enum E_ImmuneControl
	{
		None = 0, /*없음*/
		Im_NotMove = 1, /*이동금지 면역*/
		Im_NotSkill = 2, /*침묵 면역*/
		Im_Fear = 4, /*공포 면역*/
		Im_Stun = 8, /*스턴 면역*/
		Im_Knockback = 16, /*넉백 면역*/
		Im_Pull = 32, /*당기기 면역*/
		Im_Spasticity = 64, /*경직 면역*/
	}

	public enum E_LocaleType
	{
		None = 0, /*로케일의 텍스트만 출력*/
		AbilityPoint = 1, /*적용 수치 표시*/
		SupportTime = 2, /*적용 시간 표시*/
		Empty = 3, /*출력 없음*/
	}

	public enum E_MarkType
	{
		None = 0, /*표시 없음*/
		Per = 1, /*% 표시*/
	}

	public enum E_PlusOutput
	{
		None = 0, /*표시 없음*/
		Output = 1, /*수치 앞 + 표시*/
	}

	public enum E_AbilityType
	{
		FINAL_STR = 1, /*최종 힘*/
		FINAL_DEX = 2, /*최종 민첩*/
		FINAL_INT = 3, /*최종 지능*/
		FINAL_WIS = 4, /*최종 지혜*/
		FINAL_VIT = 5, /*최종 체력*/
		FINAL_MAX_HP = 6, /*최종 최대 생명력*/
		FINAL_MAX_MP = 7, /*최종 최대 마나*/
		FINAL_MIN_SHORT_ATTACK = 8, /*최종 최소 근거리 공격력*/
		FINAL_MAX_SHORT_ATTACK = 9, /*최종 최대 근거리 공격력*/
		FINAL_MIN_LONG_ATTACK = 10, /*최종 최소 원거리 데미지*/
		FINAL_MAX_LONG_ATTACK = 11, /*최종 최대 원거리 데미지*/
		FINAL_MIN_MAGIC_ATTACK = 12, /*최종 최소 마법 데미지*/
		FINAL_MAX_MAGIC_ATTACK = 13, /*최종 최대 마법 데미지*/
		FINAL_SHORT_ACCURACY = 14, /*최종 근거리 명중*/
		FINAL_LONG_ACCURACY = 15, /*최종 원거리 명중*/
		FINAL_MAGIC_ACCURACY = 16, /*최종 마법 명중*/
		FINAL_MELEE_DEFENCE = 17, /*최종 방어력*/
		FINAL_MAGIC_DEFENCE = 18, /*최종 마법 방어력*/
		FINAL_SHORT_CRITICAL = 19, /*최종 근거리 치명타 확률*/
		FINAL_LONG_CRITICAL = 20, /*최종 원거리 치명타 확률*/
		FINAL_MAGIC_CRITICAL = 21, /*최종 마법 치명타 확률*/
		FINAL_SHORT_CRITICAL_MINUS = 22, /*최종 받는 근거리 치명타 확률 감소*/
		FINAL_LONG_CRITICAL_MINUS = 23, /*최종 받는 원거리 치명타 확률 감소*/
		FINAL_MAGIC_CRITICAL_MINUS = 24, /*최종 받는 마법 치명타 확률 감소*/
		FINAL_SHORT_CRITICALDAMAGE = 25, /*최종 근거리 치명타 데미지*/
		FINAL_LONG_CRITICALDAMAGE = 26, /*최종 원거리 치명타 데미지*/
		FINAL_MAGIC_CRITICALDAMAGE = 27, /*최종 마법 치명타 데미지*/
		FINAL_SHORT_CRITICALDAMAGE_MINUS = 28, /*최종 받는 근거리 치명타 데미지 감소*/
		FINAL_LONG_CRITICALDAMAGE_MINUS = 29, /*최종 받는 원거리 치명타 데미지 감소*/
		FINAL_MAGIC_CRITICALDAMAGE_MINUS = 30, /*최종 받는 마법 치명타 데미지 감소*/
		FINAL_MOVE_SPEED = 31, /*최종 이동속도*/
		FINAL_ATTACK_SPEED = 32, /*최종 공격속도*/
		FINAL_SKILL_SPEED = 33, /*최종 시전속도*/
		FINAL_SHORT_REDUCTION = 34, /*최종 근거리 피해감소*/
		FINAL_LONG_REDUCTION = 35, /*최종 원거리 피해감소*/
		FINAL_MAGIC_REDUCTION = 36, /*최종 마법 피해감소*/
		FINAL_SHORT_REDUCTION_IGNORE = 37, /*최종 근거리 피해감소 무시*/
		FINAL_LONG_REDUCTION_IGNORE = 38, /*최종 원거리 피해감소 무시*/
		FINAL_MAGIC_REDUCTION_IGNORE = 39, /*최종 마법 피해감소 무시*/
		FINAL_SHORT_EVASION = 40, /*최종 근거리 회피*/
		FINAL_LONG_EVASION = 41, /*최종 원거리 회피*/
		FINAL_MAGIC_EVASION = 42, /*최종 마법 회피*/
		FINAL_SHORT_EVASION_IGNORE = 43, /*최종 근거리 회피 무시*/
		FINAL_LONG_EVASION_IGNORE = 44, /*최종 원거리 회피 무시*/
		FINAL_MAGIC_EVASION_IGNORE = 45, /*최종 마법 회피 무시*/
		FINAL_HP_RECOVERY = 46, /*최종 생명력 회복량*/
		FINAL_HP_RECOVERY_TIME = 47, /*최종 생명력 회복 시간*/
		FINAL_MP_RECOVERY = 48, /*최종 마나 회복량*/
		FINAL_MP_RECOVERY_TIME = 49, /*최종 마나 회복 시간*/
		FINAL_POTION_RECOVERY_PLUS = 50, /*최종 물약 회복량*/
		FINAL_POTION_RECOVERY_PER = 51, /*최종 물약 회복율*/
		FINAL_POTION_RECOVERY_TIME_PER = 52, /*최종 물약 회복시간*/
		FINAL_SKILL_MP = 53, /*최종 마나 소모량*/
		FINAL_STUN_RATE_DOWN_PER = 54, /*최종 스턴 저항*/
		FINAL_FEAR_RATE_DOWN_PER = 55, /*최종 공포 저항*/
		FINAL_MAZ_RATE_DOWN_PER = 56, /*최종 상태이상 저항*/
		FINAL_MAZ_RATE_UP_PER = 57, /*최종 상태이상 발생확률*/
		FINAL_MEZ_STATE = 58, /*상태이상 총 합한 값*/
		FINAL_MAX_WEIGH = 59, /*최종 최대 무게*/
		FINAL_MAX_LENGTH = 60, /*스텟 총 개수*/
		FINAL_CURR_HP = 61, /*현재 생명력*/
		FINAL_CURR_MP = 62, /*현재 마나*/
		FINAL_PET_MAX_HP = 63, /*최종 펫 생명력*/
		FINAL_PET_SHORT_ATTACK = 64, /*최종 펫 근거리 공격력*/
		FINAL_PET_LONG_ATTACK = 65, /*최종 펫 원거리 공격력*/
		FINAL_PET_MAGIC_ATTACK = 66, /*최종 펫 마법 공격력*/
		FINAL_PET_ACCURACY = 67, /*최종 펫 명중*/
		FINAL_PET_MELEE_DEFENCE = 68, /*최종 펫 방어력*/
		FINAL_PET_MAGIC_DEFENCE = 69, /*최종 펫 마법 방어력*/
		FINAL_PET_REDUCTION = 70, /*최종 펫 피해감소*/
		FINAL_PET_REDUCTION_IGNORE = 71, /*최종 펫 피해감소 무시*/
		FINAL_PET_EVASION = 72, /*최종 펫 회피*/
		LEVEL = 73, /*캐릭터 레벨*/
		FINAL_GOLD_DROP_AMT = 74, /*최종 골드 드랍 획득량*/
		FINAL_EXP_DROP_AMT = 75, /*최종 경험치 획득량*/
		FINAL_PET_MOVE_SPEED = 76, /*최종 펫 이동속도*/
		FINAL_PET_ATTACK_SPEED = 77, /*최종 펫 공격속도*/
		FINAL_PET_MP_RECOVERY = 78, /*최종 펫 마나 회복*/
		FINAL_PET_POTION_RECOVERY = 79, /*최종 펫 포션 회복량*/
		FINAL_PET_MAZ_RATE_DOWN = 80, /*최종 펫 상태이상 저항*/
		FINAL_PET_MAZ_RATE_UP = 81, /*최종 펫 상태이상 적중*/
		FINAL_SKILL_COOLTIME = 82, /*최종 스킬 쿨타임*/
		FINAL_ABIL_MAX = 100, /*어빌리티 구분*/
		BASE_STR = 101, /*캐릭터: Character_table의 Strength 필드 값
몬스터: 없음*/
		BASE_DEX = 102, /*캐릭터: Character_table의 Dexterity 필드 값
몬스터: 없음*/
		BASE_INT = 103, /*캐릭터: Character_table의 Intellect 필드 값
몬스터: 없음*/
		BASE_WIS = 104, /*캐릭터: Character_table의 Wisdom 필드 값
몬스터: 없음*/
		BASE_VIT = 105, /*캐릭터: Character_table의 Vitality 필드 값
몬스터: 없음*/
		BASE_MAX_HP = 106, /*캐릭터: [Character_table의 MaxHP 필드 값]+[Level_table의 캐릭터 레벨의 MaxHP 필드 값]
몬스터: Monster_table의 MaxHP 필드 값*/
		BASE_MAX_MP = 107, /*캐릭터: [Character_table의 MaxMP 필드 값]+[Level_table의 캐릭터 레벨의 MaxMP 필드 값]
몬스터: Monster_table의 MaxMP 필드 값*/
		BASE_SHORT_ATTACK = 108, /*캐릭터: [Character_table의 ShortAttack 필드 값]+[Level_table의 캐릭터 레벨의 ShortAttack 필드 값]
몬스터: Monster_table의 ShortAttack 필드 값*/
		BASE_LONG_ATTACK = 109, /*캐릭터: [Character_table의 LongAttack 필드 값]+[Level_table의 캐릭터 레벨의 LongAttack 필드 값]
몬스터: Monster_table의 LongAttack 필드 값*/
		BASE_MAGIC_ATTACK = 110, /*캐릭터: [Character_table의 MagicAttack 필드 값]+[Level_table의 캐릭터 레벨의 MagicAttack 필드 값]
몬스터: Monster_table의 MagicAttack 필드 값*/
		BASE_SHORT_ACCURACY = 111, /*캐릭터: [Character_table의 ShortAccuracy 필드 값]+[Level_table의 캐릭터 레벨의 ShortAccuracy 필드 값]
몬스터: Monster_table의 ShortAccuracy 필드 값*/
		BASE_LONG_ACCURACY = 112, /*캐릭터: [Character_table의 LongAccuracy 필드 값]+[Level_table의 캐릭터 레벨의 LongAccuracy 필드 값]
몬스터: Monster_table의 LongAccuracy 필드 값*/
		BASE_MAGIC_ACCURACY = 113, /*캐릭터: [Character_table의 MagicAccuracy 필드 값]+[Level_table의 캐릭터 레벨의 MagicAccuracy 필드 값]
몬스터: Monster_table의 MagicAccuracy 필드 값*/
		BASE_MELEE_DEFENCE = 114, /*캐릭터: [Character_table의 MeleeDefense 필드 값]+[Level_table의 캐릭터 레벨의 MeleeDefense 필드 값]
몬스터: Monster_table의 MeleeDefense 필드 값*/
		BASE_MAGIC_DEFENCE = 115, /*캐릭터: [Character_table의 MagicDefense 필드 값]+[Level_table의 캐릭터 레벨의 MagicDefense 필드 값]
몬스터: Monster_table의 MagicDefense 필드 값*/
		BASE_SHORT_CRITICAL = 116, /*캐릭터: [Character_table의 ShortCritical 필드 값]+[Level_table의 캐릭터 레벨의 ShortCritical 필드 값]
몬스터: Monster_table의 ShortCritical 필드 값*/
		BASE_LONG_CRITICAL = 117, /*캐릭터: [Character_table의 LongCritical 필드 값]+[Level_table의 캐릭터 레벨의 LongCritical 필드 값]
몬스터: Monster_table의 LongCritical 필드 값*/
		BASE_MAGIC_CRITICAL = 118, /*캐릭터: [Character_table의 MagicCritical 필드 값]+[Level_table의 캐릭터 레벨의 MagicCritical 필드 값]
몬스터: Monster_table의 MagicCritical 필드 값*/
		BASE_MOVE_SPEED = 119, /*캐릭터: Character_table의 RunSpeed 필드 값
몬스터: Monster_table의 RunSpeed 필드 값*/
		BASE_ATTACK_SPEED = 120, /*캐릭터: Skill_Table의 AttackType이 Normal인 공격 애니메이션 프레임(Resource_table에 연결되어 있는 Animation_table의 AnimationID필드에서 AnimationFrame 값)
몬스터: Skill_Table의 AttackType이 Normal인 공격 애니메이션 프레임(Resource_table에 연결되어 있는 Animation_table의 AnimationID필드에서 AnimationFrame 값)*/
		BASE_SKILL_SPEED = 121, /*캐릭터: Skill_Table의 AttackType이 ActiveSkill이나 Special인 공격 애니메이션 프레임(Resource_table에 연결되어 있는 Animation_table의 AnimationID필드에서 AnimationFrame 값)
몬스터: Skill_Table의 AttackType이 ActiveSkill이나 Special인 공격 애니메이션 프레임(Resource_table에 연결되어 있는 Animation_table의 AnimationID필드에서 AnimationFrame 값)*/
		BASE_REDUCTION = 122, /*캐릭터: Character_table의 Reduction 필드 값
몬스터: Monster_table의 Reduction 필드 값*/
		BASE_REDUCTION_IGNORE = 123, /*캐릭터: Character_table의 ReductionIgnore 필드 값
몬스터: Monster_table의 ReductionIgnore 필드 값*/
		BASE_SHORT_EVASION = 124, /*캐릭터: Character_table의 ShortEvasion 필드 값
몬스터: Monster_table의 ShortEvasion 필드 값*/
		BASE_LONG_EVASION = 125, /*캐릭터: [Character_table의 LongEvasion 필드 값]+[Level_table의 캐릭터 레벨의 LongEvasion 필드 값]
몬스터: Monster_table의 LongEEvasion 필드 값*/
		BASE_MAGIC_EVASION = 126, /*캐릭터: [Character_table의 MagicEvasion 필드 값]
몬스터: Monster_table의 MagicEvasion 필드 값*/
		BASE_SHORT_EVASION_IGNORE = 127, /*캐릭터: Character_table의 ShortEvasionIgnore 필드 값
몬스터: Monster_table의 ShortEvasionIgnore 필드 값*/
		BASE_LONG_EVASION_IGNORE = 128, /*캐릭터: Character_table의 LONGEvasionIgnore 필드 값
몬스터: Monster_table의 LONGEvasionIgnore 필드 값*/
		BASE_MAGIC_EVASION_IGNORE = 129, /*캐릭터: Character_table의 MagicEvasionIgnore 필드 값
몬스터: Monster_table의 MagicEvasionIgnore 필드 값*/
		BASE_HP_RECOVERY = 130, /*캐릭터: Character_table의 HPRecovery 필드 값
몬스터: Monster_table의 HPRecovery 필드 값*/
		BASE_HP_RECOVERY_TIME = 131, /*캐릭터: Character_table의 HPRecoveryTime 필드 값
몬스터: Monster_table의 HPRecoveryTime 필드 값*/
		BASE_MP_RECOVERY = 132, /*캐릭터: Character_table의 MPRecovery 필드 값
몬스터: Monster_table의 MPRecovery 필드 값*/
		BASE_MP_RECOVERY_TIME = 133, /*캐릭터: Character_table의 MPRecoveryTime 필드 값
몬스터: Monster_table의 MPRecoveryTime 필드 값*/
		BASE_HP_POTION_RECOVERY = 134, /*Item_table의 ItemType 필드 값이 Portion인 아이템 HPRecoveryMin필드와 HPRecoveryMax필드의 랜덤 값*/
		BASE_MP_POTION_RECOVERY = 135, /*Item_table의 ItemType 필드 값이 Portion인 아이템 MPRecoveryPer필드 값*/
		BASE_MAX_WEIGH = 136, /*Character_table의 MaxWeight 필드 값*/
		BASE_NOT_MOVE_RATE = 137, /*Skill_Table의 AbilityID 필드의 값이 3001일 경우 해당 어빌리티의  AbilityRate 필드 값*/
		BASE_NOT_SKILL_RATE = 138, /*Skill_Table의 AbilityID 필드의 값이 3002일 경우 해당 어빌리티의  AbilityRate 필드 값*/
		BASE_STUN_RATE = 139, /*Skill_Table의 AbilityID 필드의 값이 3005일 경우 해당 어빌리티의  AbilityRate 필드 값*/
		BASE_FEAR_RATE = 140, /*Skill_Table의 AbilityID 필드의 값이 3006일 경우 해당 어빌리티의  AbilityRate 필드 값*/
		BASE_NOT_MOVE_TIME = 141, /*이동금지 발생확률 증가+상태이상 발생확률 증가-대상의 이동금지 저항-대상의 상태이상 저항=MezRate_table의 확률 필드 위치 결정
Skill_Table의 MinSupportTime필드와 MaxSupportTime필드의 구간 값과 동일한 MezRate_table의 GroupID필드 값으로 레코드 위치 결정
= 최종 지속시간 결정 확률 산출 후 지속시간 결정*/
		BASE_NOT_SKILL_TIME = 142, /*공격금지 발생확률 증가+상태이상 발생확률 증가-대상의 공격금지 저항-대상의 상태이상 저항=MezRate_table의 확률 필드 위치 결정
Skill_Table의 MinSupportTime필드와 MaxSupportTime필드의 구간 값과 동일한 MezRate_table의 GroupID필드 값으로 레코드 위치 결정
= 최종 지속시간 결정 확률 산출 후 지속시간 결정*/
		BASE_STUN_TIME = 143, /*스턴 발생확률 증가+상태이상 발생확률 증가-대상의 스턴 저항-대상의 상태이상 저항=MezRate_table의 확률 필드 위치 결정
Skill_Table의 MinSupportTime필드와 MaxSupportTime필드의 구간 값과 동일한 MezRate_table의 GroupID필드 값으로 레코드 위치 결정
= 최종 지속시간 결정 확률 산출 후 지속시간 결정*/
		BASE_FEAR_TIME = 144, /*공포 발생확률 증가+상태이상 발생확률 증가-대상의 공포 저항-대상의 상태이상 저항=MezRate_table의 확률 필드 위치 결정
Skill_Table의 MinSupportTime필드와 MaxSupportTime필드의 구간 값과 동일한 MezRate_table의 GroupID필드 값으로 레코드 위치 결정
= 최종 지속시간 결정 확률 산출 후 지속시간 결정*/
		BASE_GOLD_DROP_AMT = 145, /*MonsterDrop_table의 DropItemType 필드 값이 Gold인 아이템 DropItemMinCount필드와 DropItemMaxCount필드의 랜덤 값
신의 축복 및 신의 권능으로 상승 된 값도 베이스에 포함
위험성이 있기에 골드의 아이템 아이디도 체크를 하여 중복 체크 필요 있음*/
		BASE_ITEM_DROP_RATE = 146, /*MonsterDrop_table의 DropItemType 필드 값이 Item인 아이템 DropRate 필드의 값
신의 축복 및 신의 권능으로 상승 된 값도 베이스에 포함*/
		BASE_EXP_DROP_AMT = 147, /*Monster_table의 ExpCount 필드 값
신의 축복 및 신의 권능으로 상승 된 값도 베이스에 포함*/
		BASE_SKILL_COOLTIME = 148, /*Skill_table의 CoolTime 필드 값*/
		BASE_SKILL_MP = 149, /*Skill_table의 UseMPCount 필드 값*/
		WALK_SPEED = 150, /*걷기 속도*/
		BASE_POTION_RECOVERY_TIME = 151, /*Item_table의 ItemType 필드 값이 Portion인 아이템 CoolTime 필드 값*/
		BASE_PET_MAX_HP = 152, /*기본 펫 생명력*/
		BASE_PET_SHORT_ATTACK = 153, /*기본 펫 근거리 공격력*/
		BASE_PET_LONG_ATTACK = 154, /*기본 펫 원거리 공격력*/
		BASE_PET_MAGIC_ATTACK = 155, /*기본 펫 마법 공격력*/
		BASE_PET_ACCURACY = 156, /*기본 펫 명중*/
		BASE_PET_MELEE_DEFENCE = 157, /*기본 펫 방어력*/
		BASE_PET_MAGIC_DEFENCE = 158, /*기본 펫 마법 방어력*/
		BASE_PET_REDUCTION = 159, /*기본 펫 피해감소*/
		BASE_PET_REDUCTION_IGNORE = 160, /*기본 펫 피해감소 무시*/
		BASE_PET_EVASION = 161, /*기본 펫 회피*/
		BASE_DEBUFF_RATE = 162, /*기본 디버프 확률*/
		BASE_SHORT_CRITICALDAMAGE = 163, /*기본 근거리 크리티컬 데미지*/
		BASE_LONG_CRITICALDAMAGE = 164, /*기본 원거리 크리티컬 데미지*/
		BASE_MAGIC_CRITICALDAMAGE = 165, /*기본 마법 크리티컬 데미지*/
		BASE_SHORT_CRITICAL_MINUS = 166, /*기본 근거리 치명타 확률 감소*/
		BASE_LONG_CRITICAL_MINUS = 167, /*기본 원거리 치명타 확률 감소*/
		BASE_MAGIC_CRITICAL_MINUS = 168, /*기본 마법 치명타 확률 감소*/
		BASE_SHORT_CRITICALDAMAGE_MINUS = 169, /*기본 근거리 치명타 데미지 감소 감소*/
		BASE_LONG_CRITICALDAMAGE_MINUS = 170, /*기본 원거리 치명타 데미지 감소 감소*/
		BASE_MAGIC_CRITICALDAMAGE_MINUS = 171, /*기본 마법 치명타 데미지 감소 감소*/
		BASE_DOT_RATE = 172, /*기본 도트 발동 확률*/
		BASE_SHORT_DOT_RATE_DAMAGE = 173, /*기본 근거리 도트 비율 데미지*/
		BASE_SHORT_DOT_PLUS_DAMAGE = 174, /*기본 근거리 도트 데미지*/
		BASE_LONG_DOT_RATE_DAMAGE = 175, /*기본 원거리 도트 비율 데미지*/
		BASE_LONG_DOT_PLUS_DAMAGE = 176, /*기본 원거리 도트 데미지*/
		BASE_MAGIC_DOT_RATE_DAMAGE = 177, /*기본 마법 도트 비율 데미지*/
		BASE_MAGIC_DOT_PLUS_DAMAGE = 178, /*기본 마법 도트 데미지*/
		STR_PLUS = 1001, /*힘(+)*/
		STR_PER = 1002, /*힘(%)*/
		DEX_PLUS = 1003, /*민첩(+)*/
		DEX_PER = 1004, /*민첩(%)*/
		INT_PLUS = 1005, /*지능(+)*/
		INT_PER = 1006, /*지능(%)*/
		WIS_PLUS = 1007, /*지혜(+)*/
		WIS_PER = 1008, /*지혜(%)*/
		VIT_PLUS = 1009, /*체력(+)*/
		VIT_PER = 1010, /*체력(%)*/
		MAX_HP_PLUS = 1011, /*생명력 최대 량(+)*/
		MAX_HP_PER = 1012, /*생명력 최대 량(%)*/
		MAX_MP_PLUS = 1013, /*마나 최대 량(+)*/
		MAX_MP_PER = 1014, /*마나 최대 량(%)*/
		SHORT_ATTACK_PLUS = 1015, /*근거리 공격력(+)*/
		SHORT_ATTACK_PER = 1016, /*근거리 공격력(%)*/
		LONG_ATTACK_PLUS = 1017, /*원거리 공격력(+)*/
		LONG_ATTACK_PER = 1018, /*원거리 공격력(%)*/
		WEAPON_MELEE_ATTACK = 1019, /*무기 물리 공격력(+)*/
		ATTACK_PLUS = 1020, /*공격력(+)*/
		ATTACK_PER = 1021, /*공격력(%)*/
		MAGIC_ATTACK_PLUS = 1022, /*마법 공격력(+)*/
		MAGIC_ATTACK_PER = 1023, /*마법 공격력(%)*/
		PVP_SHORT_ATTACK_PLUS = 1024, /*PvP 근거리 공격력(+)*/
		PVP_LONG_ATTACK_PLUS = 1025, /*PvP 원거리 데미지(+)*/
		PVP_MAGIC_ATTACK_PLUS = 1026, /*PvP 마법 데미지(+)*/
		SHORT_ACCURACY_PLUS = 1027, /*근거리 명중(+)*/
		SHORT_ACCURACY_PER = 1028, /*근거리 명중(%)*/
		LONG_ACCURACY_PLUS = 1029, /*원거리 명중(+)*/
		LONG_ACCURACY_PER = 1030, /*원거리 명중(%)*/
		MAGIC_ACCURACY_PLUS = 1031, /*마법 명중(+)*/
		MAGIC_ACCURACY_PER = 1032, /*마법 명중(%)*/
		ACCURACY_PLUS = 1033, /*명중(+)*/
		ACCURACY_PER = 1034, /*명중(%)*/
		SHORT_CRITICAL_PLUS = 1035, /*근거리 치명타(+)*/
		LONG_CRITICAL_PLUS = 1036, /*원거리 치명타(+)*/
		MAGIC_CRITICAL_PLUS = 1037, /*마법 치명타(+)*/
		CRITICAL_PLUS = 1038, /*치명타(+)*/
		SHORT_CRITICAL_MINUS = 1039, /*받는 근거리 치명타 감소*/
		LONG_CRITICAL_MINUS = 1040, /*받는 원거리 치명타 감소*/
		MAGIC_CRITICAL_MINUS = 1041, /*받는 마법 치명타 감소*/
		CRITICAL_MINUS = 1042, /*받는 치명타 감소*/
		SHORT_CRITICALDAMAGE_PLUS = 1043, /*근거리 치명타 데미지*/
		LONG_CRITICALDAMAGE_PLUS = 1044, /*원거리 치명타 데미지*/
		MAGIC_CRITICALDAMAGE_PLUS = 1045, /*마법 치명타 데미지*/
		CRITICALDAMAGE_PLUS = 1046, /*치명타 데미지*/
		SHORT_CRITICALDAMAGE_MINUS = 1047, /*받는 근거리 치명타 데미지 감소*/
		LONG_CRITICALDAMAGE_MINUS = 1048, /*받는 원거리 치명타 데미지 감소*/
		MAGIC_CRITICALDAMAGE_MINUS = 1049, /*받는 마법 치명타 데미지 감소*/
		CRITICALDAMAGE_MINUS = 1050, /*받는 치명타 데미지 감소*/
		MELEE_DEFENCE_PLUS = 1051, /*물리 방어력(+)*/
		MELEE_DEFENCE_PER = 1052, /*물리 방어력(%)*/
		MAGIC_DEFENCE_PLUS = 1053, /*마법 방어력(+)*/
		MAGIC_DEFENCE_PER = 1054, /*마법 방어력(%)*/
		DEFENCE_PLUS = 1055, /*방어력(+)*/
		DEFENCE_PER = 1056, /*방어력(%)*/
		MOVE_SPEED_PER = 1057, /*이동속도(%)*/
		MOVE_SPEED_MINUS_PER = 1058, /*이동속도 감소(%)*/
		ATTACK_SPEED_PER = 1059, /*공격속도(%)*/
		ATTACK_SPEED_MINUS_PER = 1060, /*공격속도 감소(%)*/
		SKILL_SPEED_PER = 1061, /*시전속도(%)*/
		SKILL_SPEED_MINUS_PER = 1062, /*시전속도 감소(%)*/
		SPEED_PER = 1063, /*속도(%)*/
		REDUCTION_PLUS = 1064, /*피해감소(+)*/
		SHORT_REDUCTION_PLUS = 1065, /*근거리 피해감소(+)*/
		LONG_REDUCTION_PLUS = 1066, /*원거리 피해감소(+)*/
		MAGIC_REDUCTION_PLUS = 1067, /*마법 피해감소(+)*/
		REDUCTION_PER = 1068, /*데미지 감소율(%)*/
		PVP_REDUCTION_PER = 1069, /*PvP 데미지 감소율(%)*/
		SHORT_REDUCTION_PER = 1070, /*근거리 공격력 감소율(%)*/
		LONG_REDUCTION_PER = 1071, /*원거리 데미지 감소율(%)*/
		MAGIC_REDUCTION_PER = 1072, /*마법 피데미지 감소율(%)*/
		REDUCTION_IGNORE_PLUS = 1073, /*피해감소 무시(+)*/
		SHORT_REDUCTION_IGNORE_PLUS = 1074, /*근거리 피해감소 무시(+)*/
		LONG_REDUCTION_IGNORE_PLUS = 1075, /*원거리 피해감소 무시(+)*/
		MAGIC_REDUCTION_IGNORE_PLUS = 1076, /*마법 피해감소 무시(+)*/
		REDUCTION_IGNORE_PER = 1077, /*데미지 감소율 무시(%)*/
		PVP_REDUCTION_IGNORE_PER = 1078, /*PvP 데미지 감소율 무시(%)*/
		SHORT_REDUCTION_IGNORE_PER = 1079, /*근거리 공격력 감소율 무시(%)*/
		LONG_REDUCTION_IGNORE_PER = 1080, /*원거리 데미지 감소율 무시(%)*/
		MAGIC_REDUCTION_IGNORE_PER = 1081, /*마법 데미지 감소율 무시(%)*/
		SHORT_EVASION_PLUS = 1082, /*근거리 회피(+)*/
		LONG_EVASION_PLUS = 1083, /*원거리 회피(+)*/
		MAGIC_EVASION_PLUS = 1084, /*마법 회피(+)*/
		EVASION_PLUS = 1085, /*회피(+)*/
		SHORT_EVASION_PER = 1086, /*근거리 회피(%)*/
		LONG_EVASION_PER = 1087, /*원거리 회피(%)*/
		MAGIC_EVASION_PER = 1088, /*마법 회피(%)*/
		EVASION_PER = 1089, /*회피(%)*/
		SHORT_EVASION_IGNORE_PLUS = 1090, /*근거리 회피무시(+)*/
		LONG_EVASION_IGNORE_PLUS = 1091, /*원거리 회피무시(+)*/
		MAGIC_EVASION_IGNORE_PLUS = 1092, /*마법 회피무시(+)*/
		EVASION_IGNORE_PLUS = 1093, /*회피무시(+)*/
		MAX_WEIGH_PLUS = 1094, /*무게 최대 량(+)*/
		MAX_WEIGH_PER = 1095, /*무게 최대 량(%)*/
		HP_AUTO_RECOVERY_PLUS = 1096, /*캐릭터 자동 생명력 회복량(+)*/
		MP_AUTO_RECOVERY_PLUS = 1097, /*캐릭터 자동 마나 회복량(+)*/
		HP_RECOVERY_TIME_PLUS = 1098, /*생명력 회복 주기 시간 감소량(+)*/
		HP_RECOVERY_TIME_PER = 1099, /*생명력 회복 주기 시간(%)*/
		MP_RECOVERY_TIME_PLUS = 1100, /*마나 회복 주기 시간 감소량(+)*/
		MP_RECOVERY_TIME_PER = 1101, /*마나 회복 주기 시간(%)*/
		POTION_RECOVERY_PLUS = 1102, /*물약 회복량(+)*/
		POTION_RECOVERY_PER = 1103, /*물약 회복량(%)*/
		POTION_RECOVERY_TIME_PER = 1104, /*물약 회복 시간(%)*/
		SKILL_COOLTIME_PLUS = 1105, /*스킬 재사용 시간 감소량(+)*/
		SKILL_COOLTIME_PER = 1106, /*스킬 재사용 시간(%)*/
		GOLD_DROP_AMT_PLUS = 1107, /*골드 획득 량(+)*/
		GOLD_DROP_AMT_PER = 1108, /*골드 획득 량(%)*/
		GOLD_DROP_AMT_GOD_PER = 1109, /*골드 획득율*/
		EXP_DROP_AMT_PLUS = 1110, /*경험치 획득 량(+)*/
		EXP_DROP_AMT_PER = 1111, /*경험치 획득 량(%)*/
		EXP_DROP_AMT_GOD_PER = 1112, /*경험치 획득율*/
		ITEM_DROP_RATE_PLUS = 1113, /*아이템 획득 확률(+)*/
		ITEM_DROP_RATE_PER = 1114, /*아이템 획득 확률(%)*/
		SKILL_MP_PLUS = 1115, /*스킬 마나 소모 감소량(%)*/
		SKILL_MP_PER = 1116, /*스킬 마나 소모량(%)*/
		STATE_NOT_MOVE = 1117, /*이동금지*/
		STATE_NOT_SKILL = 1118, /*침묵*/
		STATE_NOT_POTION = 1119, /*물약 사용 불가*/
		STATE_NOT_RETURN = 1120, /*귀환 불가*/
		STATE_STUN = 1121, /*스턴*/
		STATE_FEAR = 1122, /*공포*/
		RUSH = 1123, /*돌진*/
		STATE_VISION = 1124, /*시야축소*/
		STATE_SPECIAL_VISION = 1125, /*스페셜 시야축소*/
		UNBEATABLE = 1126, /*무적*/
		HP_CHECK_ATTACK_UP = 1127, /*잃은 체력 비례 데미지 증가*/
		HP_CHANGE_MP = 1128, /*생명력 마나로 전환*/
		HIT_HP_CHANGE_MP = 1129, /*생명력 대신 마나 소모*/
		MONSTER_SUMMON = 1130, /*몬스터 소환*/
		DAMAGE_REFLECT = 1131, /*데미지 반사*/
		SPECIAL_EVASION = 1132, /*스페셜 회피*/
		SHORT_SPECIAL_EVASION = 1133, /*근거리 스페셜 회피*/
		LONG_SPECIAL_EVASION = 1134, /*원거리 스페셜 회피*/
		MAGIC_SPECIAL_EVASION = 1135, /*마법 스페셜 회피*/
		MP_BURN = 1136, /*마나번*/
		HP_POTION = 1137, /*생명력 포션 기본 회복량*/
		MP_POTION = 1138, /*마나 포션 기본 회복량*/
		HP_ABSORB = 1139, /*HP흡수*/
		ADD_DAMAGE = 1140, /*추가 데미지*/
		LINK_ABILITY_BUFF = 1141, /*링크 어빌리트 적용 버프*/
		LONG_ACCURACY_LONG_ATTACK = 1142, /*원거리 명중 대비 원거리 데미지 증가*/
		SKILL_BUFF_DELETE = 1143, /*스킬 버프 제거*/
		STUN_DELETE = 1144, /*스턴 제거*/
		FEAR_DELETE = 1145, /*공포 제거*/
		NOT_MOVE_DELETE = 1146, /*이동금지 제거*/
		NOT_SKILL_DELETE = 1147, /*침묵 제거*/
		MP_ABSORB = 1148, /*MP흡수*/
		TOTAL_DAMAGE_PER = 1149, /*데미지의 일정% 추가 데미지 발생*/
		MAGIC_ACCURACY_SHORT_ATTACK = 1150, /*마법 명중 대비 근거리 공격력 증가*/
		MP_NOT_USE = 1151, /*스킬 마나 소모 무시*/
		GET_CRITICAL_PLUS = 1152, /*받는 치명타 증가량(몬스터)*/
		MAGIC_ADD_DAMAGE = 1153, /*마법 데미지 퍼센트 적용*/
		SKILL_DAMAGE_PLUS = 1154, /*특정 스킬 데미지 증가*/
		SKILL_BUFF_RATE_PLUS = 1155, /*특정 스킬 발동확률 증가*/
		SKILL_BUFF_TIME_PLUS = 1156, /*특정 스킬 버프 지속시간 증가*/
		SKILL_COOLTIME_MIMUS = 1157, /*특정 스킬 쿨타임 감소*/
		SKILL_USE_MP_MIMUS = 1158, /*특정 스킬 마나 소모량 감소*/
		ANTI_DEFENCE = 1159, /*CC기 안티*/
		HP_CHECK_PER_DOWN = 1160, /*현재 체력 %체크*/
		DEFENCE_IGNORE_PER = 1161, /*대상 방어력 무시*/
		REPEAT_HP_PER_DECREMENT = 1162, /*중복 체력 감소 (레이드용)*/
		SPECIAL_ACCURACY = 1163, /*스페셜 명중*/
		SPECIAL_SHORT_ACCURACY = 1164, /*스페셜 근거리 명중*/
		SPECIAL_LONG_ACCURACY = 1165, /*스페셜 원거리 명중*/
		SPECIAL_MAGIC_ACCURACY = 1166, /*스페셜 마법 명중*/
		STATE_LEAP = 1167, /*도약*/
		STATE_KNOCKBACK = 1168, /*넉백*/
		STATE_PULL = 1169, /*당기기*/
		STATE_BANKJUMP = 1170, /*후방이동*/
		STATE_AURA = 1171, /*아우라*/
		STATE_SHORT_DOT = 1172, /*근거리 도트*/
		STATE_LONG_DOT = 1173, /*원거리 도트*/
		STATE_MAGIC_DOT = 1174, /*마법 도트*/
		HP_HEAL = 1175, /*회복*/
		SUMMON = 1176, /*소환수 소환*/
		NOT_MOVE_TIME_UP_PLUS = 1177, /*이동금지 지속시간 증가(+)*/
		NOT_MOVE_TIME_UP_PER = 1178, /*이동금지 지속시간 증가(%)*/
		NOT_MOVE_TIME_DOWN_PLUS = 1179, /*이동금지 지속시간 감소(+)*/
		NOT_MOVE_TIME_DOWN_PER = 1180, /*이동금지 지속시간 감소(%)*/
		NOT_MOVE_RATE_UP_PER = 1181, /*이동금지 발생확률 증가(+)*/
		NOT_MOVE_RATE_DOWN_PER = 1182, /*이동금지 저항(+)*/
		NOT_SKILL_TIME_UP_PLUS = 1183, /*스킬사용금지 지속시간 증가(+)*/
		NOT_SKILL_TIME_UP_PER = 1184, /*스킬사용금지 지속시간 증가(%)*/
		NOT_SKILL_TIME_DOWN_PLUS = 1185, /*스킬사용금지 지속시간 감소(+)*/
		NOT_SKILL_TIME_DOWN_PER = 1186, /*스킬사용금지 지속시간 감소(%)*/
		NOT_SKILL_RATE_UP_PER = 1187, /*스킬사용금지 발생확률 증가(+)*/
		NOT_SKILL_RATE_DOWN_PER = 1188, /*스킬사용금지 저항(+)*/
		STUN_TIME_UP_PLUS = 1189, /*스턴 지속시간 증가(+)*/
		STUN_TIME_UP_PER = 1190, /*스턴 지속시간 증가(%)*/
		STUN_TIME_DOWN_PLUS = 1191, /*스턴 지속시간 감소(+)*/
		STUN_TIME_DOWN_PER = 1192, /*스턴 지속시간 감소(%)*/
		STUN_RATE_UP_PER = 1193, /*스턴 발생확률 증가(+)*/
		STUN_RATE_DOWN_PER = 1194, /*스턴 저항(+)*/
		FEAR_TIME_UP_PLUS = 1195, /*공포 지속시간 증가(+)*/
		FEAR_TIME_UP_PER = 1196, /*공포 지속시간 증가(%)*/
		FEAR_TIME_DOWN_PLUS = 1197, /*공포 지속시간 감소(+)*/
		FEAR_TIME_DOWN_PER = 1198, /*공포 지속시간 감소(%)*/
		FEAR_RATE_UP_PER = 1199, /*공포 발생확률 증가(+)*/
		FEAR_RATE_DOWN_PER = 1200, /*공포 저항(+)*/
		MAZ_TIME_UP_PLUS = 1201, /*상태이상 지속시간 증가(+)*/
		MAZ_TIME_UP_PER = 1202, /*상태이상 지속시간 증가(%)*/
		MAZ_TIME_DOWN_PLUS = 1203, /*상태이상 지속시간 감소(+)*/
		MAZ_TIME_DOWN_PER = 1204, /*상태이상 지속시간 감소(%)*/
		MAZ_RATE_UP_PER = 1205, /*상태이상 발생활률 증가(+)*/
		MAZ_RATE_DOWN_PER = 1206, /*상태이상 저항(+)*/
		SKILL_ABILITYPOINT_PLUS = 1207, /*특정 스킬 능력치 증가*/
		SKILL_ALL_ABILITYPOINT_PLUS = 1208, /*특정 스킬 모든 능력치 증가*/
		SKILL_HEAL_PLUS = 1209, /*특정 스킬 HP 회복량 증가*/
		SKILL_DISTANCE_PLUS = 1210, /*특정 스킬 거리 증가(도약)*/
		IMMUNE_NOT_MOVE = 1211, /*이동금지 면역*/
		IMMUNE_NOT_SKILL = 1212, /*침묵 면역*/
		IMMUNE_FEAR = 1213, /*공포 면역*/
		IMMUNE_STUN = 1214, /*스턴 면역*/
		IMMUNE_MAZ = 1215, /*상태이상 면역*/
		ITEM_WEIGH = 1216, /*현재 아이템 무게*/
		IMMUNE_KNOCKBACK = 1217, /*넉백 면역*/
		IMMUNE_PULL = 1218, /*당기기 면역*/
		IMMUNE_SPASTICITY = 1219, /*경직 면역*/
		RETURN = 2001, /*귀환*/
		GOOD_NATURED_PLUS = 2002, /*선 수치 증가(+)*/
		STAGE_SUPPORT_TIME = 2003, /*스테이지 체류 버프 증가(+)*/
		ITEM_AUTO_BREAK = 2004, /*각인 아이템 자동 분해*/
		SELF_CHANNEL_SUPPORT_TIME = 2005, /*전용 채널 유지 시간*/
		DAILY_QUEST_COMPLETE = 2006, /*일일 퀘스트 즉시완료*/
		EVENT_SUPPORT_TIME = 2007, /*이벤트 지역 체류 버프(+)*/
		CHANGE_GACHA_OPEN = 2008, /*상급 변신 카드 뽑기 오픈*/
		PET_GACHA_OPEN = 2009, /*상급 펫 카드 뽑기 오픈*/
		GOD_TEAR_OPEN = 2010, /*신의 축복 오픈*/
		HP_POTION_BUY_AUTO = 2011, /*체력 물약 자동 구입*/
		PK_CONDITION = 2012, /*PK 상태(보라돌이)*/
		RUNE_MAX_HP_PLUS = 3001, /*룬 생명력*/
		RUNE_MAX_HP_PER = 3002, /*룬 생명력(%)*/
		RUNE_ATTACK_PLUS = 3003, /*룬 공격력*/
		RUNE_ATTACK_PER = 3004, /*룬 공격력*/
		RUNE_SHORT_ATTACK_PLUS = 3005, /*룬 근거리 공격력*/
		RUNE_SHORT_ATTACK_PER = 3006, /*룬 근거리 공격력*/
		RUNE_LONG_ATTACK_PLUS = 3007, /*룬 원거리 공격력*/
		RUNE_LONG_ATTACK_PER = 3008, /*룬 원거리 공격력*/
		RUNE_MAGIC_ATTACK_PLUS = 3009, /*룬 마법 공격력*/
		RUNE_MAGIC_ATTACK_PER = 3010, /*룬 마법 공격력*/
		RUNE_ACCURACY_PLUS = 3011, /*룬 명중*/
		RUNE_ACCURACY_PER = 3012, /*룬 명중*/
		RUNE_DEFENCE_PLUS = 3013, /*룬 방어력*/
		RUNE_DEFENCE_PER = 3014, /*룬 방어력*/
		RUNE_MELEE_DEFENCE_PLUS = 3015, /*룬 물리 방어력*/
		RUNE_MELEE_DEFENCE_PER = 3016, /*룬 물리 방어력*/
		RUNE_MAGIC_DEFENCE_PLUS = 3017, /*룬 마법 방어력*/
		RUNE_MAGIC_DEFENCE_PER = 3018, /*룬 마법 방어력*/
		RUNE_MOVE_SPEED_PER = 3019, /*룬 이동속도*/
		RUNE_ATTACK_SPEED_PER = 3020, /*룬 공격속도*/
		RUNE_REDUCTION_PLUS = 3021, /*룬 피해감소*/
		RUNE_REDUCTION_PER = 3022, /*룬 피해감소*/
		RUNE_REDUCTION_IGNORE_PLUS = 3023, /*룬 피해감소 무시*/
		RUNE_REDUCTION_IGNORE_PER = 3024, /*룬 피해감소 무시*/
		RUNE_EVASION_PLUS = 3025, /*룬 회피*/
		RUNE_EVASION_PER = 3026, /*룬 회피*/
		RUNE_POTION_RECOVERY_PLUS = 3027, /*룬 포션 회복량*/
		RUNE_MAZ_RATE_DOWN_PER = 3028, /*룬 상태이상 저항*/
		RUNE_MAZ_RATE_UP_PER = 3029, /*룬 상태이상 적중*/
		RUNE_MP_AUTO_RECOVERY_PLUS = 3030, /*룬 마나 회복*/
		ITEM_DROP_RATE_VIEW = 4071, /*아이템 획득율 표시 용*/
		PET_DROP_RATE_VIEW = 4072, /*펫 획득율 표시 용*/
		RUNE_DROP_RATE_VIEW = 4073, /*룬 획득율 표시 용*/
		SHORT_REDUCTION_DECREASE = 5001, /*근거리 피해감소 량*/
		LONG_REDUCTION_DECREASE = 5002, /*원거리 피해감소 량*/
		MAGIC_REDUCTION_DECREASE = 5003, /*마법 피해감소 량*/
		SHORT_CRITICAL_RATE = 5004, /*근거리 치명타 확률*/
		LONG_CRITICAL_RATE = 5005, /*원거리 치명타 확률*/
		MAGIC_CRITICAL_RATE = 5006, /*마법 치명타 확률*/
		SHORT_CRITICALDAMAGE = 5007, /*근거리 치명타 데미지*/
		LONG_CRITICALDAMAGE = 5008, /*원거리 치명타 데미지*/
		MAGIC_CRITICALDAMAGE = 5009, /*마법 치명타 데미지*/
	}

	public enum E_UnitType
	{
		None = 0, /*없음*/
		Monster = 1, /*몬스터*/
		Character = 2, /*캐릭터*/
		NPC = 3, /*NPC*/
		Gimmick = 4, /*기믹*/
		Summon = 5, /*소환수*/
		Pet = 6, /*펫/탈것*/
		Object = 7, /*오브젝트 (채집등)*/
	}

	public enum E_WeaponType
	{
		None = 0, /*없음*/
		Sword = 1, /*한손검*/
		Bow = 2, /*활*/
		Wand = 3, /*지팡이*/
		TwoSwords = 4, /*쌍수*/
	}

	public enum E_PreconditionType
	{
		Not = 0, /*스킬 사전 조건 체크 안함*/
		HPPer = 1, /*스킬 사용 시 현재 생명력 체크 (미만 스킬 사용 불가)*/
		HP = 2, /*스킬 사용 시 현재 생명력 체크 (미만 스킬 사용 불가)*/
		Attack = 3, /*스킬 사용 시 현재 공격력 체크 (미만 스킬 사용 불가)*/
		MeleeDefence = 4, /*스킬 사용 시 현재 물리 방어력 체크 (미만 스킬 사용 불가)*/
		MagicDefence = 5, /*스킬 사용 시 현재 마법 방어력 체크 (미만 스킬 사용 불가)*/
	}

	public enum E_SkillType
	{
		Normal = 0, /*일반 공격*/
		ActiveSkill = 1, /*액티브 스킬*/
		PassiveSkill = 2, /*패시브 스킬*/
		BuffSkill = 3, /*버프 스킬*/
	}

	public enum E_SkillEventType
	{
		Normal = 0, /*일반 시전*/
		Missile = 1, /*발사*/
		Rush = 2, /*돌진(좌표이동)*/
		Leap = 3, /*도약(좌표이동)*/
		Pull = 4, /*당기기(좌표이동)*/
		KnockBack = 5, /*넉백(좌표이동)*/
		Teleport = 6, /*순간이동(좌표이동)*/
		Summon = 7, /*소환*/
		BackJump = 8, /*후방이동*/
	}

	public enum E_TargetPosType
	{
		Target = 0, /*타겟 대상*/
		Area = 1, /*지형*/
		Self = 2, /*자신*/
		Starting = 3, /*시작 시*/
	}

	public enum E_SkillAniType
	{
		None = 0, /*애니 없음*/
		Attack_01 = 1, /*일반 공격 1*/
		Attack_02 = 2, /*일반 공격 2*/
		Attack_03 = 3, /*일반 공격 3*/
		Skill_01 = 4, /*스킬 1*/
		Skill_02 = 5, /*스킬 2*/
		Buff = 6, /*버프 1*/
		Casting = 7, /*캐스팅 타입*/
		Rush = 8, /*돌진 타입*/
		Leap = 9, /*점프 타입*/
		Pull = 10, /*당기기 타입*/
	}

	public enum E_DamageType
	{
		Not = 0, /*데미지 없음*/
		NormalDamage = 1, /*일반 데미지*/
		SkillDamage = 2, /*액티브 스킬 데미지*/
	}

	public enum E_RangeType
	{
		Not = 0, /*범위 아님*/
		Angle = 1, /*각도(주체 기준 각도)*/
		Straight = 2, /*직선*/
	}

	public enum E_ServerCheckType
	{
		Not = 0, /*체크 않함*/
		Check = 1, /*체크*/
	}

	public enum E_AttributeType
	{
		Buff = 1, /*버프*/
		Heal = 2, /*회복*/
		Mez = 3, /*행동제어*/
		Rush = 4, /*돌진*/
		Vision = 5, /*시야축소*/
		Summon = 6, /*몬스터 소환*/
		MPBurn = 7, /*마나번*/
		Shot = 8, /*일회성*/
		Leap = 9, /*도약*/
		Pull = 10, /*당기기*/
		Aura = 11, /*아우라*/
		Dot = 12, /*도트*/
		BackJump = 13, /*후방이동*/
	}

	public enum E_TargetType
	{
		Enemmy = 0, /*적*/
		Self = 1, /*자신*/
		Party = 2, /*자신 포함 파티*/
		Summoner = 3, /*소환사*/
	}

	public enum E_CastingType
	{
		None = 0, /*캐스팅 사용안함*/
		Casting = 1, /*캐스팅 사용*/
		Channeling = 2, /*시전 유지*/
	}

	[System.Flags]
	public enum E_CastingDeleteType
	{
		None = 0, /*사용 않하거나 취소 않됨*/
		KnockBack = 1, /*넉백 시*/
		NotMove = 2, /*이동금지*/
		NotSkill = 4, /*침묵*/
		Fear = 8, /*공포*/
		Stun = 16, /*스턴*/
		Hit = 32, /*피격 받을 때*/
	}

	public enum E_AttackType
	{
		None = 0, /*사용 않함*/
		NotUse = 1, /*데미지 없음*/
		Short = 2, /*근거리 공격*/
		Long = 3, /*원거리 공격*/
		Magic = 4, /*마법 공격*/
		ShortWithoutDodge = 5, /*마법사 일반공격*/
	}

	public enum E_MissileType
	{
		Not = 0, /*발사체 없음*/
		Target = 1, /*타겟형(발사체 이미지는 연출용이고 타겟에 영향을 준다.)*/
		Penetration = 2, /*관통형(미사일 사거리까지 이동하고 발사체와 충돌하는 대상에게 영향을 준다. TargetType의 조건 대상에게)*/
		Explosion = 3, /*폭발형(미사일 사거리까지 이동하고 발사체와 최초 충돌하는 대상에게 영향을 준다. TargetType의 조건 대상에게)*/
	}

	[System.Flags]
	public enum E_PosMoveType
	{
		None = 0, /*없음*/
		TargetMoveCaster = 1, /*타겟을 시전자 앞으로*/
		TargetKnockBack = 2, /*타겟 넉백*/
		CasterRandomMove = 4, /*시전자 순간이동*/
		CasterRush = 8, /*시전자 돌진*/
		CasterLeap = 16, /*시전자 도약*/
		CasterBackJump = 32, /*후방이동*/
		Teleport = 64, /*텔레포트*/
	}

	public enum E_AggroType
	{
		None = 0, /*없음*/
		Target = 1, /*해당 스킬이 적용되는 대상*/
		Range = 2, /*스킬 사용 시 시전자 기준 범위의 몬스터*/
	}

	public enum E_MonAI_Type
	{
		Not = 0, /*없음*/
		Rate = 1, /*확률*/
		HP = 2, /*생명력 량*/
		Aggro = 3, /*어그로 교체 시 발동*/
		CasterHP = 4, /*시전자 생명력 량*/
	}

	public enum E_HitAniType
	{
		NotUse = 0, /*사용 않함*/
		Use = 1, /*사용*/
	}

	public enum E_MissileEffectType
	{
		None = 0, /*없음*/
		MissileEffect_Base = 1, /*캐릭터 기본 발사체*/
		MissileEffect_01 = 2, /*캐릭터 발사체_01*/
		MissileEffect_02 = 3, /*캐릭터 발사체_02*/
		MissileEffect_03 = 4, /*캐릭터 발사체_03*/
		MissileEffect_04 = 5, /*캐릭터 발사체_04*/
	}

	public enum E_HitPositionType
	{
		None = 0, /*없음*/
		Upper = 1, /*상단*/
		Middle = 2, /*중단*/
		Lower = 3, /*하단*/
	}

	public enum E_AutoSlot
	{
		On = 0, /*사용*/
		Off = 1, /*미사용*/
	}

	public enum E_StepUpType
	{
		Not = 0, /*레벨업 불가능*/
		LevelUp = 1, /*레벨업 가능*/
		SpecialUp = 2, /*궁극기 레벨업 가능*/
	}

	public enum E_ApplicationType
	{
		None = 0, /*없음*/
		OneShot = 1, /*1회 발동*/
		Dot = 2, /*도트*/
		Aura = 3, /*아우라*/
	}

	public enum E_TownUseType
	{
		NotUse = 0, /*사용 않함*/
		Use = 1, /*사용*/
	}

	public enum E_SummonType
	{
		Summon = 0, /*소환수*/
		Gimmick = 1, /*기믹*/
		Monster = 2, /*몬스터*/
	}

	public enum E_CallType
	{
		None = 0, /*없음*/
		Fix = 1, /*고정*/
		Target = 2, /*타겟 위치*/
		Random = 3, /*반경 랜덤*/
		Touch = 4, /*지형 터치*/
	}

	public enum E_ActiveType
	{
		None = 0, /*사용 않함*/
		Auto = 1, /*자동 사용*/
		Follow = 2, /*캐릭터 연계 사용*/
	}

	[System.Flags]
	public enum E_DeathType
	{
		None = 0, /*없음*/
		CharDeath = 1, /*캐릭터 사망*/
		Time = 2, /*소환 시간 종료*/
		HP = 4, /*생명력 0 이하*/
		Skill = 8, /*스킬 사용 후*/
	}

	public enum E_UnitAttributeType
	{
		None = 0, /*없음, 시전자 속성*/
		Fire = 1, /*화 속성*/
		Water = 2, /*수 속성*/
		Electric = 3, /*전기 속성*/
		Light = 4, /*빛 속성*/
		Dark = 5, /*어둠 속성*/
	}

	public enum E_ItemUseType
	{
		Equip = 1, /*장비*/
		Goods = 2, /*재화*/
		Potion = 3, /*포션*/
		Enchant = 4, /*강화*/
		Upgrade = 5, /*승급*/
		Buff = 6, /*버프*/
		Gacha = 7, /*가차*/
		Indulgence = 8, /*면죄부*/
		Change = 9, /*변신*/
		PetSummon = 10, /*펫 소환*/
		Ticket = 11, /*입장 티켓*/
		Material = 12, /*재료*/
		Temple = 13, /*유적 입장*/
		Event = 14, /*이벤트*/
		Move = 15, /*위치 이동*/
		UseItem = 16, /*사용 아이템*/
		ChickenCoupon = 17, /*치킨 쿠폰*/
		SkillBook = 18, /*스킬북*/
		Restoration = 19, /*복구 쿠폰*/
		Rune = 20, /*룬*/
		SmeltScroll = 21, /*제련서*/
		Teleport = 22, /*순간이동*/
		Gem = 23, /*젬*/
		Ingredients = 24, /*요리재료*/
		Food = 25, /*요리*/
		OptionStone = 26, /*옵션석*/
	}

	public enum E_ItemType
	{
		Weapon = 1, /*무기*/
		SideWeapon = 2, /*보조무기*/
		Helmet = 3, /*투구*/
		Armor = 4, /*갑옷*/
		Pants = 5, /*바지*/
		Shoes = 6, /*신발*/
		Gloves = 7, /*장갑*/
		Cape = 8, /*망토*/
		Ring = 9, /*반지*/
		Earring = 10, /*귀걸이*/
		Necklace = 11, /*목걸이*/
		Belt = 12, /*벨트*/
		Tshirt = 13, /*티셔츠*/
		Goods = 14, /*재화*/
		Mileage = 15, /*마일리지*/
		StatPoint = 16, /*스텟상승 포인트*/
		SkillPoint = 17, /*스킬상승 포인트*/
		HPPotion = 18, /*HP포션*/
		MPPotion = 19, /*MP포션*/
		WeaponEnchant = 20, /*무기 강화석*/
		DefenseEnchant = 21, /*방어구 강화석*/
		AccessoryEnchant = 22, /*장신구 강화석*/
		Upgrade = 23, /*승급석*/
		SpeedBuff = 24, /*신속의 물약*/
		AttackBuff = 25, /*공격의 물약*/
		DefenseBuff = 26, /*방어의 물약*/
		EvasionBuff = 27, /*회피의 물약*/
		DespairTower = 28, /*절망의 탑 유지 버프*/
		Move = 29, /*위치 이동*/
		GodTear = 30, /*신의 눈물*/
		ZeroBless = 31, /*제로의 축복*/
		ChangeGacha = 32, /*변신 뽑기*/
		PetGacha = 33, /*펫 뽑기*/
		Indulgence = 34, /*면죄부*/
		Change = 35, /*변신 주문서*/
		PetSummon = 36, /*펫 소환서*/
		InterServerTicket = 37, /*인터서버 티켓*/
		GuildDungeonTicket = 38, /*길드던전 티켓*/
		ItemGacha = 39, /*아이템 뽑기*/
		Material = 40, /*재료*/
		Quest = 41, /*퀘스트*/
		Event = 42, /*이벤트*/
		SelfChannel = 43, /*전용채널 버프*/
		Slot = 44, /*슬롯 확장*/
		Exp = 45, /*경험치*/
		ChickenCoupon = 46, /*치킨 쿠폰*/
		EventBuff = 47, /*이벤트 던전 체류 아이템*/
		FixGacha = 48, /*선택 확정권*/
		NameChange = 49, /*닉네임 변경권*/
		SkillBook = 50, /*스킬 북*/
		Restoration = 51, /*복구 쿠폰*/
		NonDestroyWeaponEnchant = 52, /*파괴 방지 무기 강화석*/
		NonDestroyDefenseEnchant = 53, /*파괴 방지 방어구 강화석*/
		NonDestroyAccessoryEnchant = 54, /*파괴 방지 장신구 강화석*/
		ClassChange = 55, /*클래스 변경권*/
		Rune_01 = 56, /*1번 룬*/
		Rune_02 = 57, /*2번 룬*/
		Rune_03 = 58, /*3번 룬*/
		Rune_04 = 59, /*4번 룬*/
		Rune_05 = 60, /*5번 룬*/
		Rune_06 = 61, /*6번 룬*/
		Bracelet = 62, /*팔찌*/
		LowSmeltScroll = 63, /*하급 무기 제련서*/
		MidSmeltScroll = 64, /*중급 무기 제련서*/
		HighSmeltScroll = 65, /*상급 무기 제련서*/
		ShopGacha = 66, /*샵 리스트 테이블 이용 장비 가차*/
		Teleport = 67, /*순간이동*/
		RuneSelectGacha = 68, /*룬 선택 가차*/
		Artifact = 69, /*아티팩트*/
		Buff = 70, /*버프*/
		FireGem = 71, /*화염의 젬*/
		SeaGem = 72, /*바다의 젬*/
		WindGem = 73, /*바람의 젬*/
		EarthGem = 74, /*대지의 젬*/
		TreeGem = 75, /*나무의 젬*/
		ChaosGem = 76, /*혼돈의 젬*/
		TempleEnter = 77, /*유적입장*/
		Ingredients = 78, /*요리재료*/
		Food = 79, /*요리*/
		NonDestroyArtifactMake = 80, /*아티팩트 파괴 방지 보호재*/
		VehicleGacha = 81, /*탈것 가차*/
		SeasonPass = 82, /*유료 출석부 패스*/
		OptionStone = 83, /*옵션석*/
	}

	public enum E_ItemSubType
	{
		Sword = 101, /*검*/
		Bow = 102, /*활*/
		Wand = 103, /*지팡이*/
		TwoSwords = 104, /*쌍검*/
		SideWeapon = 200, /*보조무기*/
		Meat = 301, /*육류*/
		Grain = 302, /*곡류*/
		Fruits = 303, /*과일*/
		Vegetable = 304, /*야채*/
		Fish = 305, /*물고기*/
		Spice = 306, /*향신료*/
		Herb = 307, /*허브(약초)*/
	}

	public enum E_TradeTapType
	{
		None = 0, /*없음*/
		Trade_Weapon = 1, /*무기*/
		Trade_SubWeapon = 2, /*보조무기*/
		Trade_DefenseEquip = 3, /*방어구*/
		Trade_Accesary = 4, /*액세서리*/
		Trade_SkillBook = 5, /*스킬북*/
		Trade_Material = 6, /*제작 재료*/
		Trade_Gem = 7, /*젬*/
		Trade_Consumables = 8, /*소모품*/
	}

	public enum E_TradeSubTapType
	{
		None = 0, /*없음*/
		Trade_Sub_Sword = 101, /*검*/
		Trade_Sub_Bow = 102, /*활*/
		Trade_Sub_Wand = 103, /*지팡이*/
		Trade_Sub_TwinSword = 104, /*쌍검*/
		Trade_Sub_KnightSubEquip = 201, /*방패*/
		Trade_Sub_ArcherSubEquip = 202, /*화살통*/
		Trade_Sub_WizardSubEquip = 203, /*수정구*/
		Trade_Sub_AssassinSubEquip = 204, /*단검*/
		Trade_Sub_Helmet = 301, /*투구*/
		Trade_Sub_Armor = 302, /*갑옷*/
		Trade_Sub_Pants = 303, /*바지*/
		Trade_Sub_Gloves = 304, /*장갑*/
		Trade_Sub_Shoes = 305, /*신발*/
		Trade_Sub_Cape = 401, /*망토*/
		Trade_Sub_Necklace = 402, /*목걸이*/
		Trade_Sub_Earring = 403, /*귀걸이*/
		Trade_Sub_Bracelet = 404, /*팔찌*/
		Trade_Sub_Ring = 405, /*반지*/
		Trade_Sub_SkillBook = 501, /*스킬북*/
		Trade_Sub_Material = 601, /*제작재료*/
		Trade_Sub_FireGem = 701, /*화염의 젬*/
		Trade_Sub_SeaGem = 702, /*바다의 젬*/
		Trade_Sub_WindGem = 703, /*바람의 젬*/
		Trade_Sub_EarthGem = 704, /*대지의 젬*/
		Trade_Sub_TreeGem = 705, /*나무의 젬*/
		Trade_Sub_ChaosGem = 706, /*혼돈의 젬*/
		Trade_Sub_Enchant = 801, /*강화 주문서*/
		Trade_Sub_Moveitem = 802, /*이동 주문서*/
		Trade_Sub_Food = 803, /*요리*/
		Trade_Sub_Etc = 899, /*기타*/
	}

	public enum E_UniqueType
	{
		Common = 0, /*일반 아이템*/
		Unique = 1, /*유니크 아이템*/
	}

	public enum E_ItemStackType
	{
		Not = 0, /*중첩이 안되는 아이템*/
		Stack = 1, /*중첩이 가능한 아이템*/
		AccountStack = 2, /*계정 공유 중첩이 가능한 아이템*/
	}

	public enum E_EquipSlotType
	{
		None = 0, /*등록 없음*/
		Weapon = 1, /*무기*/
		SideWeapon = 2, /*보조무기*/
		Helmet = 3, /*투구*/
		Armor = 4, /*갑옷*/
		Pants = 5, /*바지*/
		Shoes = 6, /*신발*/
		Gloves = 7, /*장갑*/
		Cape = 8, /*망토*/
		Necklace = 9, /*목걸이*/
		Ring = 10, /*반지*/
		Ring_2 = 11, /*반지2*/
		Ring_3 = 12, /*반지3*/
		Ring_4 = 13, /*반지4*/
		Earring = 14, /*귀걸이*/
		Earring_2 = 15, /*귀걸이2*/
		Bracelet = 16, /*팔찌*/
		Bracelet_2 = 17, /*팔찌2*/
		Rune_01 = 18, /*1번 룬*/
		Rune_02 = 19, /*2번 룬*/
		Rune_03 = 20, /*3번 룬*/
		Rune_04 = 21, /*4번 룬*/
		Rune_05 = 22, /*5번 룬*/
		Rune_06 = 23, /*6번 룬*/
		Artifact = 24, /*아티팩트*/
		Gem = 25, /*젬*/
	}

	public enum E_CharacterSelect
	{
		Not = 0, /*미출력*/
		Use = 1, /*출력*/
	}

	[System.Flags]
	public enum E_CharacterType
	{
		None = 0, /*없음*/
		Knight = 1, /*기사*/
		Archer = 2, /*궁사*/
		Wizard = 4, /*법사*/
		Assassin = 8, /*어쌔신*/
		All = 15, /*모두 포함*/
	}

	[System.Flags]
	public enum E_EnchantUseType
	{
		None = 0, /*없음*/
		NormalEnchant = 1, /*일반 강화 가능*/
		BlessEnchant = 2, /*축복 강화 가능*/
		CurseEnchant = 4, /*저주 강화 가능*/
		Upgrade = 8, /*승급 가능*/
		NonDestroy = 16, /*파괴 방지 강화 가능*/
	}

	public enum E_RuneGradeType
	{
		None = 0, /*없음*/
		Normal = 1, /*일반*/
		HighClass = 2, /*고급*/
		Rare = 3, /*희귀*/
		Legend = 4, /*전설*/
		Myth = 5, /*신화*/
	}

	public enum E_GetSupOptionType
	{
		None = 0, /*없음*/
		GetSupOption = 1, /*부옵션 생성 됨*/
		NotSupOption = 2, /*부옵션 생성 않됨*/
	}

	public enum E_InvenUseType
	{
		NotInven = 0, /*인벤토리 등록 않됨*/
		UseInven = 1, /*획득 시 인벤토리 등록*/
		RuneInven = 2, /*획득 시 룬 인벤토리 등록*/
		GemInven = 3, /*획득 시 젬 인벤토리 등록*/
	}

	public enum E_QuickSlotType
	{
		NotQuickSlot = 0, /*퀵 슬롯 등록 불가능*/
		UseQuickSlot = 1, /*퀵 슬롯 등록 가능*/
	}

	public enum E_QuickSlotAutoType
	{
		Not = 0, /*퀵 슬롯 등록 중 자동사용 기능 않됨*/
		Auto = 1, /*퀵 슬롯 등록 중 자동사용 됨*/
		AutoButtonOn = 2, /*퀵 슬롯 등록 중 Auto 버튼 활성화 시 자동사용 됨*/
	}

	public enum E_BelongType
	{
		None = 0, /*귀속 아님*/
		Belong = 1, /*획득 시 귀속*/
	}

	[System.Flags]
	public enum E_LimitType
	{
		None = 0, /*없음*/
		Store = 1, /*상점 판매*/
		Break = 2, /*분해 가능*/
		Exchange = 4, /*거래소 등록*/
		Storage = 8, /*창고 보관*/
		Delete = 16, /*삭제*/
		RuneStore = 32, /*룬 상점 판매*/
		Trade = 64, /*1:1 거래 가능 여부*/
		SkillBookBreak = 128, /*스킬북 분해*/
		Gem = 256, /*젬 장착 가능여부*/
		GemStore = 512, /*젬 판매*/
	}

	public enum E_GachaType
	{
		None = 0, /*없음*/
		Rate = 1, /*확률 획득(기존)*/
		Select = 2, /*선택 획득*/
		RuneSelect = 3, /*룬 선택 획득*/
	}

	public enum E_BuffStackType
	{
		Reset = 0, /*초기화*/
		TmePlus = 1, /*지속시간 중첩*/
	}

	[System.Flags]
	public enum E_InvokeTimingType
	{
		None = 0, /*없음*/
		BaseAttack = 1, /*기본공격 성공 시*/
		ShortHit = 2, /*근거리 공격 맞을때*/
		LongHit = 4, /*원거리 공격 맞을때*/
		MagictHit = 8, /*마법 공격 맞을때*/
		SpecialEvasion = 16, /*스페셜 회피 시(일반회피와 다르게 적용되는 물리공격에 대한 특수 회피)*/
		CantApplyAction = 32, /*현재 어빌리티 액션 발동확율 실패 시 링크 어빌리티 액션 발동*/
		ActiveSkill = 64, /*액티브 스킬 성공 시*/
		UseSkill = 128, /*특정 스킬 사용 시*/
		Mez = 256, /*상태이상mez 상태 시 (기절, 공포, 이동금지, 침묵)*/
		CriticalAttack = 512, /*기본공격 치명타 발생 시*/
		MezDelete = 1024, /*상태이상 제거 시*/
		UseCriticalAttack = 2048, /*해당 사용 스킬 크리티컬 발생 시*/
	}

	public enum E_BuffSupportType
	{
		None = 0, /*사용 않함*/
		GameTime = 1, /*게임 시간*/
		RealTime = 2, /*실 시간*/
	}

	public enum E_ReconnectionType
	{
		None = 0, /*유지 않함*/
		Support = 1, /*유지*/
		SkillBuff = 2, /*스킬 버프 유지*/
	}

	public enum E_DeathSupportType
	{
		None = 0, /*유지 않함*/
		Support = 1, /*유지*/
	}

	public enum E_ChangeSupportType
	{
		None = 0, /*유지 않함*/
		Support = 1, /*유지*/
	}

	public enum E_HaveSupportType
	{
		Lasting = 0, /*영구적 소유*/
		GetTime = 1, /*획득 시 지속시간 적용*/
		EquipTime = 2, /*착용 시 지속시간 적용*/
		EndTime = 3, /*특정 날짜에 아이템 삭제*/
	}

	public enum E_DropModelType
	{
		None = 0, /*없음*/
		Pack = 1, /*주머니*/
		Money = 2, /*골드*/
		Sword = 3, /*무기,보조무기*/
		Armor = 4, /*방어구*/
		Rune = 5, /*룬*/
		EnchantScroll = 6, /*주문서*/
		Case = 7, /*상자*/
	}

	public enum E_AutoStorageType
	{
		Not = 0, /*출력 않함*/
		Output = 1, /*출력*/
	}

	public enum E_RuneSetType
	{
		None = 0, /*없음*/
		Physical = 1, /*체력의 룬*/
		Battle = 2, /*격투의 룬*/
		Attack = 3, /*공격의 룬*/
		MagicAttack = 4, /*마법의 룬*/
		Defense = 5, /*방어의 룬*/
		Protect = 6, /*보호의 룬*/
		IronWall = 7, /*철벽의 룬*/
		Punitive = 8, /*징벌의 룬*/
		Spot = 9, /*명중의 룬*/
		Evasive = 10, /*회피의 룬*/
		FastPaced = 11, /*속공의 룬*/
		Swift = 12, /*신속의 룬*/
		Enemy = 13, /*적중의 룬*/
		Resistance = 14, /*저항의 룬*/
		Recovery = 15, /*회복의 룬*/
		Mana = 16, /*마나의 룬*/
		Max = 17, /*룬 세트 타입이 추가될 때마다 다음번호로 부여 필수(서버 처리용)*/
	}

	[System.Flags]
	public enum E_EnchantType
	{
		Enchant = 1, /*강화*/
		Upgrade = 2, /*승급*/
		NoUseItemEnchant = 4, /*재료 없이 강화*/
	}

	public enum E_DestroyType
	{
		NotDestroy = 0, /*파괴 않됨*/
		Destroy = 1, /*파괴*/
	}

	public enum E_TextType
	{
		Text = 0, /*텍스트*/
		Image = 1, /*이미지*/
	}

	public enum E_StageType
	{
		None = 0, /*없음*/
		Town = 1, /*마을*/
		Field = 2, /*필드*/
		Tower = 3, /*절망의 탑*/
		InterServer = 4, /*인터서버 보스지역*/
		GuildDungeon = 5, /*길드던전*/
		Event = 6, /*이벤트 지역*/
		Tutorial = 7, /*튜토리얼 지역*/
		Colosseum = 8, /*콜로세움*/
		AbyssTop = 9, /*심연의탑*/
		Instance = 10, /*인스턴스 던전*/
		Scenario = 11, /*시나리오 던전*/
		Infinity = 12, /*무한의 탑*/
		Raid = 13, /*레이드*/
		InterField = 14, /*인터필드*/
		Temple = 15, /*유적*/
		GodLand = 16, /*성지*/
	}

	public enum E_UnusedType
	{
		Use = 0, /*레코드 내용 사용*/
		Unuse = 1, /*미사용*/
	}

	public enum E_ViewType
	{
		View = 0, /*상점 내 탭 보임*/
		NotView = 1, /*상점 내 탭 안보임*/
		CheckView = 2, /*상점 내 조건 체크하여 노출*/
	}

	public enum E_MiniGoodsType
	{
		None = 0, /*없음*/
		Not = 1, /*미니게임 재화 아님*/
		MiniGoods = 2, /*미니게임 재화*/
	}

	public enum E_StageOpenType
	{
		Always = 0, /*항상*/
		Time = 1, /*매일 특정 시간*/
		GuildMaster = 2, /*길드장 오픈*/
		Period = 3, /*특정 기간(이벤트)*/
		Quest = 4, /*퀘스트 완료*/
		Item = 5, /*아이템 사용*/
		Gimmick = 6, /*기믹 완료*/
	}

	public enum E_StageEnterType
	{
		None = 0, /*없음*/
		Buff = 1, /*버프 적용 중*/
		Guild = 2, /*오픈 길드장과 동일 길드*/
		Item = 3, /*아이템 사용*/
	}

	public enum E_ChannelPrivate
	{
		NoPrivate = 0, /*전용채널없음*/
		Private = 1, /*전용채널있음*/
	}

	public enum E_ChannelChange
	{
		NoChange = 0, /*채널변경불가*/
		Change = 1, /*채널변경가능*/
	}

	public enum E_SummonBossType
	{
		None = 0, /*보스 소환이 없거나 미리 소환되어 있음*/
		SelfKillCount = 1, /*일반 몬스터 사냥 수(필드 지역 셀프 보스 소환)*/
		AllClear = 2, /*모든 몬스터 사냥 후*/
		DayTime = 3, /*매일 고정된 시간*/
		WeekTime = 4, /*매주 고정된 날짜와 시간*/
		Interaction = 5, /*상호작용*/
	}

	public enum E_StageClearType
	{
		None = 0, /*클리어 없음*/
		BossDie = 1, /*보스 사망*/
		MonsterDie = 2, /*모든 몬스터 사망*/
		EndTime = 3, /*제한시간 종료*/
		Interaction = 4, /*상호작용*/
	}

	public enum E_PKUseType
	{
		NotPK = 0, /*PK 불가능*/
		UsePK = 1, /*PK 가능*/
	}

	public enum E_TendencyDownType
	{
		Not = 0, /*선함 수치 감소 않함*/
		TendencyDown = 1, /*선함 수치 감소*/
	}

	public enum E_DeathPenaltyType
	{
		NotPenalty = 0, /*패널티 없음*/
		UsePenalty = 1, /*패널티 적용*/
	}

	public enum E_StageSaveType
	{
		NotSave = 0, /*위치 저장하지 않음*/
		Save = 1, /*위치 저장*/
	}

	public enum E_RoamingType
	{
		Not = 0, /*사용 않함*/
		Roaming = 1, /*로밍 사용*/
	}

	public enum E_PortalType
	{
		None = 0, /*없음*/
		LocalMap = 1, /*지역 지도창*/
		TowerMap = 2, /*절망의탑 지도창*/
		InterServerMap = 3, /*보스전 지도창*/
		EventMap = 4, /*이벤트 지도창*/
		GuildDungeon = 5, /*길드 던전창*/
		BossNPC = 6, /*보스전 NPC*/
		Quest = 7, /*퀘스트*/
		Colosseum = 8, /*콜로세움*/
		AbyssTop = 9, /*심연의탑*/
		Instance = 10, /*인스턴스 던전*/
		Infinity = 11, /*무한의 탑*/
		LocalMap_Boss = 12, /*지역 보스 포탈*/
		Scenario = 13, /*시나리오 던전*/
		Raid = 14, /*레이드*/
		Temple = 15, /*유적*/
	}

	public enum E_PKAreaChangeType
	{
		None = 0, /*없음*/
		NonPKArea = 1, /*캠프, 안전지대*/
		PKArea = 2, /*사냥, 전쟁 지역*/
	}

	public enum E_DropConditionType
	{
		None = 0, /*조건 없음*/
		UniqueBuff = 1, /*신의축복이나 신의권능 적용 중*/
	}

	public enum E_DropItemType
	{
		None = 0, /*구분 없음*/
		Gold = 1, /*골드*/
		Item = 2, /*아이템*/
		DropGroup = 3, /*드랍그룹*/
	}

	public enum E_GodBuffType
	{
		GodBless = 0, /*신의 축복*/
		GodPower = 1, /*신의 권능*/
	}

	public enum E_MarkAbleType
	{
		None = 0, /*없음*/
		RecoveryMark = 1, /*회복의 문양*/
		AttackMark = 2, /*공격의 문양*/
		DefenseMark = 3, /*방어의 문양*/
	}

	public enum E_MarkEnchantType
	{
		Not = 0, /*강화 못함*/
		Enchant = 1, /*강화 가능*/
	}

	public enum E_MarkUniqueType
	{
		Normal = 0, /*일반 문양*/
		Unique = 1, /*특수 능력 문양*/
	}

	public enum E_DiceUseType
	{
		Not = 0, /*주사위 연출 사용 않함*/
		Use = 1, /*주사위 연출 사용함*/
	}

	public enum E_CollectionType
	{
		HandRegister = 0, /*수동 등록*/
		GetRegister = 1, /*획득 시 자동 등록*/
	}

	public enum E_TapType
	{
		Tier_01 = 0, /*1티어*/
		Tier_02 = 1, /*2티어*/
		Tier_03 = 2, /*3티어*/
		Tier_04 = 3, /*4티어*/
		Tier_05 = 4, /*5티어*/
		Tier_06 = 5, /*6티어*/
		Event = 6, /*이벤트*/
		All = 7, /*전체*/
	}

	public enum E_MakeType
	{
		Weapon = 0, /*무기*/
		DefenseEquip = 1, /*방어구*/
		Accessory = 2, /*장신구*/
		Etc = 3, /*레오닉의 분노*/
		SkillBook = 4, /*스킬북*/
		Event = 5, /*이벤트*/
		HonorCoin = 6, /*사용않함*/
		Honor = 7, /*명예훈장*/
		Gem = 8, /*젬*/
	}

	public enum E_MakeTapType
	{
		Weapon_Knight = 1, /*기사 무기 서브 탭*/
		Weapon_Archer = 2, /*궁수 무기 서브 탭*/
		Weapon_Wizard = 3, /*법사 무기 서브 탭*/
		Weapon_Assassin = 4, /*암살자 무기 서브 탭*/
		SideWeapon_Knight = 5, /*기사 보조무기 서브 탭*/
		SideWeapon_Archer = 6, /*궁수 보조무기 서브 탭*/
		SideWeapon_Wizard = 7, /*법사 보조무기 서브 탭*/
		SideWeapon_Assassin = 8, /*암살자 보조무기 서브 탭*/
		Make_Helmet = 9, /*투구 제작 서브 탭*/
		Make_Armor = 10, /*갑옷 제작 서브 탭*/
		Make_Pants = 11, /*바지 제작 서브 탭*/
		Make_Gloves = 12, /*장갑 제작 서브 탭*/
		Make_Shoes = 13, /*신발 제작 서브 탭*/
		Make_Ring = 16, /*반지 제작 서브 탭*/
		Make_Earring = 17, /*귀걸이 제작 서브 탭*/
		Make_Belt = 18, /*벨트 제작 서브 탭*/
		Make_Tshirt = 19, /*티셔츠 제작 서브 탭*/
		Make_Ticket = 20, /*충전권*/
		Make_UseItem = 21, /*소모품*/
		Make_Change_Card = 22, /*변신 카드 제작 서브 탭*/
		Make_Pet_Card = 23, /*펫 카드 제작 서브 탭*/
		Make_Material = 24, /*재료 제작 서브 탭*/
		T6_Skill_Book = 25, /*6티어 스킬북 서브 탭*/
		T5_Skill_Book = 26, /*5티어 스킬북 서브 탭*/
		T4_Skill_Book = 27, /*4티어 스킬북 서브 탭*/
		Make_Upgrad = 28, /*승급석 제작*/
		Make_Enchant = 29, /*강화석 제작*/
		Make_Equip = 30, /*장비 제작*/
		T3_Skill_Book = 31, /*3티어 스킬북 서브 탭*/
		T2_Skill_Book = 32, /*2티어 스킬북 서브 탭*/
		T1_Skill_Book = 33, /*1티어 스킬북 서브 탭*/
		Make_Skill_Book = 34, /*스킬북 서브 탭*/
		Make_Box = 35, /*상자*/
		Make_Reonick = 36, /*레오닉의 분노 탭*/
		Make_Gem_1Tier = 37, /*젬 1티어*/
		Make_Gem_2Tier = 38, /*젬 2티어*/
		Make_Gem_3Tier = 39, /*젬 3티어*/
		Make_Gem_4Tier = 40, /*젬 4티어*/
		Make_Gem_5Tier = 41, /*젬 5티어*/
		Make_Gem_6Tier = 42, /*젬 6티어*/
	}

	public enum E_MakeLimitType
	{
		Infinite = 0, /*무제한 제작 가능*/
		OneShot = 1, /*초기화 없는 제한 제작 - 제작 시도 시 카운트 증가*/
		Day = 2, /*매일 새벽 5시 초기화 제한 제작*/
		Week = 3, /*주 단위 새벽 5시 초기화 제작*/
		Monthly = 4, /*메월 1일 새벽 5시 초기화 제한 제작*/
		TryOneShot = 5, /*초기화 없는 제한 제작 - 제작 성공 시 카운트 증가*/
		ServerLimit = 6, /*서버 한정 제작*/
	}

	public enum E_Materialtype
	{
		None = 0, /*일반*/
		Change = 1, /*대체 아이템 타입*/
		Stack = 2, /*재료가 합해져서 사용되는 타입*/
	}

	public enum E_QuestType
	{
		None = 0, /*없음*/
		Main = 1, /*메인 퀘스트*/
		Sub = 2, /*업적*/
		Daily = 3, /*일일 퀘스트*/
		Weekly = 4, /*주간 퀘스트*/
		Achievement = 5, /*업적*/
		Temple = 6, /*유적 퀘스트*/
	}

	public enum E_UIQuestType
	{
		None = 0, /*없음*/
		Main = 1, /*메인 퀘스트*/
		Sub = 2, /*업적*/
		Daily = 3, /*일일 퀘스트*/
		Weekly = 4, /*주간 퀘스트*/
		Achievement = 5, /*업적*/
		Temple = 6, /*유적 퀘스트*/
	}

	public enum E_QuestGroupRewardOn
	{
		None = 0, /*사용안함*/
		On = 1, /*보상 처음퀘스트 것만 지급*/
	}

	[System.Flags]
	public enum E_QuestOpenType
	{
		None = 0, /*없음*/
		CreateCharacter = 1, /*캐릭터 생성 후 최초 접속 시*/
		Auto = 2, /*자동 수락*/
		Journal = 3, /*저널 선택 수락*/
		NPC = 4, /*NPC 통해 수락*/
		Item = 5, /*ITEM 사용 통해 수락*/
		Trigger = 6, /*클라이언트 트리거 진입 시*/
	}

	public enum E_DailyMissionType
	{
		ETC0 = 0, /*임시1*/
		ETC1 = 1, /*임시2*/
		ETC2 = 2, /*임시3*/
		ETC3 = 3, /*임시4*/
		ETC4 = 4, /*임시5*/
		ETC5 = 5, /*임시6*/
		ETC6 = 6, /*임시7*/
		ETC7 = 7, /*임시8*/
		ETC8 = 8, /*임시9*/
		ETC9 = 9, /*임시10*/
		ETC10 = 10, /*임시11*/
		ETC11 = 11, /*임시12*/
		ETC12 = 12, /*임시13*/
		ETC13 = 13, /*임시14*/
		ETC14 = 14, /*임시15*/
		ETC15 = 15, /*임시16*/
		ETC16 = 16, /*임시17*/
		ETC17 = 17, /*임시18*/
		ETC18 = 18, /*임시19*/
		ETC19 = 19, /*임시20*/
	}

	public enum E_DailyMissionLevel
	{
		Easy = 0, /*쉬움*/
		Normal = 1, /*보통*/
		Hard = 2, /*어려움*/
	}

	public enum E_TaskQuestType
	{
		None = 0, /*태스크퀘스트 사용안함*/
		TaskQuest = 1, /*태스크퀘스트 사용*/
	}

	public enum E_CompleteCheck
	{
		None = 0, /*없음*/
		MonsterKill = 1, /*특정 몬스터 사냥 시*/
		GetItem = 2, /*특정 아이템 확률 카운팅*/
		DeliveryItem = 3, /*특정 아이템 전달 시*/
		Level = 4, /*특정 레벨 도달 시*/
		MapMove = 5, /*특정 맵 이동 시*/
		MapPos = 6, /*특정 맵, 특정 좌표 이동 시*/
		NPCTalk = 7, /*특정 npc 대화 시*/
		GetObject = 8, /*특정 오브젝트 인터렉션 시*/
		EquipEnchant = 9, /*장비 강화*/
		EquipUpgrade = 10, /*장비 승급*/
		ChangeBuy = 11, /*변신 구입*/
		ChangeCompose = 12, /*변신 합성*/
		PetBuy = 13, /*펫 구입*/
		PetCompose = 14, /*펫 합성*/
		RideBuy = 15, /*탈 것 구입*/
		RideCompose = 16, /*탈 것 합성*/
		Tutorial = 17, /*연결 튜토리얼 완료 시*/
		LevelPer = 18, /*업적 레벨 달성 경험치의 %*/
		EnterTemple = 19, /*입장 유적*/
		ClearTemple = 20, /*완료 유적*/
	}

	public enum E_DeliveryItemType
	{
		None = 0, /*아이템 회수 안함*/
		Collect = 1, /*아이템 회수*/
	}

	public enum E_InteractionObjType
	{
		None = 0, /*없음*/
		Gather = 1, /*채집*/
		Collect = 2, /*수집*/
		Operation = 3, /*작동*/
		Destruction = 4, /*파괴*/
		Build = 5, /*설치*/
		Survey = 6, /*조사*/
		Extract = 7, /*추출*/
		Repair = 8, /*수리*/
		Research = 9, /*탐색*/
		Activation = 10, /*활성화*/
		Deactivation = 11, /*비활성화*/
	}

	public enum E_InteractionGetItem
	{
		None = 0, /*아이템 생성 안함*/
		Drop = 1, /*아이템 생성*/
	}

	public enum E_ItemAutoInven
	{
		None = 0, /*아이템 자동인벤습득 안함*/
		Auto = 1, /*아이템 자동인벤 습득*/
	}

	public enum E_UIChapterType
	{
		None = 0, /*챕터 연출 없음*/
		Start = 1, /*챕터 시작 연출*/
		End = 2, /*챕터 종료 연출*/
	}

	public enum E_AutoProgressType
	{
		None = 0, /*사용안함*/
		Auto = 1, /*자동진행 사용*/
	}

	public enum E_AutoNoneInfoType
	{
		None = 0, /*사용안함*/
		ShowSystemMsg = 1, /*시스템메세지 출력*/
		ShowMsgPopup = 2, /*팝업알림창*/
	}

	public enum E_AutoCompleteType
	{
		None = 0, /*사용안함*/
		AutoComplete = 1, /*자동완료*/
	}

	public enum E_UICompleteHideType
	{
		None = 0, /*사용안함*/
		Hide = 1, /*숨김*/
	}

	public enum E_VideoType
	{
		None = 0, /*사용안함*/
		Sequence = 1, /*시퀀스 재생*/
		Movie = 2, /*영상 재생*/
	}

	public enum E_RewardExpType
	{
		None = 0, /*절대값 사용*/
		Percent = 1, /*퍼센트 사용*/
	}

	public enum E_QuestGradeType
	{
		Easy = 0, /*쉬움*/
		Normal = 1, /*보통*/
		Hard = 2, /*어려움*/
		Complete = 3, /*완료*/
	}

	public enum E_DailyQuestType
	{
		MonsterKill = 0, /*몬스터 사냥*/
		BossMonsterKill = 1, /*보스 몬스터 사냥*/
		FixMonsterKill = 2, /*특정 몬스터 사냥*/
		MakeCount = 3, /*제작 시도*/
		FixMakeCount = 4, /*특정 아이템 제작*/
		FixItemUse = 5, /*특정 아이템 사용*/
		DropGoldCount = 6, /*사냥 시 골드 획득 량*/
		BreakEssence = 7, /*장비분해 시 정수 획득 량*/
		ChangeCount = 8, /*변신 횟수*/
		PetSummonCount = 9, /*펫 소환 수*/
		PetMix = 10, /*펫 합성 시도 수*/
		ChangeMix = 11, /*변신 합성 시도 수*/
		FixTierPetMix = 12, /*특정 티어 펫 합성 시도 수*/
		FixTierChangeMix = 13, /*특정 티어 변신 합성 시도 수*/
		FixTierBreakEquip = 14, /*특정 티어 장비 분해*/
		EquipEnchant = 15, /*장비 강화 시도 수*/
		AccessoryEnchant = 16, /*엑세서리 강화 시도 수*/
		PetGacha = 17, /*펫 뽑기 시도 수*/
		ChangeGacha = 18, /*변신 뽑기 시도 수*/
		Complete3 = 19, /*일일 미션 3개 완료*/
		Complete6 = 20, /*일일 미션 6개 완료*/
		Complete9 = 21, /*일일 미션 9개 완료*/
		InstanceDungeonClear = 22, /*시련의장 클리어*/
		RuneEnchant = 23, /*룬 강화*/
		JoinColosseum = 24, /*콜로세움 참여*/
		UseEnchantScroll = 25, /*재련 진행*/
		InstanceDungeonClearCnt = 26, /*시련의장 클리어 횟수*/
		QuestEventPoint01 = 27, /*퀘스트이벤트 포인트 달성 시 보상 획득*/
		QuestEventPoint02 = 28, /*퀘스트이벤트 포인트 달성 시 보상 획득*/
		QuestEventPoint03 = 29, /*퀘스트이벤트 포인트 달성 시 보상 획득*/
		QuestEventPoint04 = 30, /*퀘스트이벤트 포인트 달성 시 보상 획득*/
		QuestEventPoint05 = 31, /*퀘스트이벤트 포인트 달성 시 보상 획득*/
		NormalShop = 32, /*노말샵 상품 구매*/
		SpecialShop = 33, /*스페셜샵 상품 구매*/
		QuestEventPoint06 = 34, /*퀘스트이벤트 포인트 달성 시 보상 획득*/
	}

	public enum E_UIShortCut
	{
		None = 0, /**/
		EnchantUI = 1, /*장비 강화 창*/
		BossGaugeUI = 2, /*보스 소환 게이지 창*/
		InstanceDungeonUI = 3, /*인스턴드 던전 창*/
		RuneEnchantUI = 4, /*룬 강화 창*/
		NormalShopUI = 5, /*노말샵 창*/
		SpecialShopUI = 6, /*스페셜샵 창*/
		Warp = 7, /*텔레포트*/
	}

	public enum E_AttendEventType
	{
		Attend = 0, /*출석 이벤트*/
		Continuity = 1, /*연속 출석 이벤트*/
		AddAttend = 2, /*출석 이벤트 종료 시 반복 발생*/
		NewAttend = 3, /*신규 출석 이벤트*/
		ReturnAttend = 4, /*복귀 출석 이벤트*/
		Cumulative = 5, /*누적 출석 이벤트*/
	}

	public enum E_AttendEventOpenType
	{
		EventOpen = 0, /*이벤트 발동 시 오픈*/
		EventComplete = 1, /*선행 이벤트 완료 시 오픈*/
	}

	public enum E_AttendEventFinishType
	{
		End = 0, /*완료 후 종류*/
		ReOpen = 1, /*완료 후 재오픈(반복)*/
	}

	public enum E_AttendBoardType
	{
		OneWeek = 0, /*7일형*/
		FourWeek = 1, /*28일형*/
	}

	public enum E_NormalShopType
	{
		None = 0, /*없음*/
		General = 1, /*정수 상점*/
		Colosseum_01 = 2, /*콜로세움(하)*/
		Colosseum_02 = 3, /*콜로세움(중)*/
		Colosseum_03 = 4, /*콜로세움(상)*/
		SkillBook = 5, /*스킬북 상점*/
	}

	public enum E_ShopType
	{
		None = 0, /*없음*/
		Normal = 1, /*잡화점*/
		Colosseum = 2, /*콜로세움 상점*/
	}

	public enum E_CashType
	{
		None = 0, /*없음*/
		Cash = 1, /*캐쉬*/
	}

	public enum E_MileageShopType
	{
		None = 0, /*없음*/
		ChangeGachaMileage = 1, /*변신 가차 마일리지*/
		PetGachaMileage = 2, /*펫 가차 마일리지*/
		ChangeMileage = 3, /*변신 마일리지*/
		PetMileage = 4, /*펫 마일리지*/
		AccessoryMileage = 5, /*강화 마일리지*/
		MakeMileage = 6, /*제작 마일리지*/
		EquipMileage = 7, /*장비 가차 마일리지*/
		GemMileage = 8, /*젬 마일리지*/
	}

	public enum E_SubTapType
	{
		None = 0, /*없음*/
		RingMileage = 1, /*반지 마일리지*/
		EarringMileage = 2, /*귀걸이 마일리지*/
		BeltMileage = 3, /*팔찌 마일리지*/
		TshirtMileage = 4, /*티셔츠 마일리지*/
		ChangeMileage = 5, /*변신 마일리지*/
		PetMileage = 6, /*펫 마일리지*/
		MakeMileage = 7, /*제작 마일리지*/
		Etc = 8, /*기타*/
		SkillBook = 9, /*스킬북*/
		NonDistroy = 10, /*수호석*/
		Upgrade = 11, /*승급석*/
		WeaponMileage = 12, /*무기*/
		SideWeaponMileage = 13, /*보조무기*/
		HelmetMileage = 14, /*투구*/
		ArmorMileage = 15, /*갑옷*/
		PantsMileage = 16, /*바지*/
		ShoesMileage = 17, /*신발*/
		GlovesMileage = 18, /*장갑*/
		GemMileage = 19, /**/
	}

	public enum E_SpecialShopType
	{
		None = 0, /*없음*/
		Monthly = 1, /*월간 상점*/
		Package = 2, /*패키지 상점*/
		ChangePet = 3, /*변신/펫 상점*/
		Goods = 4, /*재화*/
		Accessory = 5, /*반지/귀걸이 상점*/
		Accessory2 = 6, /*티셔츠/벨트 상점*/
		Normal = 7, /*일반 상점*/
		Essence = 8, /*정수 상점*/
		Tutorial = 9, /*튜토리얼 사용*/
		Colosseum = 10, /*콜로세움 상점*/
		MiniGame = 11, /*미니게임 상점*/
		Equip = 12, /*장비 상점*/
		Event = 13, /*이벤트 상점*/
		CandyEvent = 14, /*달달한 막대사탕 샵*/
		Rune = 15, /*룬 상점*/
		Scenario = 16, /*시간의방 상점*/
		Gem = 17, /*젬 상점*/
		BlackMarket = 18, /*암시장*/
		CollectEvent = 19, /*수집 이벤트*/
	}

	public enum E_SpecialSubTapType
	{
		None = 0, /*없음*/
		tier_1 = 1, /*1티어*/
		tier_2 = 2, /*2티어*/
		tier_3 = 3, /*3티어*/
		tier_4 = 4, /*4티어*/
		Colosseum_Small = 5, /*콜로세움(소)*/
		Colosseum_Medium = 6, /*콜로세움(중)*/
		Colosseum_Large = 7, /*콜로세움(대)*/
		Scenario_Shop1 = 8, /*시나리오*/
		All = 9, /*전체*/
	}

	public enum E_BuyBonusType
	{
		Not = 0, /*보너스 지급 않함*/
		Use = 1, /*보너스 지급*/
	}

	[System.Flags]
	public enum E_BuyKindType
	{
		Normal = 1, /*일반 상품*/
		GoodsList = 2, /*상품 리스트 연결 구입 타입*/
	}

	public enum E_GoodsListGetType
	{
		None = 0, /*없음*/
		All = 1, /*리스트 모든 아이템 획득*/
		Select = 2, /*리스트 중 한 개 선택*/
		Rate = 3, /*리스트 중 확률로 한 개 획득*/
	}

	[System.Flags]
	public enum E_StateType
	{
		Normal = 0, /*일반 상품*/
		Sale = 1, /*세일 상품*/
		Popularity = 2, /*인기 상품*/
		Event = 4, /*이벤트 상품*/
		Package = 8, /*패키지 상품*/
		SeasonPass = 9, /*시즌 패스*/
	}

	public enum E_GoodsKindType
	{
		None = 0, /*없음*/
		Item = 1, /*아이템*/
		Change = 2, /*변신체*/
		Pet = 3, /*펫*/
	}

	public enum E_PriceType
	{
		Normal = 0, /*고정 가격*/
		Number = 1, /*매일 구입 횟수*/
	}

	public enum E_FlatProductType
	{
		Normal = 0, /*없음*/
		Period = 1, /*제한된 기간*/
	}

	public enum E_ServerType
	{
		None = 0, /*사용 않함*/
		Server = 1, /*단일 서버*/
		Global = 2, /*글로벌 서버*/
	}

	public enum E_BuyLimitType
	{
		Infinite = 0, /*무제한 구입 가능*/
		OneShot = 1, /*초기화 없는 제한 구입*/
		Day = 2, /*매일 새벽 5시 초기화 제한 구입*/
		Week = 3, /*주 단위 새벽 5시 초기화*/
		Monthly = 4, /*매월 1일 새벽 5시 초기화 제한 구입*/
	}

	public enum E_CharStateType
	{
		Active = 0, /*활성 상태*/
		DeleteWait = 1, /*삭제 대기 상태*/
		Delete = 2, /*삭제 상태*/
	}

	public enum E_StateOutputType
	{
		Not = 0, /*출력 안함*/
		Output = 1, /*출력*/
	}

	public enum E_TapOutputType
	{
		None = 0, /*없음*/
		Shop = 1, /*샵*/
		Mileage = 2, /*마일리지*/
	}

	public enum E_SizeType
	{
		Small = 0, /*스몰*/
		Big = 1, /*빅*/
	}

	public enum E_AbilityActionType
	{
		Buff = 0, /*사용 시 일정시간*/
		Equip = 1, /*장착 시*/
		Passive = 2, /*적용/획득 시 계속 적용*/
		OneShot = 3, /*일회성*/
		Mez = 4, /*행동제어*/
		Change = 5, /*변신*/
		Pet = 6, /*펫 사용*/
		Destiny = 7, /*인연효과*/
		Damage = 8, /*데미지*/
		Heal = 9, /*회복*/
	}

	public enum E_BuffType
	{
		None = 0, /*없음*/
		Buff = 1, /*버프*/
		DeBuff = 2, /*디버프*/
	}

	public enum E_MagicSignType
	{
		Not = 0, /*출력 않함*/
		Sign = 1, /*출력*/
	}

	public enum E_HudBuffSignType
	{
		Not = 0, /*출력 않함*/
		Sign = 1, /*출력*/
	}

	public enum E_AbilityViewType
	{
		Not = 0, /*기존 룰에 의해 능력 출력*/
		ToolTip = 1, /*능력치 출력 없이 툴팁 내용 출력*/
	}

	public enum E_AbilityTargetType
	{
		None = 0, /*없음*/
		Not = 1, /*적용 제한 없음*/
		Skill = 2, /*특정 스킬에만 적용*/
	}

	public enum E_BuffTimeSignType
	{
		Not = 0, /*출력 않함*/
		Sign = 1, /*출력*/
	}

	public enum E_MailType
	{
		OperatorNormal = 0, /*운영자 일반 (계정용)*/
		OperatorCharacter = 1, /*운영자 캐릭터용*/
		Attend = 2, /*출석 이벤트*/
		ContinueAttend = 3, /*연속 출석 이벤트*/
		GuildAttend = 4, /*길드 출석 이벤트*/
		GuildAttendBonus = 5, /*길드 출석 보너스 이벤트*/
		Coupon = 6, /*쿠폰*/
		PKExpRestoreBonus = 7, /*PK 사망EXP패널티 회복시 보상*/
		PeriodGoods = 8, /*정기 상품 우편*/
		GuildDungeonSuccess = 9, /*길드던전 성공 보상*/
		GuildDungeonFail = 10, /*길드던전 실패 보상*/
		PushEvent = 11, /*푸시이벤트 보상*/
		PeriodGoods1 = 12, /*1일차 정기 상품 우편*/
		PeriodGoods2 = 13, /*2일차 정기 상품 우편*/
		PeriodGoods3 = 14, /*3일차 정기 상품 우편*/
		PeriodGoods4 = 15, /*4일차 정기 상품 우편*/
		PeriodGoods5 = 16, /*5일차 정기 상품 우편*/
		PeriodGoods6 = 17, /*6일차 정기 상품 우편*/
		PeriodGoods7 = 18, /*7일차 정기 상품 우편*/
		PeriodGoods8 = 19, /*8일차 정기 상품 우편*/
		PeriodGoods9 = 20, /*9일차 정기 상품 우편*/
		PeriodGoods10 = 21, /*10일차 정기 상품 우편*/
		PeriodGoods11 = 22, /*11일차 정기 상품 우편*/
		PeriodGoods12 = 23, /*12일차 정기 상품 우편*/
		PeriodGoods13 = 24, /*13일차 정기 상품 우편*/
		PeriodGoods14 = 25, /*14일차 정기 상품 우편*/
		PeriodGoods15 = 26, /*15일차 정기 상품 우편*/
		PeriodGoods16 = 27, /*16일차 정기 상품 우편*/
		PeriodGoods17 = 28, /*17일차 정기 상품 우편*/
		PeriodGoods18 = 29, /*18일차 정기 상품 우편*/
		PeriodGoods19 = 30, /*19일차 정기 상품 우편*/
		PeriodGoods20 = 31, /*20일차 정기 상품 우편*/
		PeriodGoods21 = 32, /*21일차 정기 상품 우편*/
		PeriodGoods22 = 33, /*22일차 정기 상품 우편*/
		PeriodGoods23 = 34, /*23일차 정기 상품 우편*/
		PeriodGoods24 = 35, /*24일차 정기 상품 우편*/
		PeriodGoods25 = 36, /*25일차 정기 상품 우편*/
		PeriodGoods26 = 37, /*26일차 정기 상품 우편*/
		PeriodGoods27 = 38, /*27일차 정기 상품 우편*/
		PeriodGoods28 = 39, /*28일차 정기 상품 우편*/
		PeriodGoods29 = 40, /*29일차 정기 상품 우편*/
		PeriodGoods30 = 41, /*30일차 정기 상품 우편*/
		BosswarAttend = 42, /*보스전 참여 보상*/
		PeriodGoods30_End = 43, /*30일차 정기 상품 특별 우편*/
		EventReward01 = 101, /*이벤트 참여 보상*/
		RestorationCoupon = 102, /*복구 쿠폰*/
		FieldBossGuildReward = 103, /*필드 보스 길드 보상*/
		RaidClearReward = 104, /*레이드 클리어 보상*/
		RaidLanternReward = 105, /*레이드 랜턴방 보상*/
		ColoGuildReward = 106, /*투기장 길드 보상*/
		AttendEventSeason = 107, /*유료출석 시즌패스*/
		AttendEventSeason2 = 108, /*유료출석 시즌패스2*/
		Cumulative = 109, /*누적 출석 보상*/
	}

	public enum E_MailReceiver
	{
		Character = 0, /*캐릭터용 메일*/
		Account = 1, /*계정용 메일*/
	}

	public enum E_TendencyIconType
	{
		Icon_Ability_ValuesNeutral = 0, /*중도*/
		Icon_Ability_ValuesGood = 1, /*선*/
		Icon_Ability_ValuesEvil = 2, /*악*/
	}

	[System.Flags]
	public enum E_GuildBuffOpenType
	{
		Master = 1, /*길드장*/
		SubMaster = 2, /*부길드장*/
		Member = 4, /*길드원*/
	}

	public enum E_RankBuffType
	{
		PK = 0, /*PK 랭킹*/
		Exp = 1, /*경험치 랭킹*/
		ExpClassType = 2, /*직업별 랭킹*/
	}

	public enum E_SkillUseType
	{
		Not = 0, /*사용 않함*/
		Use = 1, /*사용*/
	}

	public enum E_BotUseType
	{
		Not = 0, /*사용 않함*/
		Use = 1, /*사용*/
	}

	public enum E_MiniItemType
	{
		Equip = 0, /*장비아이템*/
		Enchant = 1, /*강화재료*/
		Upgrade = 2, /*승급재료*/
	}

	public enum E_LoadingType
	{
		Not = 0, /*사용 않함*/
		Intro = 1, /*인트로*/
		Level = 2, /*캐릭터 레벨*/
		DeathPenalty = 3, /*사망패널티*/
		Event = 4, /*이벤트*/
	}

	public enum E_SoundType
	{
		Normal = 0, /*일반,기타*/
		UI = 1, /*UI 사운드*/
		Effect = 2, /*이펙트 사운드*/
		BGM = 3, /*지역 사운드*/
		Alarm = 4, /*경고 사운드*/
	}

	public enum E_TutorialType
	{
		None = 0, /**/
		Dialogue = 1, /*다이알로그*/
		Guide = 2, /*가이드, 기능*/
		AutoDialogue = 3, /*시간에 따른 다이알로그 출력용*/
	}

	public enum E_TutorialDialogueType
	{
		None = 0, /**/
		Image = 1, /*이미지 출력 타입*/
		Object = 2, /*오브젝트 출력 타입*/
	}

	public enum E_GuideType
	{
		None = 0, /*기본 GuideType*/
		TouchTopMenu = 1, /*Hud Menu에서 해당 버튼을 터치한다.*/
		TouchMenuButton = 2, /*Menu에서 해당 버튼을 클릭한다.*/
		TouchInventoryItem = 3, /*Inventory에서 해당 itemTid가 있다면 터치한다.*/
		TouchQuickSlot = 4, /*해당 index의 QuickSlot을 터치한다.*/
		QuickSlotAutoUse = 5, /*해당 index의 QuickSlot에서 Auto 를 활성화 한다.*/
		TouchJoystick = 6, /*Joystick을 터치한다.*/
		TouchAttack = 7, /*Attack을 터치한다.*/
		TouchCamera = 8, /*Camera을 터치한다.*/
		TouchCookMake = 9, /*요리 제작을 터치한다.*/
		TouchCookMaterial = 10, /*요리 ui에서 해당 재료들을 선택한다*/
		ItemInfoPopup = 11, /*아이템 정보창에서 사용 버튼을 터치한다.*/
		TouchClassSelect = 12, /*클래스 ui에서 classTid에 해당하는 class를 선택한다.*/
		TouchClassChange = 13, /*클래스 ui에서 장착을 터치한다.*/
		TouchRideSelect = 14, /*탈것 ui에서 RidTid에 해당하는 탈것을 선택한다.*/
		TouchRideRegist = 15, /*탈것 ui에서 장착을 터치한다.*/
		TouchRideButton = 16, /*hud에서 탈것 타기 버튼을 터치한다.(쿨타임이면 기다린다.)*/
		TouchWorldMap = 17, /*hud에서 월드맵을 터치한다.*/
		TouchWorldMapStage = 18, /*월드맵 ui에서 stageTid에 해당하는 탭을 선택한다.*/
		TouchWorldMapStagePortal = 19, /*월드맵 ui에서 portalTid해당하는 맵 위치를 선택한다.*/
		TouchWoldMapTeleport = 20, /*월드맵 ui에서 텔레포트 버튼을 터치한다.*/
		TouchMessagePopupOk = 21, /*메시지 팝업에서 ok 버튼을 터치한다.*/
		TouchQuestHudTeleport = 22, /*quest hud 에서 텔레포트를 터치한다.*/
		TouchSpecialShopTabType = 23, /*스페셜 상점 ui에서 해당 탭 타입을 선택한다.*/
		TouchSpecialShopItem = 24, /*스페셜 상점 ui에서 해당 아이템을 선택한다.*/
		TouchSpecialShopWarn = 25, /*스페셜 상점 ui에서 청약철회 버튼을 터치한다.*/
		ScrollSpecialShopWarn = 26, /*스페셜 상점 ui에서 청약철회 ui를 스크롤한다.*/
		TouchSpecialShopWarnClose = 27, /*스페셜 상점 ui에서 청약처회 닫기 버튼을 터치한다.*/
		TouchSpecialShopBuy = 28, /*스페셜 상점 ui에서 구매버튼을 터치한다.*/
		TouchPetSelect = 29, /*펠로우 ui에서 해당 펠로우를 선택한다.*/
		TouchPetSummon = 30, /*펠로우 ui에서 펠로우를 소환한다.*/
		ItemInfoEquipItem = 31, /*iteminfo 에서 장착 버튼을 터치한다.*/
		OpenTutorialTemple = 32, /*해당 튜토리얼 유적을 오픈한다.*/
		MoveTo = 33, /*해당 위치로 이동한다.*/
		PlayCutScene = 34, /*해당 컷씬을 플레이한다.*/
		TouchClosePanel = 35, /*이전 튜토리얼 스탭에서 열렸던 패널 닫기*/
		TouchCookMakeCloseResult = 36, /*요리 결과창 닫기*/
		TouchQuestHud = 37, /*quest hud 에서 알림 영역을 터치한다.*/
		TouchInteractionGimmick = 38, /*필드에 배치된 특정 기믹 상호작용 버튼 터치한다.*/
		TouchCookRecipe = 39, /*요리 ui에서 레시피 탭을 터치한다*/
		TouchCookRecipePotion = 40, /*요리 ui 레시피 페이지에서 포션을 터치한다*/
		TouchCookRecipeMax = 41, /*요리 ui 레시피 페이지에서 MAX 버튼을 터치한다*/
		TouchCookRecipeMake = 42, /*요리 ui 레시피 페이지에서 만들기 버튼을 터치한다*/
		TouchInventoryItemByCharacterType = 43, /*인벤토리에서 캐릭터 타입따른 아이템을 선택한다.*/
		TouchPotionHud = 44, /*HUD 포션 버튼 터치한다*/
		TouchPotionAuto = 45, /*일반 포션 우선순위 버튼을 터치한다*/
		TutorialClearTemple = 46, /*유적 클리어 체크*/
		TouchClassByCharacterID = 47, /*클래스 ui에서 캐릭터tid를 참조하여 classTid에 해당하는 class를 선택한다.*/
		TempleFixedClass = 48, /*유적에서 특정 클래스로 플레이강제 (유적 탈출 시 초기화)*/
		TouchInventoryItemByCharacterID = 49, /*캐릭터 클래스, 속성 아이템을 찾아 터치한다.*/
		HideCharacter = 50, /*모든 캐릭터를 숨긴다*/
		HideMonster = 51, /*모든 몬스터를 숨긴다*/
		HideNpc = 52, /*모든 npc를 숨긴다*/
		ShowCharacter = 53, /*모든 캐릭터를 보이게 한다*/
		ShowMonster = 54, /*모든 몬스터를 보이게 한다*/
		ShowNpc = 55, /*모든 npc를 보이게 한다*/
		Event = 56, /*특수한 이벤트를 처리한다*/
		TouchWorldMapWorld = 57, /*월드맵에서 월드 지역 보기 버튼을 클릭한다.*/
		TouchEnhanceEquip = 58, /*장비 강화 팝업에서 강화 버튼을 터치한다.*/
		TouchEnhanceMaterial = 59, /*장비 강화 팝업에서 재료 선택 버튼을 터치한다.*/
		TouchEnhanceClose = 60, /*장비 강화 팝업에서 닫기 버튼을 터치한다.*/
	}

	public enum E_NameTextType
	{
		English = 0, /*영어*/
		Korean = 1, /*한국어*/
		Arabic = 2, /*숫자*/
	}

	public enum E_MaterialType
	{
		Fix = 0, /*고정 재료량*/
		Increment = 1, /*구입 횟수에 의해 증가*/
	}

	public enum E_CategoryType
	{
		None = 0, /*없음*/
		Equip = 1, /*장비*/
		EquipAcc = 2, /*장신구*/
		Change = 3, /*변신*/
		Pet = 4, /*펫*/
		SkillBook = 5, /*스킬북*/
	}

	public enum E_SpecialButtonType
	{
		None = 0, /**/
		Evasion = 1, /*회피*/
		ElcidEvasion = 2, /*엘시드 회피*/
	}

	public enum E_RaidMatchType
	{
		Auto = 0, /*자동*/
		Manual = 1, /*수동*/
	}

	public enum E_RaidGrade
	{
		Easy = 0, /*쉬움*/
		Normal = 1, /*보통*/
		Hard = 2, /*어려움*/
		Hero = 3, /*영웅*/
	}

	public enum E_ScenarioDirectionType
	{
		None = 0, /**/
		Dialogue = 1, /*다이알로그*/
		Guide = 2, /*가이드, 기능*/
	}

	public enum E_ScenarioDialogueType
	{
		None = 0, /**/
		Image = 1, /*이미지 출력 타입*/
		ImageNoise = 2, /*이미지 흔들림 / DialogueParams초만큼 출력*/
		ImageQuake = 3, /*화면 흔들림 / DialogueParams초만큼 출력*/
		ImageHide = 4, /*이미지 반투명*/
		ImageFadeout = 5, /*대화 끝날때 페이드 아웃*/
	}

	public enum E_ScenarioGuideType
	{
		None = 0, /**/
		GuideTime = 1, /*가이드가 일정시간동안 나옴*/
		SpecialButton = 2, /*퀵슬롯 터치 / GuideParams번째 터치*/
	}

	public enum E_ChangeType
	{
		None = 0, /*없음*/
		Character = 1, /*캐릭터*/
		Skill = 2, /*스킬*/
		Item = 3, /*아이템*/
	}

	public enum E_ChangeClassType
	{
		None = 0, /*없음*/
		Knight = 1, /*기사*/
		Archer = 2, /*궁수*/
		Wizard = 3, /*마법사*/
		Assassin = 4, /*암살자*/
	}

	public enum E_MiniShopType
	{
		None = 0, /*없음*/
		limit = 1, /*한정 상품*/
		Souvenir = 2, /*기념품*/
	}

	public enum E_MiniSubTapType
	{
		None = 0, /*없음*/
		Etc = 1, /*기타*/
		Consumable = 2, /*소모품*/
	}

	[System.Flags]
	public enum E_ChangeQuestType
	{
		None = 0, /*없음*/
		AttackShort = 1, /*근거리*/
		AttackLong = 2, /*원거리*/
	}

	public enum E_PetType
	{
		Pet = 0, /*펫*/
		Vehicle = 1, /*탈것*/
	}

	public enum E_PetLevelUpType
	{
		Up = 0, /*레벨 업 가능*/
		End = 1, /*레벨 업 불가능*/
	}

	public enum E_OpenDay
	{
		Sun = 0, /*일요일*/
		Mon = 1, /*월요일*/
		Tue = 2, /*화요일*/
		Wed = 3, /*수요일*/
		Thu = 4, /*목요일*/
		Fri = 5, /*금요일*/
		Sat = 6, /*토요일*/
		All = 99, /*전체*/
	}

	public enum E_JoinPetType
	{
		All = 0, /*전체*/
		human = 1, /*사람*/
		Oak = 2, /*오크*/
		Golem = 3, /*골렘*/
		Ogre = 4, /*오우거*/
		Male = 5, /*남자*/
		Female = 6, /*여자*/
	}

	public enum E_AdventureTab
	{
		None = 0, /*없음*/
		Silver = 1, /*실버 던전*/
		Essence = 2, /*정수 던전*/
		PetMaterial = 3, /*펫 제작서 던전*/
		SkillBook = 4, /*스킬북 던전*/
		MarkCoin = 5, /*유물 코인 던전*/
	}

	public enum E_RoundingType
	{
		None = 0, /*없음*/
		Down = 1, /*내림 처리*/
		Up = 2, /*올림 처리*/
	}

	public enum E_RuneOptionType
	{
		None = 0, /*없음*/
		BaseOption = 1, /*베이스 주 옵션*/
		FirstOption = 2, /*접두어 옵션*/
		SubOption = 3, /*부 옵션*/
	}

	public enum E_AbilityRetain
	{
		None = 0, /*없음*/
		Retain = 1, /*어빌리티 유지*/
	}

	public enum E_UIType
	{
		None = 0, /*없음*/
		Item = 1, /*아이템*/
		Change = 2, /*변신*/
		Pet = 3, /*펫*/
		Skill = 4, /*스킬*/
		Gem = 5, /*젬*/
		Header = 6, /*아이템 정보창 헤더*/
	}

	public enum E_RuneAbilityType
	{
		None = 0, /*없음*/
		MaxHpPlus = 1, /*체력*/
		MaxHpPer = 2, /*체력(%)*/
		MeleeAttackPlus = 3, /*공격력*/
		MeleeAttackPer = 4, /*공격력(%)*/
		MagicAttackPlus = 5, /*마법 공격력*/
		MagicAttackPer = 6, /*마법 공격력(%)*/
		DefencePlus = 7, /*방어력*/
		DefencePer = 8, /*방어력(%)*/
		PotionRecovery = 9, /*포션 회복량*/
		MezEnemy = 10, /*상태이상 적중*/
		MezResistance = 11, /*상태이상 저항*/
	}

	public enum E_RuneAbilityViewType
	{
		None = 0, /*없음*/
		RUNE_MAX_HP_PLUS = 1, /*체력*/
		RUNE_MAX_HP_PER = 2, /*체력(%)*/
		RUNE_SHORT_ATTACK_PLUS = 3, /*근거리 공격력*/
		RUNE_SHORT_ATTACK_PER = 4, /*근거리 공격력(%)*/
		RUNE_LONG_ATTACK_PLUS = 5, /*원거리 공격력*/
		RUNE_LONG_ATTACK_PER = 6, /*원거리 공격력(%)*/
		RUNE_MAGIC_ATTACK_PLUS = 7, /*마법 공격력*/
		RUNE_MAGIC_ATTACK_PER = 8, /*마법 공격력(%)*/
		RUNE_MELEE_DEFENCE_PLUS = 9, /*방어력*/
		RUNE_MELEE_DEFENCE_PER = 10, /*방어력(%)*/
		RUNE_MAGIC_DEFENCE_PLUS = 11, /*마법 방어력*/
		RUNE_MAGIC_DEFENCE_PER = 12, /*마법 방어력(%)*/
		RUNE_MAZ_RATE_DOWN_PER = 13, /*상태이상 적중*/
		RUNE_MAZ_RATE_UP_PER = 14, /*상태이상 저항*/
		RUNE_POTION_RECOVERY_PLUS = 15, /*포션 회복량*/
		RUNE_ACCURACY_PLUS = 16, /*명중*/
		RUNE_EVASION_PLUS = 17, /*회피*/
		RUNE_REDUCTION_PLUS = 18, /*피해감소 무시*/
		RUNE_REDUCTION_IGNORE_PLUS = 19, /*피해감소*/
	}

	public enum E_BroadcastType
	{
		None = 0, /*없음*/
		Item = 1, /*아이템*/
		Change = 2, /*강림*/
		Pet = 3, /*펫*/
		AccountStack = 4, /*계정 재화*/
		Stack = 5, /*스택 아이템*/
	}

	public enum E_AutoUseType
	{
		None = 0, /*없음*/
		Auto = 1, /*필드 입장 시 오토 설정*/
		NotAuto = 2, /*필드 입장 시 오토 설정 안함*/
		RetainAuto = 3, /*필드 입장 시 이전 필드에서 오토인 경우 오토 유지*/
	}

	public enum E_BuyOpenType
	{
		None = 0, /*없음*/
		Always = 1, /*항상*/
		Get = 2, /*획득 시 구매 가능*/
	}

	public enum E_ExtractionType
	{
		None = 0, /*없음*/
		Extraction = 1, /*추출 가능*/
	}

	[System.Flags]
	public enum E_EqupItemType
	{
		None = 0, /*없음*/
		Weapon = 1, /*무기*/
		SideWeapon = 2, /*보조무기*/
		Helmet = 4, /*투구*/
		Armor = 8, /*갑옷*/
		Pants = 16, /*바지*/
		Shoes = 32, /*신발*/
		Gloves = 64, /*장갑*/
	}

	public enum E_SmeltScrollUseType
	{
		None = 0, /*없음*/
		SmeltScroll = 1, /*제련 가능*/
	}

	public enum E_GuildBuffType
	{
		None = 0, /*없음*/
		Active = 1, /*액티브*/
		Passive = 2, /*패시브*/
	}

	public enum E_InfinityDungeonRotation
	{
		First = 1, /*불칸의빛 던전*/
		Second = 2, /*경험치 던전*/
		Third = 3, /*골드 던전*/
		Forth = 4, /*미정*/
		Fifth = 5, /*미정*/
		Sixth = 6, /*미정*/
		Seventh = 7, /*미정*/
		Eighth = 8, /*미정*/
		Ninth = 9, /*미정*/
		Tenth = 10, /*미정*/
	}

	public enum E_DungeonType
	{
		None = 0, /*없음*/
		Essence = 1, /*불칸의 빛*/
		Exp = 2, /*경험치*/
		Gold = 3, /*골드*/
		SkillBook = 4, /*스킬북*/
	}

	public enum E_RuneType
	{
		None = 0, /*없음*/
		PhysicalRune = 1, /*체력의 룬*/
		BattleRune = 2, /*격투의 룬*/
		AttackRune = 3, /*공격의 룬*/
		MagicAttackRune = 4, /*마법의 룬*/
		DefenseRune = 5, /*방어의 룬*/
		ProtectRune = 6, /*가호의 룬*/
		IronWallRune = 7, /*철벽의 룬*/
		PunitiveRune = 8, /*징벌의 룬*/
		SpotRune = 9, /*명중의 룬*/
		EvasiveRune = 10, /*회피의 룬*/
		FastPacedRune = 11, /*속공의 룬*/
		SwiftRune = 12, /*신속의 룬*/
		EnemyRune = 13, /*적중의 룬*/
		ResistanceRune = 14, /*저항의 룬*/
		RecoveryRune = 15, /*회복의 룬*/
		ManaRune = 16, /*명상의 룬*/
	}

	public enum E_GachaGetType
	{
		None = 0, /*없음*/
		Direct = 1, /*즉시 획득*/
		RuneSelect = 2, /*룬 선택 후 획득*/
	}

	public enum E_ArtifactActionType
	{
		None = 0, /*없음*/
		Make = 1, /*제작*/
		Upgrade = 2, /*승급*/
	}

	public enum E_BonusType
	{
		None = 0, /*없음*/
		Infinity = 1, /*무제한 지급*/
		OneShot = 2, /*한 번 지급*/
	}

	public enum E_DropType
	{
		None = 0, /*없음*/
		Item = 1, /*아이템*/
		Rune = 2, /*룬*/
	}

	public enum E_TeleportType
	{
		None = 0, /*텔레포트 불가*/
		Teleport = 1, /*텔레포트 가능*/
	}

	public enum E_ApplyType
	{
		None = 0, /*없음*/
		Character = 1, /*캐릭터 기준*/
		Account = 2, /*계정 기준*/
	}

	public enum E_NameViewType
	{
		None = 0, /*사용안함*/
		NameView = 1, /*이름표시 적용*/
	}

	public enum E_ObjInteractionType
	{
		None = 0, /*사용안함*/
		Action = 1, /*실행*/
		Pickup = 2, /*들기, 놓기*/
		Push = 3, /*밀기, 당기기*/
	}

	public enum E_PhysicType
	{
		None = 0, /*사용안함*/
		Physic = 1, /*물리 적용*/
	}

	public enum E_WeightType
	{
		None = 0, /*사용안함*/
		Weight = 1, /*무게 적용*/
	}

	public enum E_ObjMaterialType
	{
		None = 0, /*없음*/
		Wood = 1, /*나무*/
		Stone = 2, /*돌*/
		CrackStone = 3, /*파괴 가능한 돌*/
		Iron = 4, /*철*/
		Alloy = 5, /*합금*/
	}

	public enum E_TempleType
	{
		None = 0, /*사용안함*/
		God = 1, /*주신*/
		Quest = 2, /*퀘스트*/
		Normal = 3, /*일반*/
	}

	public enum E_HiddenUI
	{
		None = 0, /*사용안함*/
		Hidden = 1, /*UI 숨김*/
	}

	public enum E_RegistrationUI
	{
		None = 0, /*사용안함*/
		Registration = 1, /*UI 등록*/
	}

	public enum E_Replay
	{
		None = 0, /*사용안함*/
		Replay = 1, /*재진입 가능*/
	}

	public enum E_DialogueType
	{
		None = 0, /*사용안함*/
		Dialogue = 1, /*다이얼로그*/
		Guide = 2, /*가이드*/
		SelectDialogue = 3, /*선택대사*/
	}

	public enum E_SelectRewardOrderType
	{
		None = 0, /*사용안함*/
		Use = 1, /*선택보상 사용*/
	}

	public enum E_DialogueAutoNextType
	{
		None = 0, /*사용안함*/
		AutoNext = 1, /*자동 대사 넘김*/
	}

	public enum E_DialogueResourceType
	{
		None = 0, /*사용안함*/
		Image = 1, /*이미지*/
		Object = 2, /*오브젝트*/
	}

	public enum E_DialogueSkipType
	{
		None = 0, /*사용안함*/
		Show = 1, /*건너뛰기 버튼 표시*/
	}

	public enum E_DialogueBGType
	{
		None = 0, /*사용안함*/
		Use = 1, /*암부 사용*/
	}

	public enum E_RidingType
	{
		NotRiding = 0, /*탑승물 소환 불가*/
		Riding = 1, /*탑승물 소환 가능*/
	}

	public enum E_ArtifactMaterialType
	{
		None = 0, /*없음*/
		Pet = 1, /*펫*/
		Vehicle = 2, /*탈것*/
	}

	public enum E_ObjectType
	{
		Normal = 0, /*조건 없이 채집 가능*/
		NeedQuest = 1, /*특정 퀘스트 보유 중에만 채집 가능*/
	}

	public enum E_ObjectActionType
	{
		Unique = 0, /*한 번에 하나의 캐릭터만 상호작용 가능*/
		AnyOne = 1, /*동시에 다수의 캐릭터가 상호작용 가능*/
	}

	public enum E_ObjectSpawnType
	{
		Always = 0, /*항상*/
		Triggered = 1, /*조건 만족 시*/
		ChaosChannelOnly = 2, /*카오스 채널에서만 생성*/
	}

	public enum E_AttributeLevel
	{
		Level_1 = 1, /*유적 기믹풀이에 사용되는 속성 단계 1*/
		Level_2 = 2, /*유적 기믹풀이에 사용되는 속성 단계 2*/
		Level_3 = 3, /*유적 기믹풀이에 사용되는 속성 단계 3*/
		Level_4 = 4, /*유적 기믹풀이에 사용되는 속성 단계 4*/
		Level_5 = 5, /*유적 기믹풀이에 사용되는 속성 단계 5*/
		Level_6 = 6, /*유적 기믹풀이에 사용되는 속성 단계 6*/
	}

	public enum E_EventOpenDay
	{
		None = 0, /*없음*/
		FirstDay = 1, /*1일차*/
		SecondDay = 2, /*2일차*/
		ThirdDay = 3, /*3일차*/
		ForthDay = 4, /*4일차*/
		FifthDay = 5, /*5일차*/
		SixthDay = 6, /*6일차*/
		SeventhDay = 7, /*7일차*/
		EighthDay = 8, /*8일차*/
		NinthDay = 9, /*9일차*/
		TenthDay = 10, /*10일차*/
	}

	public enum E_EventCompleteCheck
	{
		None = 0, /*없음*/
		MonsterKill = 1, /*특정 몬스터 사냥 시*/
		Level = 2, /*특정 레벨 도달 시*/
		MapMove = 3, /*특정 맵 이동 시*/
		GetObject = 4, /*특정 오브젝트 인터렉션 시*/
		EquipEnchant = 5, /*장비 강화*/
		EquipUpgrade = 6, /*장비 승급*/
		ChangeCompose = 7, /*변신 합성*/
		PetCompose = 8, /*펫 합성*/
		RideCompose = 9, /*탈 것 합성*/
		ClearTemple = 10, /*유적 완료 시*/
		BreakItem = 11, /*아이템 분해*/
		AttendEvent = 12, /*출석 이벤트*/
		AttendGuildEvent = 13, /*길드 출석*/
		GuildGive = 14, /*길드 기부*/
		StageInstance = 15, /*시련의 성역 클리어*/
		StageInfinity = 16, /*주신의 탑 데일리 보상 획득*/
		MonsterKillField = 17, /*필드 몬스터 처치*/
		ChangeQuest = 18, /*클래스 파견 보내기*/
		PetAdventure = 19, /*펠로우 탐험 보내기*/
		RepeatQuest = 20, /*반복 퀘스트 수행*/
		AttendInterServer = 21, /*5군단 참여*/
		AttendColosseum = 22, /*영웅의 전당 참여*/
		SellItem = 23, /*아이템 판매*/
		SellItemID = 24, /*특정 아이템 판매*/
		BuyItem = 25, /*아이템 구매*/
		BuyItemID = 26, /*특정 아이템 구매*/
		AnyGacha = 27, /*Gacha 타입 아이템 사용*/
		ItemSmelt = 28, /*연금 아이템 사용*/
		RuneEquipEnchant = 29, /*장식 강화*/
		MakeItem = 30, /*제작*/
		MakeItemType = 31, /*특정타입 제작*/
		TargetItemCollect = 32, /*대상 아이템 수집(완료 시 대상 아이템 삭제)*/
	}

	public enum E_PayAttendEventStep
	{
		Next = 0, /*다음*/
		End = 1, /*종료*/
	}

	public enum E_ServerEventSubCategory
	{
		EventDungeon = 0, /*이벤트 던전*/
		Colosseum = 1, /*영웅의 전당*/
		BannerPopUp = 2, /*배너 이벤트*/
		QuestEvent = 3, /*퀘스트 이벤트*/
		GoldUpEvent = 4, /*골드 드랍 상승 이벤트*/
		ExpUpEvent = 5, /*경험치 드랍 상승 이벤트*/
		ItemDropRateUpEvent = 6, /*아이템 드랍율 상승 이벤트*/
		ItemDropEvent = 7, /*아이템 수집 이벤트*/
		ScenarioDungeon = 8, /*시나리오 던전*/
		AttendEvent = 9, /*일반 출석 이벤트*/
		AttendEventNewUser = 10, /*신규 유저 출석 이벤트*/
		AttendEventComeback = 11, /*복귀 유저 출석 이벤트*/
		AttendEventComulative = 12, /*누적 출석 이벤트*/
		AttendEventContiniuity = 13, /*연속 출석 이벤트*/
		AttendEventPaidDaily = 14, /*유료 출석 이벤트*/
		AttendEventPaidLevelUp = 15, /*유료 레벨업 이벤트*/
		BattlePass = 16, /*배틀패스*/
		BlackMarket = 17, /*암시장*/
		AddAttend = 18, /*추가 출석 보상*/
	}

}
