/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2016 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Web.
 *
 * Zongsoft.Web is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Web is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Web; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

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
