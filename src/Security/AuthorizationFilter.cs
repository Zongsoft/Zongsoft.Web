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

namespace Zongsoft.Web.Security
{
	public class AuthorizationFilter : System.Web.Mvc.IAuthorizationFilter
	{
		#region 成员字段
		private IAuthorization _authorization;
		private ICertificationProvider _certificationProvider;
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

		#region 验证方法
		public void OnAuthorization(AuthorizationContext filterContext)
		{
			string schemaId, actionId;
			ICertificationValidator validator;

			//获取授权验证的方式
			var mode = AuthenticationUtility.GetAuthorizationMode(filterContext.ActionDescriptor, filterContext.RequestContext, out schemaId, out actionId, out validator);

			//忽略授权验证
			if(mode == AuthorizationMode.Disabled)
				return;

			//如果连身份验证都未通过则返回身份验证失败并退出
			if(!AuthenticationUtility.IsAuthenticated)
			{
				filterContext.Result = new HttpUnauthorizedResult();
				return;
			}

			//进行凭证验证(确保凭证是未过期并且可用的)，凭证验证失败则退出
			if(!this.ValidateCertification(filterContext, validator))
				return;

			if(mode == AuthorizationMode.Required)
			{
				//获取授权验证服务
				var authorization = this.Authorization;

				if(authorization == null)
					throw new MissingMemberException(this.GetType().FullName, "Authorization");

				//执行授权验证操作，如果验证失败则返回验证失败的响应
				if(!authorization.Authorize(((CertificationPrincipal)filterContext.HttpContext.User).Identity.Certification.User.UserId, schemaId, actionId))
					filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
			}
		}
		#endregion

		#region 私有方法
		private bool ValidateCertification(AuthorizationContext filterContext, ICertificationValidator validator)
		{
			//获取凭证提供者服务
			var certificationProvider = this.CertificationProvider;

			if(certificationProvider == null)
				throw new MissingMemberException(this.GetType().FullName, "CertificationProvider");

			//获取当前的安全主体
			var principal = filterContext.HttpContext.User as CertificationPrincipal;

			if(principal == null || principal.Identity == null || !certificationProvider.Validate(principal.Identity.CertificationId))
			{
				filterContext.Result = new HttpUnauthorizedResult();
				return false;
			}

			if(validator != null && !validator.Validate(principal.Identity.Certification))
			{
				filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
				return false;
			}

			return true;
		}
		#endregion
	}
}
