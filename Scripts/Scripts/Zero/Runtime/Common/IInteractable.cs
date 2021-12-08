
namespace Zero
{
	/// <summary>
	/// 상호작용 기능이 필요한 클래스를 위한 공용 인터페이스
	/// </summary>
	public interface IInteractable
	{
		/// <summary>
		/// </summary>
		/// <param name="_interactor">상호작용 시킨 주체</param>
		void Interact(object _interactor);
	}
}//