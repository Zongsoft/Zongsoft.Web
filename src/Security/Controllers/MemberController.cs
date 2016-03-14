using System;
using System.Collections.Generic;
using System.Web.Http;

using Zongsoft.Data;
using Zongsoft.Services;
using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Security.Controllers
{
	public class MemberController : ApiController
	{
		#region 成员字段
		private IMemberProvider _memberProvider;
		#endregion

		#region 公共属性
		[ServiceDependency]
		public IMemberProvider MemberProvider
		{
			get
			{
				return _memberProvider;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_memberProvider = value;
			}
		}
		#endregion

		#region 公共方法
		public virtual object Get(int id)
		{
			return this.MemberProvider.GetMembers(id);
		}

		public virtual int Delete(IEnumerable<Member> members)
		{
			if(members == null)
				return 0;

			return this.MemberProvider.DeleteMembers(members);
		}

		public virtual void Put(int id, IEnumerable<Member> members)
		{
			if(members == null)
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			if(this.MemberProvider.SetMembers(id, members) < 1)
				throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
		}

		public virtual IEnumerable<Member> Post(IEnumerable<Member> members)
		{
			if(members == null)
				throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);

			if(this.MemberProvider.CreateMembers(members) > 0)
				return members;

			throw new HttpResponseException(System.Net.HttpStatusCode.Conflict);
		}
		#endregion
	}
}
