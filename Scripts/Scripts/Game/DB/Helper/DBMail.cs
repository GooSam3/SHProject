using GameDB;

public class DBMail : IGameDBHelper
{
    public void OnReadyData()
    {
    }

    public static Mail_Table GetMailData(E_MailType mailType)
    {
        foreach (var tableData in GameDBManager.Container.Mail_Table_data.Values)
        {
            if (tableData.MailType == mailType)
                return tableData;
        }

        return null;
    }
}
