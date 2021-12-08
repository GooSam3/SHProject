using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zero;

public class CoroutineManager : Singleton<CoroutineManager>
{
	public delegate void Callback();

	private static readonly Dictionary<float, WaitForSeconds> timeInterval = new Dictionary<float, WaitForSeconds>();

	public Coroutine StartTimer( float time_sec, Callback callback )
	{
		if( time_sec <= 0.0f ) {
			callback?.Invoke();
			return null;
		}

		return StartCoroutine( _StartTime( time_sec, callback ) );
	}

	public Coroutine NextFrame( Callback callback )
	{
		return StartCoroutine( _NextFrame( 1, callback ) );
	}

	public Coroutine Frame( int count, Callback callback )
	{
		return StartCoroutine(_NextFrame(count, callback));
	}

	/// <summary>
	/// StopCoroutine 코루틴을 돌리고 있는 같은 객체(CoroutineManager)가 스탑을 해야 에러코드(단순에러)가 나오지 않는다
	/// </summary>
	public void StopAction( Coroutine coroutine )
	{
		if( coroutine != null ) {
			StopCoroutine( coroutine );
		}
	}

	public void StopAllActions()
	{
		StopAllCoroutines();
	}

	public void Interval( float time, int count, bool immediate, Callback callback, Callback finished )
	{
		int total_count = count;
		if( immediate ) {
			callback?.Invoke();

			total_count--;
			if( total_count < 0 ) {
				return;
			}
		}

		float last = 0.0f;
		for( int i = 0; i < total_count; ++i ) {
			last = time + ( time * i );
			StartTimer( last, callback );
		}
		StartTimer( last, finished );
	}

	private IEnumerator _StartTime( float time_sec, Callback callback )
	{
		yield return WaitForSeconds( time_sec );

		callback?.Invoke();
	}

	private IEnumerator _NextFrame( int count, Callback callback )
	{
		for( int i = 0; i < count; ++i ) {
			yield return null;
		}
		callback?.Invoke();
	}

	// 유니티 코루틴 최적화  https://ejonghyuck.github.io/blog/2016-12-12/unity-coroutine-optimization/
	private WaitForSeconds WaitForSeconds( float seconds )
	{
		WaitForSeconds wfs;
		if( timeInterval.TryGetValue( seconds, out wfs ) == false ) {
			timeInterval.Add( seconds, wfs = new WaitForSeconds( seconds ) );
		}
		return wfs;
	}
}