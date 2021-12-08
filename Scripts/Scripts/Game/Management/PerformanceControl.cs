//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AdaptivePerformance;

//public class PerformanceControl : MonoBehaviour
//{
//	private IAdaptivePerformance mAP = null;
	
//	void Start()
//    {
//		if (null == Holder.Instance)
//			return;

//		mAP = Holder.Instance;

//		if (!mAP.Active)
//			return;

//		//QualitySettings.lodBias = 1;

//		mAP.ThermalStatus.ThermalEvent += OnThermalEvent;
//		mAP.PerformanceStatus.PerformanceBottleneckChangeEvent += OnPerformanceStatus;
//	}

//	private void OnDestroy()
//	{
//		if (null != mAP)
//		{
//			mAP.ThermalStatus.ThermalEvent -= OnThermalEvent;
//			mAP.PerformanceStatus.PerformanceBottleneckChangeEvent += OnPerformanceStatus;
//		}
//	}

//	void OnThermalEvent(ThermalMetrics thermalMetrics)
//	{
//		// 부하가 심해질수록 LOD거리는 줄이도록 해서 LOD단계를 높이도록 한다.
//		switch (thermalMetrics.WarningLevel)
//		{
//			case WarningLevel.NoWarning:
//				{
//					QualitySettings.lodBias = 1;
//				}
//				break;

//			case WarningLevel.ThrottlingImminent:
//				{
//					QualitySettings.lodBias = 0.75f;
//				}
//				break;

//			case WarningLevel.Throttling:
//				{
//					QualitySettings.lodBias = 0.5f;
//				}
//				break;
//		}
//	}

//	void OnPerformanceStatus(PerformanceBottleneckChangeEventArgs bottleneckEventArgs)
//	{
//		var devicePerfControl = mAP.DevicePerformanceControl;

//		switch (bottleneckEventArgs.PerformanceBottleneck)
//		{
//			case PerformanceBottleneck.TargetFrameRate:
//				{
//					if (devicePerfControl.CpuLevel > 0)
//					{
//						devicePerfControl.CpuLevel--;
//					}
//					if (devicePerfControl.GpuLevel > 0)
//					{
//						devicePerfControl.GpuLevel--;
//					}
//				}
//				break;

//			case PerformanceBottleneck.GPU:
//				{
//					if (devicePerfControl.GpuLevel < devicePerfControl.MaxGpuPerformanceLevel)
//					{
//						devicePerfControl.GpuLevel++;
//					}
//				}
//				break;

//			case PerformanceBottleneck.CPU:
//				{
//					if (devicePerfControl.CpuLevel < devicePerfControl.MaxCpuPerformanceLevel)
//					{
//						devicePerfControl.CpuLevel++;
//					}
//				}
//				break;
//		}
//	}
//}
