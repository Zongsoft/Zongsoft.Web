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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http.Filters;

using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Http.Security
{
	public class AuthenticationFilter : IAuthenticationFilter
	{
		#region 常量定义
		private const string HTTP_AUTHORIZATION_SCHEME = "Credential";
		#endregion

		#region 成员字段
		private ICredentialProvider _credentialProvider;
		#endregion

		#region 公共属性
		public bool AllowMultiple
		{
			get
			{
				return false;
			}
		}

		[Services.ServiceDependency]
		public ICredentialProvider CredentialProvider
		{
			get => _credentialProvider;
			set => _credentialProvider = value ?? throw new ArgumentNullException();
		}
		#endregion

		#region 验证实现
		public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			string credentialId = null;

			//优先从HTTP的Authorization头获取凭证编号，如果没有获取成功则从请求的Cookie中获取
			if(context.Request.Headers.Authorization != null && string.Equals(context.Request.Headers.Authorization.Scheme, HTTP_AUTHORIZATION_SCHEME, StringComparison.OrdinalIgnoreCase))
			{
				if(context.Request.Headers.Authorization.Parameter != null)
					credentialId = context.Request.Headers.Authorization.Parameter.Trim();
			}
			else
			{
				var cookie = context.Request.Headers.GetCookies(Zongsoft.Web.Security.AuthenticationUtility.CredentialKey).FirstOrDefault();

				if(cookie != null)
					credentialId = cookie[Zongsoft.Web.Security.AuthenticationUtility.CredentialKey].Value;
			}

			if(string.IsNullOrWhiteSpace(credentialId))
				context.Principal = CredentialPrincipal.Empty;
			else
				context.Principal = new CredentialPrincipal(new CredentialIdentity(credentialId, this.CredentialProvider));

			return Task.CompletedTask;
		}

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			var attribute = Utility.GetAuthorizationAttribute(context.ActionContext.ActionDescriptor);

			if(attribute == null || attribute.Suppressed)
				return Task.CompletedTask;

			var principal = context.ActionContext.RequestContext.Principal;

			if(attribute.ChallengerType != null)
			{
				var challenger = Activator.CreateInstance(attribute.ChallengerType) as IChallenger;

				if(challenger != null)
					principal = context.ActionContext.RequestContext.Principal = challenger.Challenge(principal);
			}

			if(principal == null || !principal.Identity.IsAuthenticated)
			{
				var challenge = new System.Net.Http.Headers.AuthenticationHeaderValue(HTTP_AUTHORIZATION_SCHEME);
				context.Result = new System.Web.Http.Results.UnauthorizedResult(new[] { challenge }, context.Request);
			}

			return Task.CompletedTask;
		}
		#endregion
	}
}
