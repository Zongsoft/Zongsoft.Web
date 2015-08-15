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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Security
{
	public class AuthenticationFilter : System.Web.Mvc.Filters.IAuthenticationFilter
	{
		#region 成员字段
		private ICertificationProvider _certificationProvider;
		#endregion

		#region 公共属性
		public ICertificationProvider CertificationProvider
		{
			get
			{
				return _certificationProvider;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_certificationProvider = value;
			}
		}
		#endregion

		#region 验证实现
		public void OnAuthentication(AuthenticationContext filterContext)
		{
			var certificationId = AuthenticationUtility.CertificationId;

			if(string.IsNullOrWhiteSpace(certificationId))
				filterContext.Principal = CertificationPrincipal.Empty;
			else
				filterContext.Principal = new CertificationPrincipal(new CertificationIdentity(certificationId, this.CertificationProvider));
		}

		public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
		{
			if(AuthenticationUtility.IsAuthenticated || AuthenticationUtility.GetAuthorizationMode(filterContext.ActionDescriptor) == AuthorizationMode.Disabled)
				return;

			filterContext.Result = new RedirectResult(Zongsoft.Web.Security.AuthenticationUtility.GetLoginUrl() + "?ReturnUrl=" + Uri.EscapeDataString(filterContext.HttpContext.Request.RawUrl));
		}
		#endregion
	}
}
