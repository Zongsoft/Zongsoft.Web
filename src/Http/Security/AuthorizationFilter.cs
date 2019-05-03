/*
 *   _____                                ______
 *  /_   /  ____  ____  ____  _________  / __/ /_
 *    / /  / __ \/ __ \/ __ \/ ___/ __ \/ /_/ __/
 *   / /__/ /_/ / / / / /_/ /\_ \/ /_/ / __/ /_
 *  /____/\____/_/ /_/\__  /____/\____/_/  \__/
 *                   /____/
 *
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2016-2019 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http.Controllers;

using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Http.Security
{
	public class AuthorizationFilter : System.Web.Http.Filters.IAuthorizationFilter
	{
		#region 成员字段
		private IAuthorization _authorization;
		#endregion

		#region 公共属性
		public bool AllowMultiple
		{
			get
			{
				return true;
			}
		}

		[Services.ServiceDependency]
		public IAuthorization Authorization
		{
			get => _authorization;
			set => _authorization = value ?? throw new ArgumentNullException();
		}
		#endregion

		#region 授权校验
		public async Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
		{
			var principal = actionContext.RequestContext.Principal;

			var response = await continuation();
			return response;
		}
		#endregion
	}
}
