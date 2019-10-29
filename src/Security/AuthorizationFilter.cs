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
using System.Web;
using System.Web.Mvc;

using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Security
{
	public class AuthorizationFilter : System.Web.Mvc.IAuthorizationFilter
	{
		#region 成员字段
		private IAuthorizer _authorizer;
		private ICredentialProvider _credentialProvider;
		#endregion

		#region 公共属性
		[Zongsoft.Services.ServiceDependency(IsRequired = true)]
		public IAuthorizer Authorizer
		{
			get
			{
				return _authorizer;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_authorizer = value;
			}
		}

		[Zongsoft.Services.ServiceDependency(IsRequired = true)]
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

		#region 验证方法
		public void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
		{
			//如果当前操作是禁止身份授权验证的则忽略授权验证
			if(AuthenticationUtility.IsSuppressed(filterContext.ActionDescriptor))
				return;

			var attribute = AuthenticationUtility.GetAuthorizationAttribute(filterContext.ActionDescriptor);
			var authorizer = this.Authorizer ?? throw new InvalidOperationException("Missing required authorizer.");
			var principal = filterContext.HttpContext.User as CredentialPrincipal;

			if(principal != null)
			{
				if(!principal.Identity.IsAuthenticated)
				{
					filterContext.Result = new HttpUnauthorizedResult();
					return;
				}

				if(attribute.TryGetRoles(out var roles) && !authorizer.InRoles(principal.Identity.Credential.User.UserId, roles))
				{
					filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
					return;
				}

				if(!string.IsNullOrEmpty(attribute.Schema) && 
				   !authorizer.Authorize(principal.Identity.Credential.User.UserId, attribute.Schema, string.IsNullOrEmpty(attribute.Action) ? filterContext.ActionDescriptor.ActionName : attribute.Action))
				{
					filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
					return;
				}
			}
		}
		#endregion
	}
}
