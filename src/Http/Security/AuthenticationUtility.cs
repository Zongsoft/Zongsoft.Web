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
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Http.Security
{
	internal static class AuthenticationUtility
	{
		public static bool IsAuthenticated(System.Security.Principal.IPrincipal principal)
		{
			return (principal != null && principal.Identity != null && principal.Identity.IsAuthenticated);
		}

		internal static AuthorizationAttribute GetAuthorizationAttribute(HttpActionDescriptor actionDescriptor)
		{
			//查找位于Action方法的授权标记
			var attribute = actionDescriptor.GetCustomAttributes<AuthorizationAttribute>(true).FirstOrDefault();

			//查找位于Controller类的授权标记
			if(attribute == null)
				attribute = actionDescriptor.ControllerDescriptor.GetCustomAttributes<AuthorizationAttribute>(true).FirstOrDefault();

			return attribute;
		}

		internal static AuthorizationMode GetAuthorizationMode(HttpActionDescriptor actionDescriptor)
		{
			var attribute = GetAuthorizationAttribute(actionDescriptor);

			if(attribute == null)
				return AuthorizationMode.Anonymous;

			return attribute.Mode;
		}

		internal static AuthorizationAttribute GetAuthorizationAttribute(HttpActionDescriptor actionDescriptor, System.Web.Routing.RequestContext requestContext)
		{
			//查找位于Action方法的授权标记
			var attribute = actionDescriptor.GetCustomAttributes<AuthorizationAttribute>(true).FirstOrDefault();

			if(attribute == null)
			{
				//查找位于Controller类的授权标记
				attribute = actionDescriptor.ControllerDescriptor.GetCustomAttributes<AuthorizationAttribute>(true).FirstOrDefault();

				if(attribute == null)
					return null;

				if(attribute.Mode == AuthorizationMode.Requires)
				{
					if(string.IsNullOrWhiteSpace(attribute.SchemaId))
						return new AuthorizationAttribute(GetSchemaId(actionDescriptor.ControllerDescriptor.ControllerName, requestContext.RouteData.Values["area"] as string)) { ValidatorType = attribute.ValidatorType };
				}

				return attribute;
			}

			if(attribute.Mode == AuthorizationMode.Requires)
			{
				string schemaId = attribute.SchemaId, actionId = attribute.ActionId;

				if(string.IsNullOrWhiteSpace(schemaId))
				{
					var controllerAttribute = actionDescriptor.ControllerDescriptor.GetCustomAttributes<AuthorizationAttribute>(true).FirstOrDefault();

					if(controllerAttribute == null || string.IsNullOrWhiteSpace(controllerAttribute.SchemaId))
						schemaId = GetSchemaId(actionDescriptor.ControllerDescriptor.ControllerName, requestContext.RouteData.Values["area"] as string);
					else
						schemaId = controllerAttribute.SchemaId;
				}

				if(string.IsNullOrWhiteSpace(actionId))
					actionId = actionDescriptor.ActionName;

				return new AuthorizationAttribute(schemaId, actionId) { ValidatorType = attribute.ValidatorType };
			}

			return attribute;
		}

		private static string GetSchemaId(string name, string areaName)
		{
			if(name != null && name.Length > 10 && name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
				name = name.Substring(0, name.Length - 10);

			if(string.IsNullOrWhiteSpace(areaName))
				return name;

			return areaName.Replace('/', '-') + "-" + name;
		}

	}
}
