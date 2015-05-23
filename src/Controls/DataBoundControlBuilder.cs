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
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class DataBoundControlBuilder : ControlBuilder
	{
		public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, IDictionary attribs)
		{
			IDictionary attributes = new Hashtable(attribs.Count);

			foreach(DictionaryEntry entry in attribs)
			{
				var attributeName = (string)entry.Key;

				if(string.Equals(attributeName, "ID", StringComparison.OrdinalIgnoreCase))
				{
					if(string.IsNullOrEmpty(id))
						id = (string)entry.Value;

					if(Environment.OSVersion.Platform == PlatformID.Unix ||
					   Environment.OSVersion.Platform == PlatformID.MacOSX)
					{
						attributes["$ID"] = id;
						continue;
					}
				}

				var property = type.GetProperty(attributeName, (BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase));

				if(property != null && property.IsDefined(typeof(BindableAttribute), true))
				{
					var attribute = Attribute.GetCustomAttribute(property, typeof(PropertyMetadataAttribute), true);

					if(attribute != null)
					{
						if(((PropertyMetadataAttribute)attribute).Bindable)
							attributeName = "$" + attributeName;
					}
					else
					{
						attribute = Attribute.GetCustomAttribute(property, typeof(BindableAttribute), true);

						if(attribute != null && ((BindableAttribute)attribute).Bindable)
							attributeName = "$" + attributeName;
					}
				}

				attributes[attributeName] = entry.Value;
			}

			base.Init(parser, parentBuilder, type, tagName, id, attributes);
		}

		/*
		 * 本注释代码在 .NET on Windows 中工作良好，但是在 Mono on Linux 中无法正常工作。
		public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, IDictionary attribs)
		{
			IDictionary<string, BindingEntry> bindingEntiries = new Dictionary<string, BindingEntry>(StringComparer.OrdinalIgnoreCase);
			IDictionary attributes = new Hashtable(attribs.Count);

			foreach(DictionaryEntry entry in attribs)
			{
				string key = (string)entry.Key;
				var property = type.GetProperty(key, (BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase));

				if(property != null && property.IsDefined(typeof(BindableAttribute), true))
				{
					if(property.GetCustomAttributes(typeof(BindableAttribute), true).Cast<BindableAttribute>().FirstOrDefault().Bindable)
					{
						if(property.PropertyType == typeof(object))
							key = "$" + key;

						bindingEntiries.Add(key, new BindingEntry((string)entry.Value, property.PropertyType));
						attributes[key] = GetDefaultValue(property);
					}
					else
					{
						attributes[key] = entry.Value;
					}
				}
				else
				{
					attributes[key] = entry.Value;
				}
			}

			base.Init(parser, parentBuilder, type, tagName, id, attributes);
			var propertyEntries = this.GetObjectPersistData().AllPropertyEntries;

			foreach(var bindingEntry in bindingEntiries)
			{
				foreach(PropertyEntry propertyEntry in propertyEntries)
				{
					if(string.Equals(bindingEntry.Key, propertyEntry.Name))
					{
						SimplePropertyEntry simpleProperty = propertyEntry as SimplePropertyEntry;

						if(simpleProperty != null)
						{
							if(simpleProperty.Name.StartsWith("$"))
								simpleProperty.Name = simpleProperty.Name.Substring(1);

							simpleProperty.PersistedValue = bindingEntry.Value.PersistedValue;
							simpleProperty.Value = bindingEntry.Value.PersistedValue;
							simpleProperty.Type = bindingEntry.Value.ValueType;
							simpleProperty.UseSetAttribute = true;
						}
					}
				}
			}
		}

		private static object GetDefaultValue(PropertyInfo property)
		{
			DefaultValueAttribute[] attributes = (DefaultValueAttribute[])property.GetCustomAttributes(typeof(DefaultValueAttribute), true);

			if(attributes != null && attributes.Length > 0 && attributes[0].Value != null)
				return attributes[0].Value;

			if(property.PropertyType == typeof(string))
				return string.Empty;

			return Activator.CreateInstance(property.PropertyType);
		}

		private struct BindingEntry
		{
			internal BindingEntry(string persistedValue, Type valueType)
			{
				this.PersistedValue = persistedValue;
				this.ValueType = valueType;
			}

			public string PersistedValue;
			public Type ValueType;
		}
		*/
	}
}
