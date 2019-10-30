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
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;

using Zongsoft.Services;
using Zongsoft.ComponentModel;
using Zongsoft.Security;
using Zongsoft.Security.Membership;

namespace Zongsoft.Web.Security
{
	public static class AuthenticationUtility
	{
		#region 常量定义
		private const string SECRET_KEY = "__Zongsoft.Security.Authentication:Secret.Key__";
		private const string SECRET_IV = "__Zongsoft.Security.Authentication:Secret.IV__";

		private const string SCENE_KEY = "scene";
		private const string DEFAULT_URL = "/";
		private const string DEFAULT_LOGIN_URL = "/login";
		#endregion

		#region 私有字段
		private static readonly System.Text.RegularExpressions.Regex RETURNURL_REGEX = new System.Text.RegularExpressions.Regex(@"\bReturnUrl\s*=\s*(?<url>[^&]+)", System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
		#endregion

		#region 公共字段
		public static readonly string CredentialKey = "__Zongsoft.Credential__";
		#endregion

		#region 公共方法
		public static bool IsAuthenticated
		{
			get
			{
				return (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated);
			}
		}

		public static string CredentialId
		{
			get
			{
				var cookie = HttpContext.Current.Request.Cookies[CredentialKey];

				if(cookie == null)
					return null;

				return cookie.Value;
			}
		}

		public static string GetScene()
		{
			var scene = HttpContext.Current.Request[SCENE_KEY];

			if(string.IsNullOrWhiteSpace(scene))
			{
				var config = GetAuthenticationElement();

				if(config != null)
					scene = config.Scene;
			}

			return scene;
		}

		public static string GetLoginUrl(string scene = null)
		{
			//根据当前请求来获得指定的应用场景
			if(string.IsNullOrWhiteSpace(scene))
				scene = HttpContext.Current.Request[SCENE_KEY];

			var configuration = GetAuthenticationSceneElement(scene);

			if(configuration == null)
				return DEFAULT_LOGIN_URL;

			return string.IsNullOrWhiteSpace(configuration.LoginUrl) ? DEFAULT_LOGIN_URL : configuration.LoginUrl;
		}

		public static string GetRedirectUrl(string scene = null)
		{
			var url = HttpContext.Current.Request.QueryString["ReturnUrl"];

			if(!string.IsNullOrWhiteSpace(url))
				return Uri.UnescapeDataString(url);

			var referer = HttpContext.Current.Request.UrlReferrer;

			if(referer != null)
			{
				var match = RETURNURL_REGEX.Match(referer.Query);

				if(match.Success)
					return Uri.UnescapeDataString(match.Groups["url"].Value);
			}

			//根据当前请求来获得指定的应用场景
			if(string.IsNullOrWhiteSpace(scene))
				scene = HttpContext.Current.Request[SCENE_KEY];

			var config = GetAuthenticationSceneElement(scene);

			if(config == null)
				return DEFAULT_URL;

			return string.IsNullOrWhiteSpace(config.DefaultUrl) ? DEFAULT_URL : config.DefaultUrl;
		}

		public static Credential Login(IAuthenticator authenticator, ICredentialProvider credentialProvider, string identity, string password, string @namespace, bool isRemember)
		{
			string redirectUrl;
			return Login(authenticator, credentialProvider, identity, password, @namespace, isRemember, out redirectUrl);
		}

		public static Credential Login(IAuthenticator authenticator, ICredentialProvider credentialProvider, string identity, string password, string @namespace, bool isRemember, out string redirectUrl)
		{
			if(authenticator == null)
				throw new ArgumentNullException(nameof(authenticator));

			if(credentialProvider == null)
				throw new ArgumentNullException(nameof(credentialProvider));

			System.Collections.Generic.IDictionary<string, object> parameters = new System.Collections.Generic.Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			//进行身份验证(即验证身份标识和密码是否匹配)
			var user = authenticator.Authenticate(identity, password, @namespace, null, ref parameters);

			//构建用户凭证
			var credential = new Credential(user, AuthenticationUtility.GetScene(), TimeSpan.FromHours(2), parameters);

			//注册用户凭证
			credentialProvider.Register(credential);

			//将注册成功的用户凭证保存到Cookie中
			AuthenticationUtility.SetCredentialCookie(credential, isRemember ? TimeSpan.FromDays(7) : TimeSpan.Zero);

			object redirectObject = null;

			//如果验证事件中显式指定了返回的URL，则使用它所指定的值
			if(parameters != null && parameters.TryGetValue("RedirectUrl", out redirectObject) && redirectObject != null)
				redirectUrl = redirectObject.ToString();
			else //返回重定向的路径中
				redirectUrl = AuthenticationUtility.GetRedirectUrl(credential.Scene);

			return credential;
		}

		public static void Logout(Zongsoft.Security.ICredentialProvider credentialProvider)
		{
			if(credentialProvider == null)
			{
				var applicationContext = Services.ApplicationContext.Current;

				if(applicationContext != null && applicationContext.Services != null)
					credentialProvider = applicationContext.Services.Resolve<ICredentialProvider>();
				else
				{
					var serviceProvider = Services.ServiceProviderFactory.Instance.GetProvider("Security");

					if(serviceProvider != null)
						credentialProvider = serviceProvider.Resolve<ICredentialProvider>();
				}
			}

			if(credentialProvider != null)
			{
				var credentialId = CredentialId;

				if(!string.IsNullOrWhiteSpace(credentialId))
					credentialProvider.Unregister(credentialId);
			}

			HttpContext.Current.Response.Cookies.Remove(CredentialKey);
		}

		public static void SetCredentialCookie(Credential credential)
		{
			SetCredentialCookie(credential, TimeSpan.Zero);
		}

		public static void SetCredentialCookie(Credential credential, TimeSpan duration)
		{
			if(credential == null)
				return;

			var ticket = new System.Web.HttpCookie(CredentialKey, credential.CredentialId);

			if(duration > TimeSpan.Zero)
				ticket.Expires = DateTime.Now + duration;

			HttpContext.Current.Response.Cookies.Set(ticket);
		}

		public static string Decrypt(byte[] data)
		{
			if(data == null || data.Length < 1)
				return null;

			var secretIV = HttpContext.Current.Application[SECRET_IV] as byte[];
			var secretKey = HttpContext.Current.Application[SECRET_KEY] as byte[];

			if(secretIV == null || secretKey == null)
				return null;

			using(var cryptography = RijndaelManaged.Create())
			{
				cryptography.IV = secretIV;
				cryptography.Key = secretKey;

				using(var decryptor = cryptography.CreateDecryptor())
				{
					using(var ms = new System.IO.MemoryStream(data))
					{
						using(var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
						{
							using(var reader = new System.IO.StreamReader(cs, System.Text.Encoding.UTF8))
							{
								return reader.ReadToEnd();
							}
						}
					}
				}
			}
		}

		public static string Encrypt(string text)
		{
			var secretIV = HttpContext.Current.Application[SECRET_IV] as byte[];
			var secretKey = HttpContext.Current.Application[SECRET_KEY] as byte[];

			using(var cryptography = RijndaelManaged.Create())
			{
				if(secretIV == null || secretIV.Length == 0)
				{
					cryptography.GenerateIV();
					HttpContext.Current.Application[SECRET_IV] = secretIV = cryptography.IV;
				}
				else
				{
					cryptography.IV = secretIV;
				}

				if(secretKey == null || secretKey.Length == 0)
				{
					cryptography.GenerateKey();
					HttpContext.Current.Application[SECRET_KEY] = secretKey = cryptography.Key;
				}
				else
				{
					cryptography.Key = secretKey;
				}

				using(var encryptor = cryptography.CreateEncryptor())
				{
					using(var ms = new System.IO.MemoryStream())
					{
						using(var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
						{
							var bytes = System.Text.Encoding.UTF8.GetBytes(text);
							cs.Write(bytes, 0, bytes.Length);
						}

						return System.Convert.ToBase64String(ms.ToArray());
					}
				}
			}
		}
		#endregion

		#region 内部方法
		internal static bool IsSuppressed(ActionDescriptor actionDescriptor, out AuthorizationAttribute attribute)
		{
			attribute = (AuthorizationAttribute)(actionDescriptor.GetCustomAttributes(typeof(AuthorizationAttribute), true).FirstOrDefault() ??
				   actionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(AuthorizationAttribute), true).FirstOrDefault());

			return attribute == null ? true : attribute.Suppressed;
		}
		#endregion

		#region 私有方法
		private static Configuration.AuthenticationElement GetAuthenticationElement()
		{
			var applicationContext = Zongsoft.Services.ApplicationContext.Current;

			if(applicationContext != null && applicationContext.Options != null)
				return applicationContext.Options.GetOptionValue("/Security/Authentication") as Configuration.AuthenticationElement;

			return null;
		}

		private static Configuration.AuthenticationSceneElement GetAuthenticationSceneElement(string scene, Configuration.AuthenticationElement config = null)
		{
			var configuration = config ?? GetAuthenticationElement();

			if(configuration == null || configuration.Scenes.Count < 1)
				return null;

			//如果当前请求未指定应用场景，则使用验证配置节中设置的默认场景
			if(string.IsNullOrWhiteSpace(scene))
				scene = configuration.Scene;

			if(string.IsNullOrWhiteSpace(scene))
				return configuration.Scenes[0];
			else
				return configuration.Scenes[scene];
		}
		#endregion
	}
}
