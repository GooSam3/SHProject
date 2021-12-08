using System;

public static class EnumHelper
{
    public static T[] Values<T>()
    {
        return (T[])Enum.GetValues(typeof(T));
    }

    private static void CheckIsEnum<T>(bool withFlags)
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException(string.Format("Type '{0}' is not an enum", typeof(T).FullName));
        if (withFlags && !Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
            throw new ArgumentException(string.Format("Type '{0}' doesn't have the 'Flags' attribute", typeof(T).FullName));
    }

    public static bool CheckFlag<T, V>(T mask, V flag) where T : struct where V : struct
    {
        CheckIsEnum<T>(false);
        CheckIsEnum<V>(false);
        long lValue = Convert.ToInt64(mask);
        long lFlag = Convert.ToInt64(flag);
        return (lValue & lFlag) != 0;
    }

    //public static bool CheckFlag(GameDB.E_LimitType mask, GameDB.E_LimitType flag)
    //{
    //    return (mask & flag) > 0;
    //}

    //public static bool CheckFlag(GameDB.E_CharacterType mask, GameDB.E_CharacterType flag)
    //{
    //    return (mask & flag) > 0;
    //}

    //public static bool CheckFlag(GameDB.E_EnchantUseType mask, GameDB.E_EnchantUseType flag)
    //{
    //    return (mask & flag) > 0;
    //}

    //public static bool CheckFlag(UIDefine.Alram mask, UIDefine.Alram flag)
    //{
    //    return (mask & flag) > 0;
    //}
}
