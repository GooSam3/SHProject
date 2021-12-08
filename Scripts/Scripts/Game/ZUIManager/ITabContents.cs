
public interface ITabContents
{
	int Index { get; set; }

	void Initialize();

	void Open();

	void Refresh();

	void Close();
}