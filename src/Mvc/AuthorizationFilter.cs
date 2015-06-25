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
			var schemaId = string.Empty;
			var actionId = string.Empty;

			var controllerAttribute = (AuthorizationAttribute)Attribute.GetCustomAttribute(filterContext.Controller.GetType(), typeof(Zongsoft.Security.Membership.AuthenticationAttribute), true);

			if(controllerAttribute == null || string.IsNullOrWhiteSpace(controllerAttribute.SchemaId))
				schemaId = this.GetSchemaId(filterContext.Controller.GetType().Name);
			else
				schemaId = controllerAttribute.SchemaId;

			var actionAttribute = (AuthorizationAttribute)filterContext.ActionDescriptor.GetCustomAttributes(typeof(Zongsoft.Security.Membership.AuthenticationAttribute), true).FirstOrDefault();

			if(actionAttribute != null)
			{
				if(!string.IsNullOrWhiteSpace(actionAttribute.SchemaId))
					schemaId = actionAttribute.SchemaId;

				actionId = string.IsNullOrWhiteSpace(actionAttribute.ActionId) ? filterContext.ActionDescriptor.ActionName : actionAttribute.ActionId;
			}

			if(controllerAttribute == null && actionAttribute == null)
				return;

			var principal = filterContext.HttpContext.User as Zongsoft.Security.CertificationPrincipal;

			if(principal == null || principal.Identity == null || (!principal.Identity.IsAuthenticated))
				this.Failed(filterContext, "没有通过身份验证，拒绝访问。");

			var authorization = _authorization;

			if(authorization == null)
				this.Failed(filterContext, "没有可用的授权验证程序，拒绝访问。");

			if(!authorization.IsAuthorized(principal.Identity.CertificationId, schemaId, actionId))
				this.Failed(filterContext, "权限不够，访问被拒绝。");
		}
		#endregion

		#region 私有方法
		private string GetSchemaId(string text)
		{
			if(text != null && text.Length > 10 && text.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
				return text.Substring(0, text.Length - 10);

			return text;
		}

		private void Failed(AuthorizationContext filterContext, string message)
		{
			filterContext.Result = new HttpUnauthorizedResult(message);
		}
		#endregion
	}
}
