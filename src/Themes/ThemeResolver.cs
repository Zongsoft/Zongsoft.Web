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
using System.Linq;
using System.Text;
using System.Xml;

namespace Zongsoft.Web.Themes
{
	public class ThemeResolver
	{
		#region 静态变量
		private static ConcurrentDictionary<string, ThemeResolver> _resolvers;
		#endregion

		#region 成员字段
		private string _path;
		#endregion

		#region 构造函数
		public ThemeResolver(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			_path = path.Trim();
		}
		#endregion

		#region 公共方法
		public string[] GetNames()
		{
			var directories = Directory.GetDirectories(_path);

			if(directories.Length < 1)
				return directories;

			var names = new string[directories.Length];

			for(int i = 0; i < names.Length; i++)
			{
				names[i] = Path.GetFileName(directories[i]);
			}

			return names;
		}

		public Theme Resolve(string name)
		{
			name = string.IsNullOrWhiteSpace(name) ? "default" : name.Trim();

			if(!Directory.Exists(Path.Combine(_path, name)))
				return null;

			return this.ResolveThemeFile(name);
		}
		#endregion

		#region 私有方法
		private Theme ResolveThemeFile(string themeName)
		{
			if(string.IsNullOrWhiteSpace(themeName))
				return null;

			string fileName = Path.Combine(_path, themeName, "theme.xml");

			if(!File.Exists(fileName))
				return null;

			Theme theme = new Theme(themeName);

			using(var reader = XmlReader.Create(fileName, new XmlReaderSettings()
			{
				CloseInput = true,
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true,
				ValidationType = ValidationType.None,
			}))
			{
				//移到根节点
				reader.MoveToContent();

				theme.Title = reader.GetAttribute("title");
				theme.Description = reader.GetAttribute("description");

				if(string.IsNullOrWhiteSpace(theme.Title))
					theme.Title = themeName;

				while(reader.Read())
				{
					if(reader.NodeType != XmlNodeType.Element)
						continue;

					switch(reader.Name)
					{
						case "component":
							this.ResolveComponentElement(reader, theme);
							break;
						case "content":
							this.ResolveContentElement(reader, theme);
							break;
						case "include":
							theme.Includes.Add(new IncludeElement(reader.GetAttribute("name")));
							break;
						default:
							throw new ThemeException("Invalid '" + reader.Name + "' of element in this '" + fileName + "' theme file.");
					}
				}
			}

			return theme;
		}

		private void ResolveComponentElement(XmlReader reader, Theme theme)
		{
			reader = reader.ReadSubtree();

			if(reader != null && reader.ReadState == ReadState.Initial)
				reader.Read();

			int depth = reader.Depth;
			ComponentElement component = new ComponentElement(reader.GetAttribute("name"));

			while(reader.Read())
			{
				if(reader.NodeType == XmlNodeType.Element)
				{
					switch(reader.Name)
					{
						case "file":
							component.Files.Add(new FileElement(reader.GetAttribute("name").Replace('\\', '/').TrimStart('/')));
							break;
						case "dependencies":
							this.ResolveDependencyElement(reader, component);
							break;
						default:
							throw new ThemeException("Invalid '" + reader.Name + "' of element in this '" + theme.Name + "' theme.");
					}
				}
			}

			theme.Components.Add(component);
		}

		private void ResolveDependencyElement(XmlReader reader, ComponentElement component)
		{
			int depth = reader.Depth;

			while(reader.Read())
			{
				if(depth == reader.Depth)
					return;

				if(reader.NodeType == XmlNodeType.Element)
				{
					if(reader.Name == "dependency")
						component.Dependencies.Add(new DependencyElement(reader.GetAttribute("name")));
					else
						throw new ThemeException("Invalid '" + reader.Name + "' of element in this '" + component.Name + "' component.");
				}
			}
		}

		private void ResolveContentElement(XmlReader reader, Theme theme)
		{
		}
		#endregion

		#region 静态方法
		public static ThemeResolver GetResolver(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			path = path.Trim();

			if(!Directory.Exists(path))
				return null;

			if(_resolvers == null)
				System.Threading.Interlocked.CompareExchange(ref _resolvers, new ConcurrentDictionary<string, ThemeResolver>(StringComparer.OrdinalIgnoreCase), null);

			return _resolvers.GetOrAdd(path.Trim(), key => new ThemeResolver(key));
		}
		#endregion
	}
}
