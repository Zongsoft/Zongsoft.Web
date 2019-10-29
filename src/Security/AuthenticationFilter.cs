/*
 *   _____                                ______
 *  /_   /  ____  ____  ____  _________  / __/ /_
 *    / /  / __ \/ __ \/ __ \/ ___/ __ \/ /_/ __/
 *   / /__/ /_/ / / / / /_/ /\_ \/ /_/ / __/ /_
 *  /____/\____/_/ /_/\__  /____/\____/_/  \__/
 *                   /____/
 *
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
 *
 * Copyright (C) 2015-2019 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Web.Mvc;
using System.Web.Mvc.Filters;

using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Security
{
	public class AuthenticationFilter : System.Web.Mvc.Filters.IAuthenticationFilter
	{
		#region 成员字段
		private ICredentialProvider _credentialProvider;
		#endregion

		#region 公共属性
		[Zongsoft.Services.ServiceDependency]
		public ICredentialProvider CredentialProvider
		{
			get
			{
				return _credentialProvider;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_credentialProvider = value;
			}
		}
		#endregion

		#region 验证实现
		public void OnAuthentication(System.Web.Mvc.Filters.AuthenticationContext filterContext)
		{
			var credentialId = AuthenticationUtility.CredentialId;

			if(string.IsNullOrWhiteSpace(credentialId))
				filterContext.Principal = CredentialPrincipal.Empty;
			else
				filterContext.Principal = new CredentialPrincipal(new CredentialIdentity(credentialId, this.CredentialProvider));
		}

		public void OnAuthenticationChallenge(System.Web.Mvc.Filters.AuthenticationChallengeContext filterContext)
		{
			//如果当前操作是禁止身份验证的则退出
			if(AuthenticationUtility.IsSuppressed(filterContext.ActionDescriptor, out var attribute))
				return;

			var principal = filterContext.HttpContext.User;

			if(attribute.ChallengerType != null)
			{
				var challenger = Activator.CreateInstance(attribute.ChallengerType) as IChallenger;

				if(challenger != null)
					principal = filterContext.HttpContext.User = challenger.Challenge(principal);
			}

			if(principal == null || !principal.Identity.IsAuthenticated)
			{
				var url = Web.Utility.RepairQueryString(AuthenticationUtility.GetLoginUrl(), filterContext.HttpContext.Request.Url.Query);
				url = Web.Utility.RepairQueryString(url, "?ReturnUrl=" + Uri.EscapeDataString(filterContext.HttpContext.Request.RawUrl));
				filterContext.Result = new RedirectResult(url);
			}
		}
		#endregion
	}
}
