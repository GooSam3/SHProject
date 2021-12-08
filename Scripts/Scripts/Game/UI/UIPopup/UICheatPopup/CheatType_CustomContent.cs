using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatType_CustomContent : CheatPanel
{
    public override void Initialize()
    {
    }

    // i : style
    public void OnClickCombineDirection(int i)
    {
        UIGachaEnum.E_GachaStyle style = (UIGachaEnum.E_GachaStyle)i;

        var CardValue = new List<uint>();

        UIGachaEnum.E_TimeLineType timeline = UIGachaEnum.E_TimeLineType.Class_10_Start;

        switch (style)
        {
            case UIGachaEnum.E_GachaStyle.Pet:
                timeline = UIGachaEnum.E_TimeLineType.Pet_10_Start;
                CardValue.AddRange(new uint[] { 210005, 210006, 210003, 220001, 220002, 220003, 230001, 230002, 230003, 240001,
                                                615010, 210006, 210003, 220001, 220002, 220003, 230001, 230002, 230003, 240001,
                                                615010, 210006, 210003, 220001, 220002, 220003, 230001, 230002, 230003, 240001,
                                                615010, 210006, 210003});
                break;
            case UIGachaEnum.E_GachaStyle.Class:
                timeline = UIGachaEnum.E_TimeLineType.Class_10_Start;
                CardValue.AddRange(new uint[] { 10013, 20014, 30010, 40022, 50023, 60017, 10013, 20014, 30010, 40022,
                                                50023, 10013, 20014, 30010, 40022, 50023, 60017, 10013, 20014, 30010,
                                                40022, 50023, 10013, 20014, 30010, 40022, 50023, 60017, 10013, 20014,
                                                30010, 40022, 50023});
                break;
            case UIGachaEnum.E_GachaStyle.Ride:
                timeline = UIGachaEnum.E_TimeLineType.Ride_10_Start;
                CardValue.AddRange(new uint[] { 130009, 130010, 130011, 130012, 160007, 130014, 130015, 130016, 160008, 130018,
                                                130019, 130010, 130011, 130012, 160007, 130014, 130015, 130016, 160008, 130018,
                                                130019, 130010, 130011, 130012, 160007, 130014, 130015, 130016, 160008, 130018,
                                                130019, 130010, 130011 });
                break;

            default:
                return;
        }

        UIManager.Instance.Open<UIPopupGachaResult>((str, frame) =>
        {
            frame.SetCombineResult(style, timeline, CardValue);
        });

        UIManager.Instance.Close<UICheatPopup>();
    }

    // i : timeline_start
    public void OnClickGachaDirection(int i)
    {
        UIGachaEnum.E_TimeLineType timeline = (UIGachaEnum.E_TimeLineType)i;
        UIGachaEnum.E_GachaStyle style = (UIGachaEnum.E_GachaStyle)i;

        var CardValue = new List<uint>();

        switch (timeline)
        {
            case UIGachaEnum.E_TimeLineType.Item_1_Start://1
                CardValue.AddRange(new uint[] { 611010 });
                style = UIGachaEnum.E_GachaStyle.Item;
                break;
            case UIGachaEnum.E_TimeLineType.Item_11_Start://3
                CardValue.AddRange(new uint[] { 611010, 612010, 613010, 614010, 615010, 616010, 611010, 612010, 613010, 614010, 615010 });
                style = UIGachaEnum.E_GachaStyle.Item;
                break;
            case UIGachaEnum.E_TimeLineType.Ride_1_Start://10
                CardValue.AddRange(new uint[] { 160007 });
                style = UIGachaEnum.E_GachaStyle.Ride;
                break;
            case UIGachaEnum.E_TimeLineType.Ride_11_Start://12
                CardValue.AddRange(new uint[] { 130009, 130010, 130011, 130012, 160007, 130014, 130015, 130016, 160008, 130018, 130019, });
                style = UIGachaEnum.E_GachaStyle.Ride;
                break;
            case UIGachaEnum.E_TimeLineType.Class_1_Start://20
                CardValue.AddRange(new uint[] { 50023 });
                style = UIGachaEnum.E_GachaStyle.Class;
                break;
            case UIGachaEnum.E_TimeLineType.Class_11_Start://22
                CardValue.AddRange(new uint[] { 10013, 20014, 30010, 40022, 50023, 60017, 10013, 20014, 30010, 40022, 50023 });
                style = UIGachaEnum.E_GachaStyle.Class;
                break;
            case UIGachaEnum.E_TimeLineType.Pet_1_Start://30
                CardValue.AddRange(new uint[] { 210005 });
                style = UIGachaEnum.E_GachaStyle.Pet;
                break;
            case UIGachaEnum.E_TimeLineType.Pet_11_Start://32
                CardValue.AddRange(new uint[] { 210005, 210006, 210003, 220001, 220002, 220003, 230001, 230002, 230003, 240001, 615010, });
                style = UIGachaEnum.E_GachaStyle.Pet;
                break;

            default:
                return;
        }

        UIManager.Instance.Open<UIFrameGacha>((str, frame) =>
        {
            frame.SetGachaData(style, timeline, CardValue);
        });

        UIManager.Instance.Close<UICheatPopup>();
    }
}
