using System.IO;

public class FileHelper
{
    /// <summary>현재 대상 파일을 이용가능한지 검사해준다. </summary>
    /// <remarks>
    /// https://www.codeproject.com/Questions/493093/c-23pluscheckingplusifplusaplusfileplusisplusalrea
    /// </remarks>
    public static bool IsFileinUse(FileInfo file)
    {
        FileStream stream = null;

        try
        {
            stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException)
        {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            return true;
        }
        finally
        {
            if (stream != null)
                stream.Close();
        }
        return false;
    }

    public static void ClearFolder(string folderPath)
    {
        System.IO.DirectoryInfo di = new DirectoryInfo(folderPath);
		if (null == di || !di.Exists)
			return;

        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            dir.Delete(true);
        }
    }
}