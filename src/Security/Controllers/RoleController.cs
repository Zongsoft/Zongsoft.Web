/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Web.Http;

using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Security.Controllers
{
	public class RoleController : ApiController
	{
		#region 成员字段
		private IRoleProvider _provider;
		#endregion

		#region 公共属性
		public IRoleProvider Provider
		{
			get
			{
				return _provider;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_provider = value;
			}
		}
		#endregion

		#region 公共方法
		public Role Get(int id)
		{
			var provider = this.EnsureProvider();
			return provider.GetRole(id);
		}

		[HttpGet]
		public IEnumerable<Role> List(string @namespace = null, int pageIndex = 1)
		{
			var provider = this.EnsureProvider();
			return provider.GetAllRoles(@namespace, new Data.Paging(pageIndex));
		}
		#endregion

		#region 私有方法
		private IRoleProvider EnsureProvider()
		{
			var provider = this.Provider;

			if(provider == null)
				throw new MissingMemberException(this.GetType().FullName, "Provider");

			return provider;
		}
		#endregion
	}
}
