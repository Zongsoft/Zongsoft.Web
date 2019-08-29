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
		private IMemberProvider _memberProvider;
		private ICredentialProvider _credentialProvider;
		#endregion

		#region 公共属性
		[Zongsoft.Services.ServiceDependency]
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

		[Zongsoft.Services.ServiceDependency]
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

		#region 验证方法
		public void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
		{
			//获取授权验证的声明描述
			var attribute = AuthenticationUtility.GetAuthorizationAttribute(filterContext.ActionDescriptor, filterContext.RequestContext);

			//忽略授权验证
			if(attribute == null || attribute.Mode == AuthorizationMode.Anonymous)
				return;

			//如果连身份验证都未通过则返回身份验证失败并退出
			if(!AuthenticationUtility.IsAuthenticated)
			{
				filterContext.Result = new HttpUnauthorizedResult();
				return;
			}

			//进行凭证验证(确保凭证是未过期并且可用的)
			filterContext.Result = this.ValidateCredential(filterContext.HttpContext, filterContext.HttpContext.User as CredentialPrincipal, attribute.GetValidator());

			//如果返回的结果不为空则退出
			if(filterContext.Result != null)
				return;

			//获取当前请求对应的用户编号
			var userId = ((CredentialPrincipal)filterContext.HttpContext.User).Identity.Credential.User.UserId;

			switch(attribute.Mode)
			{
				case AuthorizationMode.Identity:
					if(attribute.Roles != null && attribute.Roles.Length > 0)
					{
						//如果当前用户即不属于系统管理员也不属于指定角色的成员，则返回验证失败的响应
						if(!this.MemberProvider.InRoles(userId, "Administrators") && !this.MemberProvider.InRoles(userId, attribute.Roles))
							filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
					}
					break;
				case AuthorizationMode.Requires:
					//执行授权验证操作，如果验证失败则返回验证失败的响应
					if(!this.Authorizer.Authorize(userId, attribute.SchemaId, attribute.ActionId))
						filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
					break;
			}
		}
		#endregion

		#region 私有方法
		private ActionResult ValidateCredential(HttpContextBase httpContext, CredentialPrincipal principal, Common.IValidator<Credential> validator)
		{
			//获取凭证提供者服务
			var credentialProvider = this.CredentialProvider;

			if(credentialProvider == null)
				throw new MissingMemberException(this.GetType().FullName, "CredentialProvider");

			//如果指定的主体为空，或对应的凭证编号不存在，或对应的凭证已过期则返回未验证结果
			if(principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
				return new HttpUnauthorizedResult();

			//使用凭证验证器对指定的凭证进行验证，如果验证失败
			if(validator != null && !validator.Validate(principal.Identity.Credential))
			{
				//如果当前请求的路径是主页，并且是从登录页面跳转而来的返回特定的结果
				if(httpContext.Request.Path == "/" && httpContext.Request.UrlReferrer != null && string.Equals(httpContext.Request.UrlReferrer.LocalPath, AuthenticationUtility.GetLoginUrl(), StringComparison.OrdinalIgnoreCase))
					return new HttpStatusCodeResult(444, "Invalid Credential");

				return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
			}

			//返回空，表示成功
			return null;
		}
		#endregion
	}
}
