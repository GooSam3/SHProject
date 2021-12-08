using System;
using System.Collections.Generic;
using UnityEngine;
using WebNet; // protocol
using ZNet.Data;

namespace ZNet
{
	/// <summary>
	/// 결제용
	/// </summary>
	public class ZWebBilling : ZWebRequestClientBase
	{
		public override bool IsUsable
		{
			get { return !string.IsNullOrEmpty(this.ServerUrl) && this.enabled; }
		}

		public override E_WebSocketType SocketType => E_WebSocketType.Billing;

		/// <summary> 게임서버에서 받은 통신키 (보안키로 사용) </summary>
		public override uint SecurityKey { get => mRndKey; protected set => mRndKey = value; }

		private uint mRndKey = 0;

		/// <summary>
		/// 구매전 결제 초기화
		/// </summary>
		/// <param name="_ShopTid"></param>
		/// <param name="_Price"></param>
		/// <param name="_CurrencyCode"></param>
		public void REQ_PaymentInit(uint _ShopTid, float _Price, string _CurrencyCode, Action<ZWebRecvPacket, ResPaymentInit> onRecvPacket, WebRequestErrorDelegate _onNetError = null)
		{
			var offset = ReqPaymentInit.CreateReqPaymentInit(mBuilder,
				_ShopTid,
				ZGameManager.Instance.GetMarketType(),
				mBuilder.CreateString(NTCore.CommonAPI.RuntimeOS),
				_Price,
				mBuilder.CreateString(_CurrencyCode),
				mBuilder.CreateString(Application.version));

			var reqPacket = ZWWWPacket.Create<ReqPaymentInit>(Code.BI_PAYMENT_INIT, mBuilder, offset.Value);
			
			RequestWebData(ServerUrl, reqPacket, (recvPacket) =>
			{
				ResPaymentInit resPaymentInit = recvPacket.Get<ResPaymentInit>();
				onRecvPacket?.Invoke(recvPacket, resPaymentInit);

				onRecvPacket?.Invoke(recvPacket, resPaymentInit);
			},  _onNetError);
		}

		/// <summary>
		/// 상품 구매 요청
		/// </summary>
		/// <param name="_ProductInfo"></param>
		/// <param name="_Signature"></param>
		/// <param name="_GpsAdid"></param>
		public void REQ_PaymentPurchase(NTCore.PurchaseProductInfo _ProductInfo, string _Signature, string _GpsAdid, System.Action<ZWebRecvPacket, ResPaymentPurchase> onRecvPacket, WebRequestErrorDelegate _onNetError = null)
		{
			var offset = ReqPaymentPurchase.CreateReqPaymentPurchase(mBuilder,
				ZGameManager.Instance.GetMarketType(),
				mBuilder.CreateString(_Signature),
				mBuilder.CreateString(NTCore.CommonAPI.RuntimeOS),
				mBuilder.CreateString(NTCommon.Device.Release),
				mBuilder.CreateString(_ProductInfo.currenyCode),
				mBuilder.CreateString(NTIcarusManager.Instance.GetAndroidId()),
				mBuilder.CreateString(com.adjust.sdk.Adjust.getIdfa()),
#if UNITY_IOS && !UNITY_EDITOR
				mBuilder.CreateString(NTCore.CommonAPI.getIDFV()),
#else
				mBuilder.CreateString(""),
#endif
				mBuilder.CreateString(_GpsAdid),
				mBuilder.CreateString(NTCore.CommonAPI.GetGameServerID()),
				mBuilder.CreateString(NTCommon.Locale.GetLocaleCode()),
				mBuilder.CreateString(NTCore.CommonAPI.GetGameSupportLanguage().langCulture),
				_ProductInfo.price.ToFloat);

			var reqPacket = ZWWWPacket.Create<ReqPaymentPurchase>(Code.BI_PAYMENT_PURCHASE, mBuilder, offset.Value);

			RequestWebData(ServerUrl, reqPacket, (recvPacket) =>
			{
				ResPaymentPurchase resPaymentPurchase = recvPacket.Get<ResPaymentPurchase>();
				if (resPaymentPurchase.ResultBuyLimit.HasValue)
				{
					Me.CurCharData.AddBuyLimitInfo(new List<BuyLimitInfo>() { resPaymentPurchase.ResultBuyLimit.Value });
				}
				onRecvPacket?.Invoke(recvPacket, resPaymentPurchase);
			}, _onNetError);
		}
	}
}
