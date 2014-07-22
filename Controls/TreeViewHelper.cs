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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Zongsoft.Web.Controls
{
	public static class TreeViewHelper
	{
		private static Dictionary<string, PropertyDescriptor> _properties;

		public static MvcHtmlString TreeView(this HtmlHelper html, object node, string childrenPropertyName, string textPropertyName, string url)
		{
			return MvcHtmlString.Create(GenerateTreeView(node, childrenPropertyName, textPropertyName, url));
		}

		public static string GenerateTreeView(object node, string childrenPropertyName, string textPropertyName, string url)
		{
			StringBuilder builder = new StringBuilder();

			GenerateTreeView(node, childrenPropertyName, url, textPropertyName, builder);

			return builder.ToString();
		}

		private static void GenerateTreeView(object node, string childrenPropertyName, string textPropertyName, string url, StringBuilder builder)
		{
			if(node == null)
				throw new ArgumentNullException("node");

			PropertyDescriptor textProperty = GetProperty(node.GetType(), textPropertyName);

			builder.Append("<ul>");
			builder.Append("<li>");
			builder.AppendFormat("<a href='{1}' alt=''>{0}</a>", textProperty.GetValue(node), url);

			PropertyDescriptor childrenProperty = GetProperty(node.GetType(), childrenPropertyName);
			if(childrenPropertyName == null)
				throw new InvalidOperationException("");

			IEnumerable children = childrenProperty.GetValue(node) as IEnumerable;
			if(children == null)
				throw new InvalidOperationException();

			builder.Append("<ul>");

			foreach(object child in children)
			{
				if(child == null)
					continue;

				GenerateTreeView(child, childrenPropertyName, url, textPropertyName, builder);
			}

			builder.Append("</ul>");
			builder.Append("</li>");
			builder.Append("</ul>");
		}

		internal static object GetPropertyValue(object target, string propertyName)
		{
			PropertyDescriptor property = GetProperty(target, propertyName);

			if(property == null)
				throw new ArgumentException(string.Format("The '{0}' property is not exits.", propertyName));

			return property.GetValue(target);
		}

		internal static PropertyDescriptor GetProperty(object target, string propertyName)
		{
			if(target == null)
				throw new ArgumentNullException("target");

			if(string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName");

			string key = target.GetType().AssemblyQualifiedName + "@" + propertyName;
			System.Threading.Interlocked.Exchange(ref _properties, new Dictionary<string, PropertyDescriptor>());

			if(_properties.ContainsKey(key))
				return _properties[key];

			PropertyDescriptor property = TypeDescriptor.GetProperties(target).Find(propertyName, true);
			if(property != null)
				_properties[key] = property;

			return property;
		}

		internal static PropertyDescriptor GetProperty(Type type, string propertyName)
		{
			if(type == null)
				throw new ArgumentNullException("type");
			if(string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName");

			string key = type.AssemblyQualifiedName + "@" + propertyName;
			System.Threading.Interlocked.Exchange(ref _properties, new Dictionary<string, PropertyDescriptor>());

			if(_properties.ContainsKey(key))
				return _properties[key];

			PropertyDescriptor property = TypeDescriptor.GetProperties(type).Find(propertyName, true);
			if(property != null)
				_properties[key] = property;

			return property;
		}
	}
}
