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
		#region 公共属性
		public bool AllowMultiple
		{
			get => true;
		}

		[Services.ServiceDependency(IsRequired = true)]
		public IAuthorizer Authorizer
		{
			get; set;
		}
		#endregion

		#region 授权校验
		public async Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
		{
			//如果当前操作是禁止身份授权验证的，则返回下一个步骤
			if(Utility.IsSuppressed(actionContext.ActionDescriptor, out var attribute))
				return await continuation();

			var authorizer = this.Authorizer ?? throw new InvalidOperationException("Missing required authorizer.");
			var principal = actionContext.RequestContext.Principal as CredentialPrincipal;

			if(principal != null)
			{
				if(!principal.Identity.IsAuthenticated)
					return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);

				if(attribute.TryGetRoles(out var roles) && !authorizer.InRoles(principal.Identity.Credential.User.UserId, roles))
					return new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
					{
						Content = new StringContent(
							Resources.ResourceUtility.GetString("Text.Forbidden.NotInRoles", string.Join(",", roles)),
							System.Text.Encoding.UTF8, "text/plain")
					};

				if(!string.IsNullOrEmpty(attribute.Schema))
				{
					var action = actionContext.GetSchemaAction(attribute.Schema, attribute.Action, out var schema);

					if(!authorizer.Authorize(principal.Identity.Credential.User.UserId, attribute.Schema, action != null ? action.Name : attribute.Action))
						return new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
						{
							Content = new StringContent(
								Resources.ResourceUtility.GetString("Text.Forbidden.NotAuthorized", schema != null ? schema.Title : attribute.Schema, action != null ? action.Title : attribute.Action),
								System.Text.Encoding.UTF8, "text/plain"),
						};
				}
			}

			return await continuation();
		}
		#endregion
	}
}
