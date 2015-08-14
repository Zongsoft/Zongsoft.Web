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
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;

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
		public static readonly string CertificationKey = ".Zongsoft.Credential";
		#endregion

		#region 公共方法
		public static bool IsAuthenticated
		{
			get
			{
				return (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated);
			}
		}

		public static string CertificationId
		{
			get
			{
				var cookie = HttpContext.Current.Request.Cookies[CertificationKey];

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

		public static string Login(IAuthentication authentication, ICertificationProvider certificationProvider, string identity, string password, string @namespace, bool isRemember)
		{
			if(authentication == null)
				throw new ArgumentNullException("authentication");

			if(certificationProvider == null)
				throw new ArgumentNullException("certificationProvider");

			//进行身份验证(即验证身份标识和密码是否匹配)
			var result = authentication.Authenticate(identity, password, @namespace);

			//注册用户凭证
			var certification = certificationProvider.Register(result.User, AuthenticationUtility.GetScene(), (result.HasExtendedProperties ? result.ExtendedProperties : null));

			//将注册成功的用户凭证保存到Cookie中
			AuthenticationUtility.SetCertificationCookie(certification, isRemember ? TimeSpan.FromDays(7) : TimeSpan.Zero);

			//返回重定向的路径中
			return AuthenticationUtility.GetRedirectUrl(certification.Scene);
		}

		public static void Logout(Zongsoft.Security.ICertificationProvider certificationProvider)
		{
			if(certificationProvider == null)
			{
				var applicationContext = Zongsoft.ComponentModel.ApplicationContextBase.Current;

				if(applicationContext != null && applicationContext.ServiceFactory != null)
				{
					var serviceProvider = applicationContext.ServiceFactory.GetProvider("Security");

					if(serviceProvider != null)
						certificationProvider = serviceProvider.Resolve<ICertificationProvider>();
				}
			}

			if(certificationProvider != null)
			{
				var certificationId = CertificationId;

				if(!string.IsNullOrWhiteSpace(certificationId))
					certificationProvider.Unregister(certificationId);
			}

			HttpContext.Current.Response.Cookies.Remove(CertificationKey);
		}

		public static void SetCertificationCookie(Certification certification)
		{
			SetCertificationCookie(certification, TimeSpan.Zero);
		}

		public static void SetCertificationCookie(Certification certification, TimeSpan duration)
		{
			if(certification == null)
				return;

			var ticket = new System.Web.HttpCookie(CertificationKey, certification.CertificationId);

			if(duration > TimeSpan.Zero)
				ticket.Expires = DateTime.Now + duration;

			HttpContext.Current.Response.Cookies.Set(ticket);
		}
		#endregion

		#region 私有方法
		private static Configuration.AuthenticationElement GetAuthenticationElement()
		{
			var applicationContext = Zongsoft.ComponentModel.ApplicationContextBase.Current;

			if(applicationContext != null && applicationContext.OptionManager != null)
				return applicationContext.OptionManager.GetOptionObject("/Security/Authentication") as Configuration.AuthenticationElement;

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
