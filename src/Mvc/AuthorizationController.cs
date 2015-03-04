/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2013 Zongsoft Corporation <http://www.zongsoft.com>
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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
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
using System.Web.Mvc;
using System.Web.Security;

using Zongsoft.ComponentModel;
using Zongsoft.Services;
using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Mvc
{
	[Obsolete]
	[HandleError]
	public class AuthorizationController : Controller
	{
		#region 成员变量
		private IServiceProviderFactory _serviceProviderFactory;
		private static IDictionary<string, object> _principals = new Dictionary<string, object>();
		private static IDictionary<string, object> _authorizations = new Dictionary<string, object>();
		private static object _lock = new object();
		#endregion

		#region 构造函数
		public AuthorizationController(IServiceProviderFactory serviceProviderFactory)
		{
			_serviceProviderFactory = serviceProviderFactory;
		}
		#endregion

		#region 属性
		/// <summary>
		/// 当前用户的所有者
		/// </summary>
		protected object Principal
		{
			get
			{
				lock(_lock)
				{
					if(User.Identity.IsAuthenticated && _principals.ContainsKey(User.Identity.Name))
						return _principals[User.Identity.Name];
				}
				return null;
			}
		}

		/// <summary>
		/// 当前用户的授权信息
		/// </summary>
		protected object Authorizations
		{
			get
			{
				lock(_lock)
				{
					if(User.Identity.IsAuthenticated && _authorizations.ContainsKey(User.Identity.Name))
						return _authorizations[User.Identity.Name];
				}
				return null;
			}
		}
		/// <summary>
		/// 设置当前用户的所有者，如果用户名为空，则替换缓存中所有equals为true的principal，如果所有者为空，则移除指定用户的所有者
		/// </summary>
		/// <param name="userName">当前登录的用户名</param>
		/// <param name="principal">当前登录用户的所有者</param>
		protected void SetPrincipal(string userName, object principal)
		{
			lock(_lock)
			{
				if(!string.IsNullOrEmpty(userName))
				{
					if(_principals.ContainsKey(userName))
						if(principal != null)
							_principals[userName] = principal;
						else
							_principals.Remove(userName);
					else if(_principals != null)
						_principals.Add(userName, principal);
				}
				else if(principal != null)
				{
					foreach(var item in _principals)
					{
						if(item.Value.Equals(principal))
						{
							_principals[item.Key] = principal;
							break;
						}
					}
				}
			}
		}
		/// <summary>
		/// 设置当前用户的权限，如果用户名为空，则替换缓存中所有equals为true的authorizations，如果权限为空，则移除指定用户的authorizations
		/// </summary>
		/// <param name="userName">当前登录的用户名</param>
		/// <param name="Authorizations">当前登录用户的权限</param>
		protected void SetAuthorizations(string userName, object authorizations)
		{
			lock(_lock)
			{
				if(!string.IsNullOrEmpty(userName))
				{
					if(_authorizations.ContainsKey(userName))
						if(authorizations != null)
							_authorizations[userName] = authorizations;
						else
							_authorizations.Remove(userName);
					else if(_authorizations != null)
						_authorizations.Add(userName, authorizations);
				}
				else if(authorizations != null)
				{
					foreach(var item in _authorizations)
					{
						if(item.Value.Equals(authorizations))
						{
							_authorizations[item.Key] = authorizations;
							break;
						}
					}
				}
			}
		}
		#endregion

		protected override void OnAuthorization(AuthorizationContext context)
		{
			//获取当前Action方法的自定义属性中是否包含验证特性(AuthenticationAttribute)
			var attributes = context.ActionDescriptor.GetCustomAttributes(typeof(AuthenticationAttribute), true);

			if(attributes != null && attributes.Length > 0)
			{
				AuthenticationAttribute authenticationAttribute = (AuthenticationAttribute)attributes[0];

				//如果当前Action方法指定了验证特性，并表明要忽略验证则返回
				if(authenticationAttribute.IgnoreAuthenticate)
					return;
			}

			if(!User.Identity.IsAuthenticated)
			{
				context.Result = this.Redirect(string.Format("{0}?ReturnUrl={1}", FormsAuthentication.LoginUrl, Uri.EscapeDataString(Request.RawUrl)));
				return;
			}

			if(User.Identity.Name.Equals("admin", StringComparison.CurrentCultureIgnoreCase))
				return;

			var schemaProvider = _serviceProviderFactory.Default.Resolve<ISchemaProvider>();
			var schemas = schemaProvider.GetSchemas();
			var controllerName = context.ActionDescriptor.ControllerDescriptor.ControllerName;
			var actionName = context.ActionDescriptor.ActionName;

			object areaValue = null;
			string schemaId = string.Empty;

			if(context.RouteData.DataTokens.TryGetValue("area", out areaValue))
			{
				if(areaValue != null)
				{
					string areaText = areaValue.ToString();

					if(!string.IsNullOrWhiteSpace(areaText))
						schemaId = areaText.Trim('/').Replace('/', '-') + '-';
				}
			}

			schemaId += controllerName;
			actionName = schemaId + '-' + actionName;

			if(schemas == null || !schemas.Select(p => p.Name).Contains(schemaId, StringComparer.OrdinalIgnoreCase) || !schemas.FirstOrDefault(p => p.Name == schemaId).Actions.Select(p => p.Name).Contains(actionName, StringComparer.OrdinalIgnoreCase))
				return;

			//设置是否授权通过
			var authorized = false;

			//获取授权服务
			var authorization = _serviceProviderFactory.Default.Resolve<IAuthorization>();

			if(authorization != null)
				authorized = authorization.IsAuthorized(context.HttpContext.User.Identity.Name, schemaId, actionName);

			if(!authorized)
				throw new MethodAccessException();

		}
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			try
			{
				ViewData["CurrentUserAuthorizations"] = Authorizations;
			}
			catch
			{ }
			base.OnActionExecuting(filterContext);
		}
	}
}
