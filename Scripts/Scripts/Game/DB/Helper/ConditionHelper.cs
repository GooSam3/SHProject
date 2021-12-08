using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZDefine;
using ZNet.Data;

/// <summary>
/// 조건검사 헬퍼
/// </summary>
[UnityEngine.Scripting.Preserve]
class ConditionHelper
{
    /// <summary>
    /// 부족한 재화 검사 후 팝업오픈
    /// </summary>
    /// <param name="tid">아이템tid</param>
    /// <param name="count">요구 갯수</param>
    /// <returns>보유 여부</returns>
    /// <seealso cref="GetLackLocale(uint)"/>
    public static bool CheckCompareCost(uint tid, ulong count, bool showPopup = true, Action onClickOK = null)
    {
        if (count <= 0)
            return true;

        if (DBItem.GetItem(tid, out var table) == false)
        {
            ZLog.LogError(ZLogChannel.System, $"테이블에 아이템 없음~~ tid : {tid}");

            return false;
        }

        ZItem item = Me.CurCharData.GetItem(tid);

        // 캐시(다이아)는 따로처리
        if (tid == DBConfig.Diamond_ID)
        {
            if (Me.CurUserData.Cash < (long)count)
            {
                if (showPopup)
                    UIMessagePopup.ShowPopupOk(GetLackLocale(tid), onClickOK);

                return false;
            }
        }
        else if (item == null || item.cnt < count)
        {
            if (showPopup)
                UIMessagePopup.ShowPopupOk(GetLackLocale(tid), onClickOK);

            return false;
        }

        return true;
    }

    // 호환 가능 여부 ( 로케일 추가되있나), 로그출력용
    private static void CheckLocaleAdded(uint tid)
    {
        if (tid == DBConfig.Essence_ID)
            return;
        if (tid == DBConfig.Gold_ID)
            return;
        if (tid == DBConfig.Diamond_ID)
            return;
        if (tid == DBConfig.Town_Move_ItemID)
            return;

        ZLog.Log(ZLogChannel.System, $"헬퍼에 등록안된 tid, 기본 로케일 출력 | tid : {tid}");
    }

    // 로케일 출력
    private static string GetLackLocale(uint tid)
    {
        CheckLocaleAdded(tid);

        if (DBConfig.Gold_ID == tid)
        {
            return DBLocale.GetText("NOT_ENOUGH_GOLD");
        }

        if (DBConfig.Essence_ID == tid)
        {
            return DBLocale.GetText("Mark_Vulkan_lack");
        }

        if (DBConfig.Diamond_ID == tid)
        {
            return DBLocale.GetText("NOT_ENOUGH_DIAMOND");
        }

        if (DBConfig.Town_Move_ItemID == tid)
        {
            return DBLocale.GetText("Goods_Lack_TownStone");
        }

        return DBLocale.GetText("Error_Lack_Material");
    }
}
