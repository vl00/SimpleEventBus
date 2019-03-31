using System;
using System.Threading.Tasks;

namespace SimpleBus
{
	public interface IReqEntity<TRes> { }
	
	public interface IReqResHandler<TReq, TRes> where TReq : IReqEntity<TRes>
	{
		Task<TRes> Handle(TReq reqEntity);
	}
}