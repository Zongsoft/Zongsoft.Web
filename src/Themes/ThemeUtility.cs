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
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Web;
using System.Web.UI;
using System.Text.RegularExpressions;

namespace Zongsoft.Web.Themes
{
	public class ThemeUtility
	{
		#region 公共字段
		public static readonly string ThemeName = "__theme__";
		#endregion

		#region 私有变量
		private static readonly Regex _regex = new Regex(@"[a-zA-Z]+://.+", (RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture));
		private static ConcurrentDictionary<Page, HashSet<string>> _pageLinks = new ConcurrentDictionary<Page, HashSet<string>>();
		#endregion

		#region 公共方法
		public static string[] GetThemeNames(Page page)
		{
			if(page == null)
				throw new ArgumentNullException("page");

			return GetThemeNames(page.MapPath("~/themes"));
		}

		public static string[] GetThemeNames(HttpContext context)
		{
			if(context == null)
				throw new ArgumentNullException("context");

			return GetThemeNames(context.Server.MapPath("~/themes"));
		}

		public static string[] GetThemeNames(HttpContextBase context)
		{
			if(context == null)
				throw new ArgumentNullException("context");

			return GetThemeNames(context.Server.MapPath("~/themes"));
		}

		public static string[] GetThemeNames(string path)
		{
			var directories = Directory.GetDirectories(path);

			if(directories.Length < 1)
				return directories;

			var names = new string[directories.Length];

			for(int i = 0; i < names.Length; i++)
			{
				names[i] = Path.GetFileName(directories[i]);
			}

			return names;
		}

		public static string GetCurrentThemeName(Page page)
		{
			if(page == null)
				throw new ArgumentNullException("page");

			return page.Request[ThemeName] ?? "default";
		}

		public static string GetCurrentThemeName(HttpContext context)
		{
			if(context == null)
				throw new ArgumentNullException("context");

			return context.Request[ThemeName] ?? "default";
		}

		public static string GetCurrentThemeName(HttpContextBase context)
		{
			if(context == null)
				throw new ArgumentNullException("context");

			switch(context.Request.HttpMethod.ToLowerInvariant())
			{
				case "put":
				case "post":
					if(context.Request.QueryString[ThemeName] != null)
						return context.Request.QueryString[ThemeName];
					if(context.Request.Form[ThemeName] != null)
						return context.Request.Form[ThemeName];
					if(context.Request.Cookies[ThemeName] != null)
						return context.Request.Cookies[ThemeName].Value;
					if(context.Request.ServerVariables[ThemeName] != null)
						return context.Request.ServerVariables[ThemeName];
					return "Default";
				default:
					if(context.Request.Cookies[ThemeName] != null)
						return context.Request.Cookies[ThemeName].Value ?? "default";
					else
						return "default";
			}
		}

		public static bool GenerateTheme(Page page)
		{
			if(page == null || page.Request == null)
				return false;

			string themeName = page.Request[ThemeName];

			if(string.IsNullOrWhiteSpace(themeName))
				themeName = "default";
			else
				themeName = themeName.Trim().Replace("/", "");

			return GenerateTheme(page, themeName);
		}

		public static bool GenerateTheme(Page page, string themeName)
		{
			if(page == null || string.IsNullOrWhiteSpace(themeName))
				return false;

			var resolver = Zongsoft.Web.Themes.ThemeResolver.GetResolver(page.MapPath("~/themes"));

			if(resolver == null)
				return false;

			var theme = resolver.Resolve(themeName);

			if(theme == null)
				return false;

			try
			{
				foreach(var include in theme.Includes)
				{
					GenerateComponent(include.Target, page);
				}

				page.Response.SetCookie(new HttpCookie(ThemeName, themeName));

				//返回成功
				return true;
			}
			finally
			{
				HashSet<string> temp;
				_pageLinks.TryRemove(page, out temp);
			}
		}
		#endregion

		#region 私有方法
		private static void GenerateComponent(ComponentElement component, Page page)
		{
			if(component == null)
				return;

			if(component.Dependencies.Count > 0)
			{
				foreach(var dependency in component.Dependencies)
				{
					var target = dependency.GetDependenceObject();

					if(target != null)
						GenerateComponent(target, page);
				}
			}

			if(component.Files.Count > 0)
			{
				foreach(var file in component.Files)
				{
					GenerateFileLink(GetFileUrl(file.Name, component.Theme.Name), page);
				}
			}
		}

		private static void GenerateFileLink(string url, Page page)
		{
			if(page == null || page.Header == null || string.IsNullOrWhiteSpace(url))
				return;

			var links = _pageLinks.GetOrAdd(page, new HashSet<string>(StringComparer.Ordinal));

			lock(links)
			{
				//如果添加操作返回假则表示当前Url已经存在，则退出
				if(!links.Add(url.ToLowerInvariant()))
					return;
			}

			if(url.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
			{
				var link = new System.Web.UI.HtmlControls.HtmlLink()
				{
					Href = url,
				};

				link.Attributes.Add("type", "text/css");
				link.Attributes.Add("rel", "Stylesheet");

				page.Header.Controls.Add(link);
			}
			else if(url.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
			{
				var script = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
				script.Attributes.Add("type", "text/javascript");
				script.Attributes.Add("src", page.ResolveClientUrl(url));
				page.Header.Controls.Add(script);
			}
		}

		private static string GetFileUrl(string fileName, string themeName)
		{
			if(string.IsNullOrWhiteSpace(fileName))
				return null;

			fileName = fileName.Trim();

			if(fileName.StartsWith("/") || _regex.IsMatch(fileName))
				return fileName;

			return "~/themes/" + themeName + "/" + fileName;
		}
		#endregion
	}
}
