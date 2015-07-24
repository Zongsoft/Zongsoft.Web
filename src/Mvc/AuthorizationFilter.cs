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

using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Mvc
{
	public class AuthorizationFilter : System.Web.Mvc.IAuthorizationFilter
	{
		#region 成员字段
		private Zongsoft.Security.Membership.IAuthorization _authorization;
		#endregion

		#region 公共属性
		public IAuthorization Authorization
		{
			get
			{
				return _authorization;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_authorization = value;
			}
		}
		#endregion

		#region 验证方法
		public void OnAuthorization(AuthorizationContext filterContext)
		{
			string schemaId, actionId;

			//获取授权验证的方式
			var mode = this.GetAuthorizationMode(filterContext, out schemaId, out actionId);

			switch(mode)
			{
				case AuthorizationMode.Ignore: //忽略授权验证
					return;
				case AuthorizationMode.Licensee: //身份验证(即需要验证当前操作者是否为非匿名用户)

					//如果当前操作为匿名操作（即操作者没有登录验证），则返回验证失败的响应
					if(!this.IsAuthenticated(filterContext))
						filterContext.Result = new HttpUnauthorizedResult();

					return;
			}

			//获取授权验证服务
			var authorization = this.Authorization;

			if(authorization == null)
				throw new MissingMemberException(this.GetType().FullName, "Authorization");

			//执行授权验证操作，如果验证失败则返回验证失败的响应
			if(!authorization.IsAuthorized(((CertificationPrincipal)filterContext.HttpContext.User).Identity.Certification.User.UserId, schemaId, actionId))
				filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
		}
		#endregion

		#region 虚拟方法
		protected virtual bool IsAuthenticated(AuthorizationContext filterContext)
		{
			var principal = filterContext.HttpContext.User as Zongsoft.Security.CertificationPrincipal;

			if(principal == null || principal.Identity == null)
				return false;

			return principal.Identity.IsAuthenticated;
		}
		#endregion

		#region 私有方法
		private AuthorizationMode GetAuthorizationMode(AuthorizationContext filterContext, out string schemaId, out string actionId)
		{
			schemaId = null;
			actionId = null;

			//查找位于Action方法的授权标记
			var attribute = (AuthorizationAttribute)filterContext.ActionDescriptor.GetCustomAttributes(typeof(Zongsoft.Security.Membership.AuthorizationAttribute), true).FirstOrDefault();

			if(attribute == null)
			{
				//查找位于Controller类的授权标记
				attribute = (AuthorizationAttribute)Attribute.GetCustomAttribute(filterContext.Controller.GetType(), typeof(Zongsoft.Security.Membership.AuthorizationAttribute), true);

				if(attribute == null)
					return AuthorizationMode.Ignore;

				schemaId = string.IsNullOrWhiteSpace(attribute.SchemaId) ? this.GetSchemaId(filterContext.Controller.GetType().Name, filterContext.RouteData.Values["area"] as string) : attribute.SchemaId;
				actionId = filterContext.ActionDescriptor.ActionName;

				return attribute.Mode;
			}

			schemaId = attribute.SchemaId;
			actionId = string.IsNullOrWhiteSpace(attribute.ActionId) ? filterContext.ActionDescriptor.ActionName : attribute.ActionId;

			if(string.IsNullOrWhiteSpace(schemaId))
			{
				var controllerAttribute = (AuthorizationAttribute)Attribute.GetCustomAttribute(filterContext.Controller.GetType(), typeof(Zongsoft.Security.Membership.AuthorizationAttribute), true);

				if(controllerAttribute == null || string.IsNullOrWhiteSpace(controllerAttribute.SchemaId))
					schemaId = this.GetSchemaId(filterContext.Controller.GetType().Name, filterContext.RouteData.Values["area"] as string);
				else
					schemaId = controllerAttribute.SchemaId;
			}

			return attribute.Mode;
		}

		private string GetSchemaId(string name, string areaName)
		{
			if(name != null && name.Length > 10 && name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
				name = name.Substring(0, name.Length - 10);

			if(string.IsNullOrWhiteSpace(areaName))
				return name;

			return areaName.Replace('/', '-') + "-" + name;
		}
		#endregion
	}
}
