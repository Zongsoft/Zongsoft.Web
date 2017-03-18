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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Filters;

using Zongsoft.Security;
using Zongsoft.Security.Membership;
using System.Threading;
using System.Threading.Tasks;

namespace Zongsoft.Web.Http.Security
{
	public class AuthenticationFilter : IAuthenticationFilter
	{
		#region 成员字段
		private ICredentialProvider _credentialProvider;

		public bool AllowMultiple
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region 公共属性
		[Zongsoft.Services.ServiceDependency]
		public ICredentialProvider CredentialProvider
		{
			get
			{
				if(_credentialProvider == null)
					_credentialProvider = Zongsoft.ComponentModel.ApplicationContextBase.Current.ServiceFactory.Default.Resolve<ICredentialProvider>();

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

		#region 验证实现
		public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			if(context.Request.Headers.Authorization == null || !string.Equals(context.Request.Headers.Authorization.Scheme, "API", StringComparison.OrdinalIgnoreCase))
				return;

			var credentialId = GetAuthenticationKey(context.Request.Headers.Authorization.Parameter);

			if(string.IsNullOrWhiteSpace(credentialId))
				context.Principal = CredentialPrincipal.Empty;
			else
				context.Principal = new CredentialPrincipal(new CredentialIdentity(credentialId, this.CredentialProvider));
		}

		public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			var principal = context.ActionContext.RequestContext.Principal;

			if(AuthenticationUtility.IsAuthenticated(principal) || AuthenticationUtility.GetAuthorizationMode(context.ActionContext.ActionDescriptor) == AuthorizationMode.Anonymous)
				return;

			var challenge = new System.Net.Http.Headers.AuthenticationHeaderValue("API");
			context.Result = new System.Web.Http.Results.UnauthorizedResult(new[] { challenge }, context.Request);
		}
		#endregion

		#region 私有方法
		private static string GetAuthenticationKey(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return null;

			int index = text.IndexOf(':');

			if(index > 0)
				return text.Substring(0, index);

			return null;
		}
		#endregion
	}
}
